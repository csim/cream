using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using CrmUtil.Providers;
using CrmUtil.Logging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmUtil.Commands.Crm
{
    public class PublishCustomizationsOptions : CrmCommonOptionBase
    {
        public override Type GetCommandType()
        {
            return typeof(PublishCustomizationsCommand);
        }
    }

    class PublishCustomizationsCommand : CrmCommandBase<PublishCustomizationsOptions>
    {
        public PublishCustomizationsCommand(ICrmServiceProvider crmServiceProvider, LoggerBase logger, PublishCustomizationsOptions options)
            : base(crmServiceProvider, logger, options)
        {
        }

        public override void Execute()
        {
            PublishAllCustomizations();
        }
    }
}
