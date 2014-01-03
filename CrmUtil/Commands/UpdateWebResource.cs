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
        [Option('d', "directory", DefaultValue = ".", Required = false, HelpText = "Input directory to be processed.")]
        public string Directory { get; set; }

        [OptionArray('f', "filters", DefaultValue = new string[] { "*.html", "*.htm", "*.css", "*.js", "*.gif", "*.png", "*.jpg" }, HelpText = "Set of wildcard patterns.")]
        public string[] Filters { get; set; }

        [Option('m', "monitor", Required = false, HelpText = "Monitor a file or directory for changes.")]
        public bool Monitor { get; set; }

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

    class UpdateWebResource : CommandBase
    {
        public UpdateWebResource(UpdateWebResourceOptions options) : base(options)
        {
        }

        public override void Execute() //UpdateWebResourceOptions options
        {
            var options = (UpdateWebResourceOptions)Options;
            if (options.Debug) System.Diagnostics.Debugger.Launch();

            if (string.IsNullOrEmpty(options.Directory)) // || options.Directory == "."
            {
                options.Directory = Environment.CurrentDirectory;
            }

            var validFiles = FindFiles(options);

            if (options.Monitor)
            {
                var watcher = new FileSystemWatcher();
                watcher.IncludeSubdirectories = options.Recursive;
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                watcher.Path = options.Directory;
                watcher.EnableRaisingEvents = true;

                var lastReadTime = DateTime.MinValue;
                Action<object, FileSystemEventArgs> updater = (source, e) =>
                {
                    var file = new FileInfo(e.FullPath);
                    if (e.ChangeType == WatcherChangeTypes.Deleted) {
                        return;
                    }

                    if (validFiles.FirstOrDefault(i => i.Name.ToLower() == file.Name.ToLower()) == null)
                    {
                        return;
                    }

                    if ((file.LastWriteTime - lastReadTime).TotalSeconds > 1)
                    {
                        // Some editors need some time to save the file completely, wait 100 ms here
                        System.Threading.Thread.Sleep(100);
                        Console.WriteLine(string.Format("{0} {1}", e.FullPath, e.ChangeType));
                        //Console.WriteLine(string.Format("{0:s}  --  {1} {2}", lastWriteTime, filepath, e.ChangeType));
                        //Console.WriteLine(string.Format("read: {0:s} -- write: {1:s}", lastReadTime, lastWriteTime));
                        var result = UpdateSingleResource(file, options);
                        if (result && !options.NoPublish) PublishAllCustomizations();
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
                UpdateResources(options);                
            }
        }

        private void UpdateResources(UpdateWebResourceOptions options)
        {
            var files = FindFiles(options);

            var result = false;
            foreach (var file in files)
            {
                result = UpdateSingleResource(file, options) || result;
            }

            if (result && !options.NoPublish)
            {
                PublishAllCustomizations();
            }
        }

        private List<FileInfo> FindFiles(UpdateWebResourceOptions options)
        {
            var ret = new List<FileInfo>();

            var validPatterns = new List<string>();
            foreach (var pat in options.Filters)
            {
                var p = "^" + pat.Replace("?", @"(\w?)").Replace("*", @"(\w*?)").Replace(".", @"\.") + "$";
                //Console.WriteLine(p);
                validPatterns.Add(p);
            }

            var soption = SearchOption.TopDirectoryOnly;
            if (options.Recursive)
            {
                soption = soption | SearchOption.AllDirectories;
            }
            
            var files = Directory.GetFiles(options.Directory, "*.*", soption);
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

        private bool UpdateSingleResource(FileInfo file, UpdateWebResourceOptions options)
        {
            var ret = true;
            try
            {
                var name = file.Name;
                var fileBytes = File.ReadAllBytes(file.FullName);
                var resource = Context.CreateQuery("webresource").FirstOrDefault(i => (string)i["name"] == name);

                var nresource = new Entity("webresource");
                nresource.Attributes["name"] = name;
                nresource.Attributes["description"] = name;
                //resource.Attributes["logicalname"] = name;
                nresource.Attributes["displayname"] = name;
                nresource.Attributes["content"] = Convert.ToBase64String(fileBytes);
                nresource.Attributes["webresourcetype"] = new OptionSetValue(GetWebResourceType(file));

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

                Console.Write("Updating {0} ({1}) ... ", name, file.FullName);
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

            return 4;
        }

    }

}
