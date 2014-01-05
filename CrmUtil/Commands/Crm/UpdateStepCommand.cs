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
    public class UpdateStepOptions : CrmCommonOptionBase
    {
        [Option('t', "type", Required = true, HelpText = "Fully qualified assembly class type name.")]
        public string Type { get; set; }

        [Option("sync", DefaultValue = true, HelpText = "Synchronous plugin step.", MutuallyExclusiveSet = "Mode")]
        public bool Synchronous { get; set; }

        [Option("async", DefaultValue = false, HelpText = "Asynchronous plugin step.", MutuallyExclusiveSet = "Mode")]
        public bool Asynchronous { get; set; }

        [Option("stage", DefaultValue = PluginStage.Post, HelpText = "Plugin execution stage. [ Prevalidation, Pre, Post ]")]
        public PluginStage Stage { get; set; }

        public override Type GetCommandType()
        {
            return typeof(UpdateStepCommand);
        }
    }

    public enum PluginStage
    {
        Prevalidation = 10,
        Pre = 20,
        Post = 40
    }

    public class UpdateStepCommand : CrmCommandBase<UpdateStepOptions>
    {
        public UpdateStepCommand(IConfigurationProvider configurationProvider, LoggerBase logger, UpdateStepOptions options)
            : base(configurationProvider, logger, options)
        {
        }

        public override void Execute()
        {
        }
    }
}
