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
using CrmUtil.Configuration;
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

        public Program(IConfigurationProvider configurationProvider, LoggerBase logger)
        {
            Configuration = new DefaultConfigurationProvider();
            Logger = new DefaultLogger(Configuration);
        }

        public ICommand GetCommand(string verb, object options)
        {
            if (options == null) return null;
            if (options is UpdateWebResourceOptions)
            {
                return new UpdateWebResourceCommand(Configuration, Logger, (UpdateWebResourceOptions)options);
            }
            else if (options is PublishCustomizationsOptions)
            {
                return new PublishCustomizationsCommand(Configuration, Logger, (PublishCustomizationsOptions)options);
            }

            return null;
        }

        static void Main(string[] args)
        {
            var configurationProvider = new DefaultConfigurationProvider();
            var logger = new DefaultLogger(configurationProvider);

            var prog = new Program(configurationProvider, logger);
            prog.Execute(args);
        }

        private void Execute(string[] args)
        {
            var logCategory = ApplicationInfo.Title;

            Console.WriteLine("");
            try
            {
                //var assembly = Assembly.GetExecutingAssembly();
                //var version = Assembly.GetExecutingAssembly().GetName().Version;
                //var varsionDate = new DateTime(2000, 01, 01).AddDays(version.Build).AddSeconds(version.Revision * 2);
                //Console.WriteLine(date.ToString("s"));
                var startTime = DateTime.Now;
                var options = new ProgramOptions();
                using (Logger)
                {
                    CommandLine.Parser.Default.ParseArguments(args, options,
                      (verb, subOptions) =>
                      {
                          try
                          {
                              var command = GetCommand(verb, subOptions);
                              if (command != null)
                              {
                                  Logger.Write(logCategory, "v{0}".Compose(ApplicationInfo.Version));
                                  Logger.Write(logCategory, "{0}".Compose(ApplicationInfo.Copyright));
                                  //Logger.Write("Start", "{0:s}".Compose(startTime));
                                  command.Execute();
                                  var duration = (DateTime.Now - startTime);
                                  Logger.Write("Duration", "{0:00}:{1:00}:{2:00}".Compose(duration.TotalHours, duration.Minutes, duration.Seconds));
                              }
                          }
                          catch (Exception ex)
                          {
                              Logger.Write(logCategory, ex.Format());
                          }
                      });
                }

            }
            catch (Exception ex)
            {
                Logger.Write(logCategory, ex.Format());
            }
            Console.WriteLine("");
        }
    }
}
