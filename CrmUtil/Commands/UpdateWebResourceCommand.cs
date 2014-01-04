using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using CrmUtil.Configuration;
using CrmUtil.Logging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmUtil.Commands
{
    public class UpdateWebResourceOptions : CrmCommonOptions
    {
        [Option('p', "path", DefaultValue = ".", Required = false, HelpText = "Input directory to be processed.")]
        public string Path { get; set; }

        [OptionArray('f', "filters", DefaultValue = new string[] { "*.html", "*.htm", "*.css", "*.js", "*.gif", "*.png", "*.jpg", "*.xml", "*.zap" }, HelpText = "Set of wildcard patterns.")]
        public string[] Filters { get; set; }

        [Option("threads", Required = false, DefaultValue = 5, HelpText = "Number of threads to use when uploading web resources.")]
        public int Threads { get; set; }

        [Option('w', "watch", Required = false, DefaultValue = false, HelpText = "Monitor directory for changes.")]
        public bool Watch { get; set; }

        [Option("force", Required = false, DefaultValue = false, HelpText = "Update the resource even if the extension is not supported.")]
        public bool Force { get; set; }

        [Option('r', "recursive", Required = false, HelpText = "Include files in sub directories.")]
        public bool Recursive { get; set; }

        [Option("nopublish", Required = false, DefaultValue = false, HelpText = "Do not publish customizations.")]
        public bool NoPublish { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public override Type GetCommandType()
        {
            return typeof(UpdateWebResourceCommand);
        }
    }

    class UpdateWebResourceCommand : CrmCommandBase<UpdateWebResourceOptions>
    {
        private List<FileInfo> _files;

        public UpdateWebResourceCommand(IConfigurationProvider configurationProvider, LoggerBase logger, UpdateWebResourceOptions options)
            : base(configurationProvider, logger, options)
        {
        }

        public override void Execute() //UpdateWebResourceOptions options
        {
            if (string.IsNullOrEmpty(Options.Path) || Options.Path == ".")
            {
                Options.Path = Environment.CurrentDirectory;
            }
            else
            {
                Options.Path = new DirectoryInfo(Options.Path).FullName;
            }

            Logger.Write("Path", Options.Path);

            _files = FindFiles();
            if (_files == null || _files.Count == 0)
            {
                Logger.Write(BaseName, "No matching files, exiting.");
                return;
            }

            Logger.Write("File", "Count: {0:N0}".Compose(_files.Count));
            foreach (var file in _files)
            {
                Logger.Write("File", GetRelativePath(file, Options.Path));
            }

            WarmupCrmService();

            if (Options.Watch)
            {
                var watcher = new FileSystemWatcher();
                watcher.IncludeSubdirectories = Options.Recursive;
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                watcher.Path = Options.Path;
                watcher.EnableRaisingEvents = true;

                var lastReadTime = DateTime.MinValue;
                Action<object, FileSystemEventArgs> updater = (source, e) =>
                {
                    var file = new FileInfo(e.FullPath);
                    if (e.ChangeType == WatcherChangeTypes.Deleted) {
                        return;
                    }

                    if (_files.FirstOrDefault(i => i.Name.ToLower() == file.Name.ToLower()) == null)
                    {
                        // File is not a target
                        return;
                    }

                    if ((file.LastWriteTime - lastReadTime).TotalSeconds > 1)
                    {
                        // Some editors need some time to save the file completely, wait 100 ms
                        System.Threading.Thread.Sleep(100);
                        Logger.Write(e.ChangeType.ToString(), GetRelativePath(file, Options.Path));
                        //Console.WriteLine(string.Format("{0:s}  --  {1} {2}", lastWriteTime, filepath, e.ChangeType));
                        //Console.WriteLine(string.Format("read: {0:s} -- write: {1:s}", lastReadTime, lastWriteTime));
                        var result = UpdateSingleResource(file);
                        if (result && !Options.NoPublish) PublishAllCustomizations();
                        lastReadTime = file.LastWriteTime;
                    }
                };

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(updater);
                watcher.Created += new FileSystemEventHandler(updater);
                watcher.Deleted += new FileSystemEventHandler(updater);
                watcher.Renamed += new RenamedEventHandler(updater);

                Logger.Write("Waiting for changes (q<enter> to quit) ...");
                while (Console.Read() != 'q') ; 
            }
            else
            {
                UpdateResources();                
            }
        }

        private void UpdateResources()
        {
            var result = false;
            var index = 1;
            var threadList = new List<Tuple<FileInfo, int, bool>>();

            foreach (var file in _files)
            {
                threadList.Add(new Tuple<FileInfo, int, bool>(file, index++, false));
            }

            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Options.Threads };

            Parallel.ForEach(threadList, parallelOptions, (info) => {
               result = UpdateSingleResource(info.Item1, info.Item2) || result;
            });

            if (result && !Options.NoPublish)
            {
                PublishAllCustomizations();
            }
        }

        private List<FileInfo> FindFiles()
        {
            var ret = new List<FileInfo>();

            var validPatterns = new List<string>();
            foreach (var pat in Options.Filters)
            {
                var p = "^" + pat.Replace("?", @"([\w\W]?)").Replace("*", @"([\w\W]*?)").Replace(".", @"\.") + "$";
                //Console.WriteLine(p);
                validPatterns.Add(p);
            }

            var soption = SearchOption.TopDirectoryOnly;
            if (Options.Recursive)
            {
                soption = soption | SearchOption.AllDirectories;
            }

            var files = Directory.GetFiles(Options.Path, "*.*", soption);
            foreach (var file in files)
            {
                var f = new FileInfo(file);
                foreach (var pat in validPatterns)
                {
                    if (Regex.IsMatch(f.Name, pat, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                    {
                        ret.Add(f);
                        break;
                    }
                }
            }

            //ret.ForEach(i => Console.WriteLine(i.FullName));
            return ret;
        }

        private bool UpdateSingleResource(FileInfo file, int? index = null)
        {
            var ret = true;
            string category = null;
            var log = new StringBuilder();

            try
            {
                category = null;
                var type = GetWebResourceType(file);
                if (type == 0)
                {
                    if (Options.Force)
                    {
                        type = 4; // XML Data
                    }
                    else
                    {
                        Logger.Write(log, "Skip", "{0} :: Invalid extension".Compose(GetRelativePath(file, Options.Path)));
                        return false;
                    }                    
                }

                var name = file.Name;
                var fileBytes = File.ReadAllBytes(file.FullName);
                var resource = CrmContext.CreateQuery("webresource").FirstOrDefault(i => (string)i["name"] == name);

                var nresource = new Entity("webresource");
                nresource.Attributes["name"] = name;
                nresource.Attributes["description"] = name;
                //resource.Attributes["logicalname"] = name;
                nresource.Attributes["displayname"] = name;
                nresource.Attributes["content"] = Convert.ToBase64String(fileBytes);
                nresource.Attributes["webresourcetype"] = new OptionSetValue(type);
                
                OrganizationRequest request;
                if (resource != null)
                {
                    category = "({0:N0}) Update";
                    nresource.Id = resource.Id;
                    request = new UpdateRequest() { Target = nresource };
                }
                else
                {
                    category = "({0:N0}) Create";
                    request = new CreateRequest() { Target = nresource };
                }
                
                if (!string.IsNullOrEmpty(category) && index.HasValue)
                {
                    category = category.Compose(index);
                }
                Logger.Write(log, category, "{0} ... ".Compose(GetRelativePath(file, Options.Path)));
                CrmService.Execute(request);
                Logger.Write(log, category, "Done.");
            }
            catch (Exception ex)
            {
                Logger.Write(log, !string.IsNullOrEmpty(category) ? category : BaseName, ex);
                ret = false;
            }

            Logger.Write(log.ToString());

            return ret;
        }

        private int GetWebResourceType(FileInfo file)
        {
            var ext = file.Extension.ToLower();

            if (ext == ".html" || ext == ".htm")
            {
                return 1;
            }
            else if (ext == ".css")
            {
                return 2;
            }
            else if (ext == ".js")
            {
                return 3;
            }
            else if (ext == ".xml")
            {
                return 4;
            }
            else if (ext == ".png")
            {
                return 5;
            }
            else if (ext == ".jpg")
            {
                return 6;
            }
            else if (ext == ".gif")
            {
                return 7;
            }
            else if (ext == ".xap")
            {
                return 8;
            }
            else if (ext == ".xsl" || ext == ".xslt")
            {
                return 9;
            }
            else if (ext == ".ico")
            {
                return 10;
            }

            return 0;
        }

    }

}
