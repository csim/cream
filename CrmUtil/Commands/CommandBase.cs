using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using CrmUtil.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;

namespace CrmUtil.Commands
{
    public abstract class CommandBase<TOptions> : ICommand, IDisposable where TOptions : CommonOptions
    {
        private CrmConnection _crmConnection;
        private OrganizationService _crmService;
        private CrmOrganizationServiceContext _crmContext;

        protected IConfigurationProvider Configuration { get; private set; }
        protected Logger Logger { get; private set; }

        protected TOptions Options { get; private set; }

        protected string BaseName { get; private set; }

        protected CrmConnection CrmConnection {
            get {
                if (_crmConnection == null)
                {
                    _crmConnection = GetCrmConnection();
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

        public abstract void Execute();

        public CommandBase(IConfigurationProvider configuration, Logger logger, TOptions options)
        {
            Configuration = configuration;
            Logger = logger;
            Options = options;
            BaseName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Replace(".exe", string.Empty));
        }

        protected CrmConnection GetCrmConnection()
        {
            if (string.IsNullOrEmpty(Options.ServerUrl))
            {
                Options.ServerUrl = Configuration.GetSetting<string>("serverurl", null);
                if (string.IsNullOrEmpty(Options.ServerUrl))
                {
                    throw new Exception("Unable to determine CRM host.");
                }
            }

            if (string.IsNullOrEmpty(Options.Username))
            {
                Options.Username = Configuration.GetSetting<string>("username", null);
                if (string.IsNullOrEmpty(Options.Username))
                {
                    throw new Exception("Unable to determine CRM username.");
                }
            }

            if (string.IsNullOrEmpty(Options.Password))
            {
                Options.Password = Configuration.GetSetting<string>("password", null);
                if (string.IsNullOrEmpty(Options.Password))
                {
                    throw new Exception("Unable to determine CRM password.");
                }
            }

            if (string.IsNullOrEmpty(Options.Domain))
            {
                Options.Domain = Configuration.GetSetting<string>("domain", null);
            }

            var connstring = string.Format("Url={0}; Username={1}; Password={2}; DeviceID=yusamjdmckaj; DevicePassword=alkjdsfaldsjfewrqr;", 
                Options.ServerUrl, Options.Username, Options.Password);
            if (!string.IsNullOrEmpty(Options.Domain))
            {
                connstring += string.Format(" Domain={0};", Options.Domain);
            }

            //Console.WriteLine(connstring);

            var connection = CrmConnection.Parse(connstring);
            connection.ProxyTypesEnabled = false;
            return connection;
        }

        protected void PublishAllCustomizations()
        {
            Logger.Write(BaseName, "Publishing All Customizations... ");
            var request = new PublishAllXmlRequest();
            CrmService.Execute(request);
            Logger.Write(BaseName, "Done.");
        }

        protected void WarmupService()
        {
            GetCrmConnection();
            Logger.Write(BaseName, "Connecting to CRM ({0}) ... ".Compose(Options.ServerUrl));
            var request = new WhoAmIRequest();
            CrmService.Execute(request);
            Logger.Write(BaseName, "Done.");
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
