using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Cream.Providers;
using Cream.Logging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Cream.Commands
{
    public class UpdateStepOptions : OptionBase
    {
        [Option('e', "entity", Required = true, HelpText = "Entity for which the step will be registered.")]
        public string Entity { get; set; }

        [Option('t', "type", Required = true, HelpText = "Fully qualified assembly class type name.")]
        public string Type { get; set; }

        [Option('m', "message", Required = true, HelpText = "SDK message associated with this plugin.")]
        public string Message { get; set; }

        [Option("sync", DefaultValue = true, HelpText = "Register synchronous plugin step.", MutuallyExclusiveSet = "Mode")]
        public bool Synchronous { get; set; }

        [Option("async", DefaultValue = false, HelpText = "Register asynchronous plugin step.", MutuallyExclusiveSet = "Mode")]
        public bool Asynchronous { get; set; }

        [Option("prevalidation", DefaultValue = false, HelpText = "Register in pre-validation plugin stage.", MutuallyExclusiveSet = "Stage")]
        public bool Prevalidation { get; set; }

        [Option("pre", DefaultValue = false, HelpText = "Register in pre-operation plugin stage.", MutuallyExclusiveSet = "Stage")]
        public bool Pre { get; set; }

        [Option("post", DefaultValue = true, HelpText = "Register in post-operation plugin stage.", MutuallyExclusiveSet = "Stage")]
        public bool Post { get; set; }

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

    public class UpdateStepCommand : CommandBase<UpdateStepOptions>
    {
        public UpdateStepCommand(ICrmServiceProvider crmServiceProvider, LoggerBase logger, UpdateStepOptions options)
            : base(crmServiceProvider, logger, options)
        {
        }

        public override void Execute()
        {
        }
    }
}
