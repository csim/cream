﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using CrmUtil.Logging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmUtil.Commands
{
    public class PublishCustomizationsOptions : CommonOptions
    {
        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class PublishCustomizations : CommandBase<PublishCustomizationsOptions>
    {
        public PublishCustomizations(IConfigurationProvider configuration, Logger logger, PublishCustomizationsOptions options)
            : base(configuration, logger, options)
        {
        }

        public override void Execute()
        {
            PublishAllCustomizations();
        }
    }
}