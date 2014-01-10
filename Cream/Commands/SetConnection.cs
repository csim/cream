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
    public class SetConnectionOption : OptionBase
    {
        [Option('n', "name", DefaultValue = "", Required = true, HelpText = "Name of the connection.")]
        public string Name { get; set; }

        [Option('v', "value", DefaultValue = "", HelpText = "Connection string value.")]
        public string Value { get; set; }

        [Option('s', "server", Required = false, HelpText = "CRM Server URL. http://localhost:5555/contoso or https://contoso.api.crm.dynamics.com")]
        public string ServerUrl { get; set; }

        [Option('u', "username", Required = false, HelpText = "CRM Username.")]
        public string Username { get; set; }

        [Option('p', "password", Required = false, HelpText = "CRM Password.")]
        public string Password { get; set; }

        [Option('d', "domain", Required = false, HelpText = "CRM Domain.")]
        public string Domain { get; set; }

        public override Type GetCommandType()
        {
            return typeof(SetConnection);
        }
    }

    class SetConnection : CommandBase<SetConnectionOption>
    {
        public SetConnection(IKernel resolver, SetConnectionOption options)
            : base(resolver, options)
        {
        }

        public override void Execute()
        {
            if (Options.Name.Contains(";"))
            {
                throw new Exception("; character is not allowed in connection names.");
            }

            var connectionString = "";
            if (!string.IsNullOrEmpty(Options.Value))
            {
                connectionString = Options.Value;
            }
            else
            {
                if (string.IsNullOrEmpty(Options.ServerUrl)) throw new Exception("Unable to determine CRM server url.");
                if (string.IsNullOrEmpty(Options.Username)) throw new Exception("Unable to determine CRM username.");
                if (string.IsNullOrEmpty(Options.Password)) throw new Exception("Unable to determine CRM password.");

                connectionString = string.Format("Url={0}; Username={1}; Password={2};",
                Options.ServerUrl, Options.Username, Options.Password);
                if (!string.IsNullOrEmpty(Options.Domain))
                {
                    connectionString += string.Format(" Domain={0};", Options.Domain);
                }
            }

            Configuration.SetConnection(Options.Name, connectionString);
            Logger.Write("Set", "Connection: {0}".Compose(Options.Name));
            Configuration.Save(Options.Config);
            Logger.Write("Save", Options.Config);
        }
    }
}
