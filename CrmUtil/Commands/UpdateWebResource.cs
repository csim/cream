using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmUtil.Commands
{
    public class UpdateWebResourceOptions : CommonOptions
    {
        [Option('p', "path", DefaultValue = ".", Required = false, HelpText = "Input directory to be processed.")]
        public string Path { get; set; }

        [OptionArray('f', "filters", DefaultValue = new string[] { "*.html", "*.htm", "*.css", "*.js", "*.gif", "*.png", "*.jpg", "*.xml", "*.zap" }, HelpText = "Set of wildcard patterns.")]
        public string[] Filters { get; set; }

        [Option('w', "watch", Required = false, DefaultValue = false, HelpText = "Monitor directory for changes.")]
        public bool Watch { get; set; }

        [Option("force", Required = false, DefaultValue = false, HelpText = "Update the resource even if the extension is not supported.")]
        public bool Force { get; set; }

        [Option('r', "recursive", Required = false, HelpText = "Include files in sub directories.")]
        public bool Recursive { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class UpdateWebResource : CommandBase<UpdateWebResourceOptions>
    {
        private List<FileInfo> _files;

        public UpdateWebResource(UpdateWebResourceOptions options) : base(options)
        {
        }

        public override void Execute() //UpdateWebResourceOptions options
        {
            if (Options.Debug) System.Diagnostics.Debugger.Launch();

            if (string.IsNullOrEmpty(Options.Path) || Options.Path == ".")
            {
                Options.Path = Environment.CurrentDirectory;
            }
            else
            {
                Options.Path = new DirectoryInfo(Options.Path).FullName;
            }

            _files = FindFiles();
            if (_files == null || _files.Count == 0)
            {
                Console.WriteLine("No matching files, exiting.");
                return;
            }

            Console.WriteLine("Target File{0}:", _files.Count == 1 ? "" : "s");
            foreach (var file in _files)
            {
                Console.WriteLine("{0}", GetRelativePath(file, Options.Path));
            }

            Console.WriteLine("");
            WarmupService();

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
                        return;
                    }

                    if ((file.LastWriteTime - lastReadTime).TotalSeconds > 1)
                    {
                        // Some editors need some time to save the file completely, wait 100 ms here
                        System.Threading.Thread.Sleep(100);
                        Console.WriteLine(string.Format("{0} {1}", GetRelativePath(e.FullPath, Options.Path), e.ChangeType));
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

                Console.WriteLine("Waiting for changes (q<enter> to quit)...");
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
            foreach (var file in _files)
            {
                result = UpdateSingleResource(file) || result;
            }

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
                var p = "^" + pat.Replace("?", @"(\w?)").Replace("*", @"(\w*?)").Replace(".", @"\.") + "$";
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

        private bool UpdateSingleResource(FileInfo file)
        {
            var ret = true;
            try
            {
                var type = GetWebResourceType(file);
                if (type == 0)
                {
                    if (Options.Force)
                    {
                        type = 4; // XML Data
                    }
                    else
                    {
                        Console.WriteLine("{0} :: Skipping, Invalid extension.", GetRelativePath(file, Options.Path));
                        return false;
                    }                    
                }

                var name = file.Name;
                var fileBytes = File.ReadAllBytes(file.FullName);
                var resource = Context.CreateQuery("webresource").FirstOrDefault(i => (string)i["name"] == name);

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
                    nresource.Id = resource.Id;
                    request = new UpdateRequest() { Target = nresource };
                }
                else
                {
                    request = new CreateRequest() { Target = nresource };
                }

                Console.Write("{0} Updating ({1}) ... ", GetRelativePath(file, Options.Path), file.Name);
                Service.Execute(request);
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ret = false;
            }

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
            else if (ext == ".xsl")
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
