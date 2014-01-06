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
using CrmUtil.Commands.Crm;
using CrmUtil.Providers;
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
            UpdateAssemblyVerb = new UpdateAssemblyOptions();
        }

        [VerbOption("PublishCustomizations", HelpText = "Publish all CRM customizations.")]
        public PublishCustomizationsOptions PublishCustomizationsVerb { get; set; }

        [VerbOption("UpdateWebResource", HelpText = "Update WebResources.")]
        public UpdateWebResourceOptions UpdateWebResourceVerb { get; set; }

        [VerbOption("UpdateAssembly", HelpText = "Update Plugin Assembly.")]
        public UpdateAssemblyOptions UpdateAssemblyVerb { get; set; }

        [VerbOption("UpdateStep", HelpText = "Update Plugin Step.")]
        public UpdateStepOptions UpdateStepVerb { get; set; }

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

        public ApplicationInfo App { get; private set; }

        public Program(IConfigurationProvider configurationProvider, LoggerBase logger, CommandFactory factory)
        {
            Configuration = configurationProvider;
            Logger = logger;
            Factory = factory;
            App = new ApplicationInfo();
        }

        static void Main(string[] args)
        {
            var factory = new CommandFactory();
            var configurationProvider = factory.GetDependency<IConfigurationProvider>();
            var logger = factory.GetDependency<LoggerBase>();

            var prog = new Program(configurationProvider, logger, factory);
            prog.Execute(args);
        }

        private void Execute(string[] args)
        {
            if (args.Contains("--debug")) System.Diagnostics.Debugger.Launch();
            var logCategory = App.Title;

            using (Logger)
            {
                Console.WriteLine("");
                try
                {
                    var options = new ProgramOptions();
                    var parser = new Parser((p) => {
                            p.MutuallyExclusive = true;
                            p.CaseSensitive = false;
                            p.HelpWriter = Console.Error;
                    });
                    parser.ParseArguments(args, options, ExecuteCommand);
                }
                catch (Exception ex)
                {
                    Logger.Write(logCategory, ex.Format());
                }
                Console.WriteLine("");
            }
        }

        private void ExecuteCommand(string verb, object options)
        {
            if (options == null) return;

            var logCategory = App.Title;
            try
            {
                var startTime = DateTime.Now;
                var command = Factory.GetCommand((CommonOptionsBase)options);
                if (command != null)
                {
                    Logger.Write(logCategory, "v{0}".Compose(App.Version));
                    Logger.Write(logCategory, "{0}".Compose(App.Copyright));
                    //Logger.Write("Start", "{0:s}".Compose(startTime));
                    command.Execute();
                    var duration = (DateTime.Now - startTime);
                    Logger.Write("Duration", "{0:0}:{1:00}:{2:00}".Compose(duration.TotalHours, duration.Minutes, duration.Seconds));
                }
            }
            catch (Exception ex)
            {
                Logger.Write(logCategory, ex.Format());
            }
        }
    }
}
