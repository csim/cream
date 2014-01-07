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
        private bool _boundLogger = false;
        private bool _boundAllExceptionLogger = false;

        private int _bindLevel = 0;

        [Flags]
        public enum BindFlags
        {
            All = 1,
            AllExceptLogger = 2,
            Logger = 4
        }

        public OptionBase Options { get; private set; }

        public CommandFactory(OptionBase options)
        {
            Options = options;
            Kernel = new StandardKernel();
        }

        public IKernel Kernel { get; set; }

        public virtual void Bind(BindFlags flags)
        {
            Bind<
                DefaultConfigurationProvider,
                DefaultLogger,
                OrganizationService,
                CrmOrganizationServiceContext
            >(flags);
        }

        public virtual void Bind<
                TConfiguration,
                TLogger,
                TOrganizationService,
                TCrmOrganizationServiceContext
            >(BindFlags flags)
            where TLogger : LoggerBase
            where TConfiguration : IConfigurationProvider
            where TOrganizationService : IOrganizationService
            where TCrmOrganizationServiceContext : CrmOrganizationServiceContext
        {
            if (!_boundLogger && ((flags & BindFlags.All) != 0 || (flags & BindFlags.Logger) != 0))
            {
                Kernel.Bind<IConfigurationProvider>()
                    .To<TConfiguration>()
                    .InSingletonScope()
                    .WithConstructorArgument("path", Options.Config);

                Kernel.Bind<LoggerBase>()
                    .To<TLogger>()
                    .InSingletonScope();

                _boundLogger = true;
            }

            if (!_boundAllExceptionLogger && ((flags & BindFlags.All) != 0 || (flags & BindFlags.AllExceptLogger) != 0))
            {
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

                _boundAllExceptionLogger = true;
            }
        }

        public LoggerBase GetLogger()
        {
            if (!_boundLogger) { Bind(BindFlags.Logger); }
            return Kernel.Get<LoggerBase>();
        }

        public ICommand GetCommand()
        {
            if (!_boundAllExceptionLogger) { Bind(BindFlags.All); }

            var type = Options.GetCommandType();
            var ret = (ICommand)Kernel.Get(type, new ConstructorArgument("options", Options, false));

            return ret;
        }


        public TDependency GetDependency<TDependency>()
        {
            if (!_boundAllExceptionLogger) { Bind(BindFlags.All); }

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
