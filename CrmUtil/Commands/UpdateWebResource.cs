using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        [Option('f', "filename", Required = false, HelpText = "Input file to be processed.")]
        public string Filename { get; set; }

        [Option('d', "directory", DefaultValue = ".", Required = false, HelpText = "Input directory to be processed.")]
        public string Directory { get; set; }

        [OptionArray('p', "patterns", DefaultValue = new string[] { "*.html", "*.htm", "*.css", "*.js", "*.gif", "*.png", "*.jpg" }, HelpText = "Set of wildcard patterns.")]
        public string[] Patterns { get; set; }

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

            if (string.IsNullOrEmpty(options.Directory))
            {
                options.Directory = Environment.CurrentDirectory;
            }

            if (options.Monitor)
            {
                var watcher = new FileSystemWatcher();
                watcher.IncludeSubdirectories = options.Recursive;
                watcher.Path = options.Directory;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
                watcher.EnableRaisingEvents = true;

                var lastReadTime = DateTime.MinValue;
                var validExtensions = new List<string>();
                foreach (var pat in options.Patterns)
                {
                    if (pat.IndexOf(".") >= 0)
                    {
                        validExtensions.Add("." + pat.Split('.')[1].ToLower());
                    }
                }

                Action<object, FileSystemEventArgs> updater = (source, e) =>
                {
                    var ext = Path.GetExtension(e.FullPath);
                    if (!validExtensions.Contains(ext.ToLower()))
                    {
                        return;
                    }

                    var lastWriteTime = File.GetLastWriteTime(e.FullPath);
                    if ((lastWriteTime - lastReadTime).TotalSeconds > 1)
                    {
                        Console.WriteLine(string.Format("{0} {1}", e.FullPath, e.ChangeType));
                        //Console.WriteLine(string.Format("{0:s}  --  {1} {2}", lastWriteTime, e.FullPath, e.ChangeType));
                        //Console.WriteLine(string.Format("read: {0:s} -- write: {1:s}", lastReadTime, lastWriteTime));
                        UpdateSingleResource(e.FullPath, options);

                        if (!options.NoPublish) PublishAllCustomizations();

                        lastReadTime = lastWriteTime;
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
                UpdateAllResources(options);                
            }
        }

        private void UpdateAllResources(UpdateWebResourceOptions options)
        {
            List<string> files = new List<string>();

            if (!string.IsNullOrEmpty(options.Filename))
            {
                files.Add(options.Filename);
            }
            else
            {
                FindFiles(options.Directory, files, options);
            }

            foreach (var file in files)
            {
                UpdateSingleResource(file, options);
            }

            if (!options.NoPublish)
            {
                PublishAllCustomizations();
            }
        }

        private void FindFiles(string dir, List<string> files, UpdateWebResourceOptions options)
        {
            foreach (var pat in options.Patterns)
            {
                files.AddRange(Directory.GetFiles(dir, pat));
            }
            
            if (options.Recursive)
            {
                foreach (var idir in Directory.GetDirectories(dir))
                {
                    FindFiles(idir, files, options);
                }
            }
        }

        private void UpdateSingleResource(string filePath, UpdateWebResourceOptions options)
        {
            var name = Path.GetFileName(filePath);
            var fileBytes = File.ReadAllBytes(filePath);
            var resource = Context.CreateQuery("webresource").FirstOrDefault(i => (string)i["name"] == name);

            var nresource = new Entity("webresource");
            nresource.Attributes["name"] = name;
            nresource.Attributes["description"] = name;
            //resource.Attributes["logicalname"] = name;
            nresource.Attributes["displayname"] = name;
            nresource.Attributes["content"] = Convert.ToBase64String(fileBytes);
            nresource.Attributes["webresourcetype"] = new OptionSetValue(3);

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

            Console.Write("Updating {0} ... ", filePath);
            Service.Execute(request);
            Console.WriteLine("Done.");
        }

    }
}
