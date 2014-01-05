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

namespace CrmUtil.Commands.Crm
{
    public abstract class UpdateResourceCommandBaseOptions : CrmCommonOptionBase
    {
        [Option('p', "path", DefaultValue = ".", Required = false, HelpText = "Input directory to be processed.")]
        public string Path { get; set; }

        public abstract string[] Filters { get; set; }

        [Option("threads", Required = false, DefaultValue = 5, HelpText = "Number of threads to use when uploading web resources.")]
        public int Threads { get; set; }

        [Option('w', "watch", Required = false, DefaultValue = false, HelpText = "Monitor directory for changes.")]
        public bool Watch { get; set; }

        [Option("force", Required = false, DefaultValue = false, HelpText = "Update the resource even if the local file is older or the extension is not supported.")]
        public bool Force { get; set; }

        [Option('r', "recursive", Required = false, HelpText = "Include files in sub directories.")]
        public bool Recursive { get; set; }

        [Option("nopublish", Required = false, DefaultValue = false, HelpText = "Do not publish customizations.")]
        public bool NoPublish { get; set; }
    }

    public abstract class ResourceCommandBase<TOptions> : CrmCommandBase<TOptions> where TOptions : UpdateResourceCommandBaseOptions
    {
        private List<FileInfo> _files;

        public ResourceCommandBase(IConfigurationProvider configurationProvider, LoggerBase logger, TOptions options)
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
                Logger.Write("Exit", "No matching files. ({0})".Compose(string.Join(" " , Options.Filters)));
                return;
            }

            foreach (var file in _files)
            {
                Logger.Write("File", GetRelativePath(file, Options.Path));
            }

            Logger.Write("Count", "{0:N0}".Compose(_files.Count));

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
                    if (_files.FirstOrDefault(i => i.Name.ToLower() == file.Name.ToLower()) == null)
                    {
                        // File is not a target
                        return;
                    }

                    // Some editors may trigger the event twice in quick succession
                    // make sure that the last read time was more than 1 second ago
                    if ((file.LastWriteTime - lastReadTime).TotalSeconds > 1)
                    {
                        // Some editors need some time to save the file completely, wait 100 ms
                        System.Threading.Thread.Sleep(100);
                        Logger.Write(e.ChangeType.ToString(), GetRelativePath(file, Options.Path));
                        //Console.WriteLine(string.Format("{0:s}  --  {1} {2}", lastWriteTime, filepath, e.ChangeType));
                        //Console.WriteLine(string.Format("read: {0:s} -- write: {1:s}", lastReadTime, lastWriteTime));
                        var result = UpdateSingle(file);
                        if (result && !Options.NoPublish) PublishAllCustomizations();
                        lastReadTime = file.LastWriteTime;
                    }
                };

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(updater);
                //watcher.Created += new FileSystemEventHandler(updater);
                //watcher.Deleted += new FileSystemEventHandler(updater);
                //watcher.Renamed += new RenamedEventHandler(updater);

                Logger.Write("Waiting", "(q<enter> to quit or CTRL-C)");
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
               result = UpdateSingle(info.Item1, info.Item2) || result;
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

        protected abstract bool UpdateSingle(FileInfo file, int? index = null);

    }

}
