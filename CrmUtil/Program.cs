using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using CrmUtil.Commands;
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

        [VerbOption("PublishCustomizations", HelpText = "Update a WebResource.")]
        public PublishCustomizationsOptions PublishCustomizationsVerb { get; set; }

        [VerbOption("UpdateWebResource", HelpText = "Update a WebResource.")]
        public UpdateWebResourceOptions UpdateWebResourceVerb { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }

    class Program
    {
        public Program()
        {
        }

        public CommandBase GetCommand(string verb, object options)
        {
            if (options == null) return null;
            if (options is UpdateWebResourceOptions)
            {
                return new UpdateWebResource((UpdateWebResourceOptions)options);
            }
            else if (options is PublishCustomizationsOptions) 
            {
                return new PublishCustomizations((PublishCustomizationsOptions)options);
            }

            return null;
        }

        static void Main(string[] args)
        {
            var prog = new Program();
            prog.Execute(args);
        }

        private void Execute(string[] args)
        {
            var options = new ProgramOptions();

            if (!CommandLine.Parser.Default.ParseArguments(args, options,
              (verb, subOptions) =>
              {
                  try
                  {
                      var command = GetCommand(verb, subOptions);
                      if (command != null)
                      {
                          command.Execute();
                      }
                  }
                  catch (Exception ex)
                  {
                      Console.WriteLine(ex.ToString());
                  }
              }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }
        }
    }
}
