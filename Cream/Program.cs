﻿using System;
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
            PublishCustomizationsVerb = new PublishCustomizationOption();
            RegisterWebResourceVerb = new RegisterWebResourceOption();
            RegisterAssemblyVerb = new RegisterAssemblyOption();
            SaveConnection = new SaveConnectionOption();
        }

        [VerbOption("PublishCustomizations", HelpText = "Publish all CRM customizations.")]
        public PublishCustomizationOption PublishCustomizationsVerb { get; set; }

        [VerbOption("RegisterWebResource", HelpText = "Add or Update WebResources.")]
        public RegisterWebResourceOption RegisterWebResourceVerb { get; set; }

        [VerbOption("RegisterAssembly", HelpText = "Add or Update Plugin Assemblies.")]
        public RegisterAssemblyOption RegisterAssemblyVerb { get; set; }

        [VerbOption("RegisterStep", HelpText = "Add or Update Plugin Step.")]
        public RegisterStepOption RegisterStepVerb { get; set; }

        [VerbOption("SaveConnection", HelpText = "Add or Update Plugin Step.")]
        public SaveConnectionOption SaveConnection { get; set; }

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

        public Program()
        {
            App = new ApplicationInfo();
        }

        static void Main(string[] args)
        {
            if (args.Contains("--debug")) System.Diagnostics.Debugger.Launch();

            var prog = new Program();
            prog.Execute(args);
        }

        public void Execute(string[] args)
        {
            var logCategory = App.Title;

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
                Console.WriteLine(ex.Format());
            }
            Console.WriteLine("");
        }

        private void ExecuteCommand(string verb, object options)
        {
            if (options == null || !(options is OptionBase)) return;
            var opt = (OptionBase)options;
            var factory = new CommandFactory(opt);
            var logger = factory.GetLogger();

            using (logger) {
                try
                {
                    var startTime = DateTime.Now;
                    var command = factory.GetCommand();
                    if (command != null)
                    {
                        logger.Write(App.Title, "v{0}".Compose(App.Version));
                        logger.Write(App.Title, "{0}".Compose(App.Copyright));
                        //Logger.Write("Start", "{0:s}".Compose(startTime));
                        command.Execute();
                        var duration = (DateTime.Now - startTime);
                        logger.Write("Duration", "{0:0}:{1:00}:{2:00}".Compose(duration.TotalHours, duration.Minutes, duration.Seconds));
                    }
                }
                catch (Exception ex)
                {
                    logger.Write(App.Title, ex.Format());
                }
            }
        }
    }
}
