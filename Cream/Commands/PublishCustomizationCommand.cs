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

namespace Cream.Commands
{
    public class PublishCustomizationsOptions : OptionBase
    {
        public override Type GetCommandType()
        {
            return typeof(PublishCustomizationCommand);
        }
    }

    class PublishCustomizationCommand : CommandBase<PublishCustomizationsOptions>
    {
        public PublishCustomizationCommand(ICrmServiceProvider crmServiceProvider, LoggerBase logger, PublishCustomizationsOptions options)
            : base(crmServiceProvider, logger, options)
        {
        }

        public override void Execute()
        {
            PublishAllCustomizations();
        }
    }
}
