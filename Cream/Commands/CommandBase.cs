﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Cream.Providers;
using Cream.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Ninject;
using Ninject.Parameters;

namespace Cream.Commands
{
    public abstract class CommandBase<TOptions> : ICommand, IDisposable where TOptions : OptionBase
    {
        public IConfiguration Configuration { get; private set; }

        public ILogger Logger { get; set; }

        public TOptions Options { get; set; }

        protected ApplicationInfo App { get; private set; }

        public IKernel Resolver { get; set; }

        public CrmConnection Connection
        {
            get
            {
                return Resolver.Get<CrmConnection>();
            }
        }

        public IOrganizationService Service
        {
            get
            {
                return Resolver.Get<IOrganizationService>();
            }
        }

        public CrmOrganizationServiceContext Context
        {
            get
            {
                return Resolver.Get<CrmOrganizationServiceContext>();
            }
        }

        public CommandBase(IKernel resolver, TOptions options)
        {
            Options = options;
            Resolver = resolver;
            Logger = Resolver.Get<ILogger>();
            Configuration = Resolver.Get<IConfiguration>();
            App = new ApplicationInfo();
        }

        public abstract void Execute();

        protected void PublishAllCustomizations()
        {
            Logger.Write("Publish", "All Customizations ... ");
            var request = new PublishAllXmlRequest();
            Service.Execute(request);
            Logger.Write("Publish", "Done.");
        }

        protected void WarmupCrmService()
        {
            Logger.Write("Connect", Connection.ServiceUri.ToString());
            var request = new WhoAmIRequest();
            Service.Execute(request);
        }

        protected string GetRelativePath(FileInfo file, string rootPath)
        {
            return GetRelativePath(file.FullName, rootPath);
        }

        protected string GetRelativePath(string filepath, string rootPath)
        {
            var pathUri = new Uri(filepath);
            // Folders must end in a slash
            if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                rootPath += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(rootPath);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public void Dispose()
        {
            if (Logger != null) {
                Logger.Dispose();
            }
        }
    }
}
