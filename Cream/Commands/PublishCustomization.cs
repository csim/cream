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
    public class PublishCustomizationOption : CrmOptionBase
    {
        public override Type GetCommandType()
        {
            return typeof(PublishCustomization);
        }
    }

    class PublishCustomization : CommandBase<PublishCustomizationOption>
    {
        public PublishCustomization(IKernel resolver, PublishCustomizationOption options)
            : base(resolver, options)
        {
        }

        public override void Execute()
        {
            PublishAllCustomizations();
        }
    }
}
