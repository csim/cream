using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CrmUtil.Configuration;
using CrmUtil.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;

namespace CrmUtil.Commands.Crm
{
    public class CrmCommandBase<TOptions> : ICommand, IDisposable where TOptions : CrmCommonOptionBase
    {
        private CrmConnection _crmConnection;
        private OrganizationService _crmService;
        private CrmOrganizationServiceContext _crmContext;
        private object _crmConnectionLock = new object();

        protected IConfigurationProvider Configuration { get; private set; }
        protected LoggerBase Logger { get; private set; }

        protected TOptions Options { get; private set; }

        protected string BaseName { get; private set; }

        protected CrmConnection CrmConnection
        {
            get
            {
                lock (_crmConnectionLock)
                {
                    if (_crmConnection == null)
                    {
                        _crmConnection = GetCrmConnection();
                    }
                }
                return _crmConnection;
            }
        }

        protected OrganizationService CrmService
        {
            get
            {
                if (_crmService == null)
                {
                    _crmService = new OrganizationService(CrmConnection);
                }
                return _crmService;
            }
        }

        protected CrmOrganizationServiceContext CrmContext
        {
            get
            {
                if (_crmContext == null)
                {
                    _crmContext = new CrmOrganizationServiceContext(CrmService);
                }
                return _crmContext;
            }
        }

        public virtual void Execute()
        {
            if (Options.Debug) System.Diagnostics.Debugger.Launch();
        }

        public CrmCommandBase(IConfigurationProvider configuration, LoggerBase logger, TOptions options)
        {
            Configuration = configuration;
            Logger = logger;
            Options = options;
            BaseName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName.Replace(".exe", string.Empty));
        }

        protected void ProcessConnectionOptions()
        {
            var smask = "{0}";
            if (!string.IsNullOrEmpty(Options.Environment))
            {
                smask = Options.Environment.ToLower() + ".{0}";
            }

            if (string.IsNullOrEmpty(Options.ServerUrl))
            {
                Options.ServerUrl = Configuration.GetSetting<string>(smask.Compose("serverurl"));
                if (string.IsNullOrEmpty(Options.ServerUrl))
                {
                    throw new Exception("Unable to determine CRM server url.");
                }
            }

            if (string.IsNullOrEmpty(Options.Username))
            {
                Options.Username = Configuration.GetSetting<string>(smask.Compose("username"));
                if (string.IsNullOrEmpty(Options.Username))
                {
                    throw new Exception("Unable to determine CRM username.");
                }
            }

            if (string.IsNullOrEmpty(Options.Password))
            {
                Options.Password = Configuration.GetSetting<string>(smask.Compose("password"));
                if (string.IsNullOrEmpty(Options.Password))
                {
                    throw new Exception("Unable to determine CRM password.");
                }
            }

            if (string.IsNullOrEmpty(Options.Domain))
            {
                Options.Domain = Configuration.GetSetting<string>(smask.Compose("domain"));
            }
        }

        protected CrmConnection GetCrmConnection()
        {
            ProcessConnectionOptions();
            var connstring = string.Format("Url={0}; Username={1}; Password={2}; DeviceID=yusamjdmckaj; DevicePassword=alkjdsfaldsjfewrqr; ProxyTypesEnabled=false;",
                Options.ServerUrl, Options.Username, Options.Password);
            if (!string.IsNullOrEmpty(Options.Domain))
            {
                connstring += string.Format(" Domain={0};", Options.Domain);
            }
            var connection = CrmConnection.Parse(connstring);
            return connection;
        }

        protected void PublishAllCustomizations()
        {
            Logger.Write("Publish", "All Customizations ... ");
            var request = new PublishAllXmlRequest();
            CrmService.Execute(request);
            Logger.Write("Publish", "Done.");
        }

        protected void WarmupCrmService()
        {
            ProcessConnectionOptions();
            Logger.Write(BaseName, "Connecting to CRM ({0})".Compose(Options.ServerUrl));
            var request = new WhoAmIRequest();
            CrmService.Execute(request);
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
            Logger.Dispose();
        }
    }
}
