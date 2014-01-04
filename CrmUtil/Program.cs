using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using CrmUtil.Commands;
using CrmUtil.Logging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmUtil
{
    public class ProgramOptions
    {
        public ProgramOptions()
        {
            PublishCustomizationsVerb = new PublishCustomizationsOptions();
            UpdateWebResourceVerb = new UpdateWebResourceOptions();
        }

        [VerbOption("PublishCustomizations", HelpText = "Publish all CRM customizations.")]
        public PublishCustomizationsOptions PublishCustomizationsVerb { get; set; }

        [VerbOption("UpdateWebResource", HelpText = "Update WebResources.")]
        public UpdateWebResourceOptions UpdateWebResourceVerb { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            //var version = Assembly.GetExecutingAssembly().GetName().Version;
            //var date = new DateTime(2000, 01, 01).AddDays(version.Build).AddSeconds(version.Revision * 2);

            //var help = new HelpText
            //{
            //    Heading = new HeadingInfo("CrmUtil", version.ToString()),
            //    Copyright = new CopyrightInfo("<<app author>>", 2012),
            //    AdditionalNewLineAfterOption = true,
            //    AddDashesToOption = true
            //};
            //help.AddPreOptionsLine("<<license details here.>>");
            ////help.AddPreOptionsLine("Usage: app -pSomeone");
            //help.AddOptions(this);
            //return help;
            return HelpText.AutoBuild(this, verb);
        }
    }

    public class Program
    {
        public IConfigurationProvider Configuration { get; private set; }
        public LoggerBase Logger { get; private set; }
        public CommandFactory Factory { get; private set; }

        public string LogCategory { get; private set; }

        public Program(IConfigurationProvider configurationProvider, LoggerBase logger, CommandFactory factory)
        {
            Configuration = configurationProvider;
            Logger = logger;
            Factory = factory;
        }


        static void Main(string[] args)
        {
            var factory = new CommandFactory();
            var prog = new Program(factory.GetDependency<IConfigurationProvider>(), factory.GetDependency<LoggerBase>(), factory);
            prog.Execute(args);
        }

        private void Execute(string[] args)
        {
            if (args.Contains("--debug")) System.Diagnostics.Debugger.Launch();

            LogCategory = ApplicationInfo.Title;

            Console.WriteLine("");
            using (Logger)
            {
                try
                {
                    //var assembly = Assembly.GetExecutingAssembly();
                    //var version = Assembly.GetExecutingAssembly().GetName().Version;
                    //var varsionDate = new DateTime(2000, 01, 01).AddDays(version.Build).AddSeconds(version.Revision * 2);
                    //Console.WriteLine(date.ToString("s"));
                    var options = new ProgramOptions();
                    CommandLine.Parser.Default.ParseArguments(args, options, ExecuteCommand);
                }
                catch (Exception ex)
                {
                    Logger.Write(LogCategory, ex.Format());
                }
                Console.WriteLine("");
            }
        }

        private void ExecuteCommand(string verb, object options)
        {
            try
            {
                var startTime = DateTime.Now;
                var command = Factory.GetCommand(options);
                if (command != null)
                {
                    Logger.Write(LogCategory, "v{0}".Compose(ApplicationInfo.Version));
                    Logger.Write(LogCategory, "{0}".Compose(ApplicationInfo.Copyright));
                    //Logger.Write("Start", "{0:s}".Compose(startTime));
                    command.Execute();
                    var duration = (DateTime.Now - startTime);
                    Logger.Write("Duration", "{0:00}:{1:00}:{2:00}".Compose(duration.TotalHours, duration.Minutes, duration.Seconds));
                }
            }
            catch (Exception ex)
            {
                Logger.Write(LogCategory, ex.Format());
            }
        }
    }
}
