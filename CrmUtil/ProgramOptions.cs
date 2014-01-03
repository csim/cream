using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;
using CrmUtil.Commands;

namespace CrmUtil
{
    public class ProgramOptions
    {

        public ProgramOptions()
        {
            // Since we create this instance the parser will not overwrite it
            UpdateWebResourceVerb = new UpdateWebResourceOptions();
        }

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
}
