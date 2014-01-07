using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cream.Providers;
using Cream.Logging;
using Ninject;
using Ninject.Parameters;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using System.Configuration;
using Microsoft.Xrm.Sdk;

namespace Cream.Commands
{
    public class CommandFactory : IDisposable
    {
        public OptionBase Options { get; private set; }

        public CommandFactory(OptionBase options)
        {
            Options = options;
        }

        public IKernel Kernel { get; set; }

        public virtual void Bind()
        {
            Bind<
                DefaultConfigurationProvider,
                DefaultLogger,
                OrganizationService,
                CrmOrganizationServiceContext
            >();
        }

        public virtual void Bind<
                TConfiguration,
                TLogger,
                TOrganizationService,
                TCrmOrganizationServiceContext
            >()
            where TLogger : LoggerBase
            where TConfiguration : IConfigurationProvider
            where TOrganizationService : IOrganizationService
            where TCrmOrganizationServiceContext : CrmOrganizationServiceContext
        {
            Kernel = new StandardKernel();

            Kernel.Bind<IConfigurationProvider>()
                .To<TConfiguration>()
                .InSingletonScope()
                .WithConstructorArgument("path", Options.Config);

            Kernel.Bind<LoggerBase>()
                .To<TLogger>()
                .InSingletonScope();

            var connection = GetCrmConnection(Kernel.Get<IConfigurationProvider>(), Options);

            Kernel.Bind<IOrganizationService>()
                .ToConstructor(i => new OrganizationService(connection))
                .InThreadScope();

            Kernel.Bind<CrmOrganizationServiceContext>()
                .ToConstructor(i => new CrmOrganizationServiceContext(connection))
                .InThreadScope()
                .WithConstructorArgument("connection", connection);

            Kernel.Bind<CrmConnection>()
                .ToMethod(i => connection);                
        }

        public ICommand GetCommand()
        {
            if (Kernel == null) { Bind(); }

            var type = Options.GetCommandType();
            var ret = (ICommand)Kernel.Get(type, new ConstructorArgument("options", Options, false));

            return ret;
        }


        public TDependency GetDependency<TDependency>()
        {
            if (Kernel == null) { Bind(); }

            return Kernel.Get<TDependency>();
        }

        public void Dispose()
        {
            if (Kernel != null)
            {
                Kernel.Dispose();
                Kernel = null;
            }
        }


        public CrmConnection GetCrmConnection(IConfigurationProvider configuration, OptionBase options)
        {
            var connectionString = "";

            if (!string.IsNullOrEmpty(options.Connection))
            {
                connectionString = configuration.GetConnectionString(options.Connection);
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("ConnectionString does not exist.");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(options.ServerUrl)) throw new Exception("Unable to determine CRM server url.");
                if (string.IsNullOrEmpty(options.Username)) throw new Exception("Unable to determine CRM username.");
                if (string.IsNullOrEmpty(options.Password)) throw new Exception("Unable to determine CRM password.");

                connectionString = string.Format("Url={0}; Username={1}; Password={2}; DeviceID=yusamjdmckaj; DevicePassword=alkjdsfaldsjfewrqr;",
                options.ServerUrl, options.Username, options.Password);
                if (!string.IsNullOrEmpty(options.Domain))
                {
                    connectionString += string.Format(" Domain={0};", options.Domain);
                }
            }

            return CrmConnection.Parse(connectionString);
        }
    }
}
