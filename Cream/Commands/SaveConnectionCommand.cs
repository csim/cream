﻿using System;
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
    public class SaveConnectionOptions : OptionBase
    {
        [Option("name", DefaultValue = "", Required = true, HelpText = "Name of the connection.")]
        public string Name { get; set; }

        [Option("value", DefaultValue = "", HelpText = "Connection string value.")]
        public string Value { get; set; }

        public override Type GetCommandType()
        {
            return typeof(SaveConnectionCommand);
        }
    }

    class SaveConnectionCommand : CommandBase<SaveConnectionOptions>
    {
        public SaveConnectionCommand(IKernel resolver, SaveConnectionOptions options)
            : base(resolver, options)
        {
        }

        public override void Execute()
        {
            Configuration.ConfigurationData.Connections.Add(Options.Name, Options.Value);
            Configuration.Save();
            Logger.Write("Save", Options.Config);
        }
    }
}
