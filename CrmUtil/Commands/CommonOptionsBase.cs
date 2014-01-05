using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace CrmUtil.Commands
{
    public abstract class CommonOptionsBase
    {
        [Option("debug", Required = false, HelpText = "Launch debugger.")]
        public bool Debug { get; set; }

        [HelpOption]
        public virtual string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        [ParserState]
        public IParserState LastParserState { get; set; }

        public abstract Type GetCommandType();

    }
}
