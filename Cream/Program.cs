using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Cream.Commands;
using Cream.Providers;
using Cream.Logging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Ninject;

namespace Cream
{
    public class ProgramOptions
    {
        public ProgramOptions()
        {
            PublishCustomizationsVerb = new PublishCustomizationsOptions();
            RegisterWebResourceVerb = new RegisterWebResourceOptions();
            RegisterAssemblyVerb = new RegisterAssemblyOptions();
        }

        [VerbOption("PublishCustomizations", HelpText = "Publish all CRM customizations.")]
        public PublishCustomizationsOptions PublishCustomizationsVerb { get; set; }

        [VerbOption("RegisterWebResource", HelpText = "Add or Update WebResources.")]
        public RegisterWebResourceOptions RegisterWebResourceVerb { get; set; }

        [VerbOption("RegisterAssembly", HelpText = "Add or Update Plugin Assemblies.")]
        public RegisterAssemblyOptions RegisterAssemblyVerb { get; set; }

        [VerbOption("RegisterStep", HelpText = "Add or Update Plugin Step.")]
        public RegisterStepOptions RegisterStepVerb { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            //var version = Assembly.GetExecutingAssembly().GetName().Version;
            //var date = new DateTime(2000, 01, 01).AddDays(version.Build).AddSeconds(version.Revision * 2);

            //var help = new HelpText
            //{
            //    Heading = new HeadingInfo("Cream", version.ToString()),
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

    public interface IProgram {
        void Execute(string[] args);
    }

    public class Program : IProgram
    {
        public ApplicationInfo App { get; private set; }

        public CommandFactory Factory { get; private set; }

        public IKernel Resolver { get; private set; }


        public LoggerBase Logger { get; private set; }

        public Program(CommandFactory factory, IKernel resolver)
        {
            Logger = resolver.Get<LoggerBase>();
            Factory = factory;
            Resolver = resolver;
            App = new ApplicationInfo();
        }

        static void Main(string[] args)
        {
            if (args.Contains("--debug")) System.Diagnostics.Debugger.Launch();

            var factory = new CommandFactory();
            factory.Bind();
            var prog = new Program(factory, factory.Kernel);
            prog.Execute(args);
        }

        public void Execute(string[] args)
        {
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
                    //Console.WriteLine(ex.Format());
                }
                Console.WriteLine("");
            }
        }

        private void ExecuteCommand(string verb, object options)
        {
            if (options == null) return;
            try
            {
                var startTime = DateTime.Now;
                var command = Factory.GetCommand((OptionBase)options);
                if (command != null)
                {
                    Logger.Write(App.Title, "v{0}".Compose(App.Version));
                    Logger.Write(App.Title, "{0}".Compose(App.Copyright));
                    //Logger.Write("Start", "{0:s}".Compose(startTime));
                    command.Execute();
                    var duration = (DateTime.Now - startTime);
                    Logger.Write("Duration", "{0:0}:{1:00}:{2:00}".Compose(duration.TotalHours, duration.Minutes, duration.Seconds));
                }
            }
            catch (Exception ex)
            {
                Logger.Write(App.Title, ex.Format());
            }
        }
    }
}
