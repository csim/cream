using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using Cream.Providers;
using Cream.Logging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Ninject;

namespace Cream.Commands
{
    public class RemoveConnectionOption : OptionBase
    {
        [Option('n', "name", DefaultValue = "", Required = true, HelpText = "Name of the connection.")]
        public string Name { get; set; }

        public override Type GetCommandType()
        {
            return typeof(SetConnection);
        }
    }

    class RemoveConnection : CommandBase<RemoveConnectionOption>
    {
        public RemoveConnection(IKernel resolver, RemoveConnectionOption options)
            : base(resolver, options)
        {
        }

        public override void Execute()
        {
            if (Options.Name.Contains(";"))
            {
                throw new Exception("; character is not allowed in connection names.");
            }

            Configuration.RemoveConnection(Options.Name);
            Logger.Write("Remove", "Connection: {0}".Compose(Options.Name));
            Configuration.Save(Options.Config);
            Logger.Write("Save", Options.Config);
        }
    }
}
