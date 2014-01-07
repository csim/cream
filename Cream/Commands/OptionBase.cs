using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Cream.Commands
{
    public abstract class OptionBase
    {
        [Option("config", Required = false, DefaultValue = @".\cream.config", HelpText = "Path to the cream configuration file.")]
        public string Config { get; set; }

        [Option("debug", Required = false, HelpText = "Launch .NET debugger on start.")]
        public bool Debug { get; set; }

        [HelpOption]
        public virtual string GetUsage()
        {
            return HelpText.AutoBuild(this, (current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        [ParserState]
        public IParserState LastParserState { get; set; }

        public abstract Type GetCommandType();

    }
}
