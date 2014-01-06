using System;
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
    public class CommandBase<TOptions> : ICommand, IDisposable where TOptions : OptionBase
    {
        public IKernel Resolver { get; private set; }

        public IConfigurationProvider Configuration { get; private set; }

        public LoggerBase Logger { get; set; }

        public TOptions Options { get; set; }

        protected ApplicationInfo App { get; private set; }

        private CrmConnection _crmConnection;

        public CrmConnection Connection
        {
            get
            {
                if (_crmConnection == null)
                {
                    _crmConnection = GetCrmConnection();
                }
                return _crmConnection;
            }
        }

        public IOrganizationService Service
        {
            get
            {
                //return Resolver.Get<IOrganizationService>();
                //return Resolver.Get<IOrganizationService>(new ConstructorArgument("connection", Connection, false));
                return new OrganizationService(Connection);
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
            Resolver = resolver;
            Logger = Resolver.Get<LoggerBase>();
            App = new ApplicationInfo();
            Configuration = resolver.Get<IConfigurationProvider>();
            Options = options;
        }

        public virtual void Execute()
        {
            if (Options.Debug) System.Diagnostics.Debugger.Launch();
        }

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

        public CrmConnection GetCrmConnection()
        {
            var connectionString = "";

            if (!string.IsNullOrEmpty(Options.Connection))
            {
                connectionString = Configuration.GetConnectionString(Options.Connection);
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("ConnectionString does not exist.");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Options.ServerUrl)) throw new Exception("Unable to determine CRM server url.");
                if (string.IsNullOrEmpty(Options.Username)) throw new Exception("Unable to determine CRM username.");
                if (string.IsNullOrEmpty(Options.Password)) throw new Exception("Unable to determine CRM password.");

                connectionString = string.Format("Url={0}; Username={1}; Password={2}; DeviceID=yusamjdmckaj; DevicePassword=alkjdsfaldsjfewrqr;",
                Options.ServerUrl, Options.Username, Options.Password);
                if (!string.IsNullOrEmpty(Options.Domain))
                {
                    connectionString += string.Format(" Domain={0};", Options.Domain);
                }
            }

            return CrmConnection.Parse(connectionString);
        }


        public void Dispose()
        {
            if (Logger != null) {
                Logger.Dispose();
            }
        }
    }
}
