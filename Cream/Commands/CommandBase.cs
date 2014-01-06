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

namespace Cream.Commands
{
    public class CommandBase<TOptions> : ICommand, IDisposable where TOptions : OptionBase
    {
        public ICrmServiceProvider CrmServiceProvider { get; set; }

        protected LoggerBase Logger { get; private set; }

        protected TOptions Options { get; private set; }

        protected string BaseName { get; private set; }

        public OrganizationService CrmService
        {
            get
            {
                return CrmServiceProvider.GetCrmService();
            }
        }

        protected CrmOrganizationServiceContext CrmContext
        {
            get
            {
                return CrmServiceProvider.GetCrmContext();
            }
        }

        public virtual void Execute()
        {
            if (Options.Debug) System.Diagnostics.Debugger.Launch();
        }

        public CommandBase(ICrmServiceProvider crmServiceProvider, LoggerBase logger, TOptions options)
        {
            CrmServiceProvider = crmServiceProvider;
            Logger = logger;
            Options = options;
            BaseName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName.Replace(".exe", string.Empty));
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
            Logger.Write("Connect", Options.ServerUrl);
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
            if (Logger != null) {
                Logger.Dispose();
            }
        }
    }
}
