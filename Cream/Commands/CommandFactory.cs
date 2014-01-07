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

        [Flags]
        public enum BindFlags
        {
            All = 1,
            AllExceptLogger = 2,
            Logger = 4
        }

        public OptionBase Options { get; private set; }

        public IKernel Kernel { get; set; }

        public CommandFactory(OptionBase options)
        {
            Options = options;
            Kernel = new StandardKernel();
        }

        public virtual void Bind(BindFlags flags = BindFlags.All)
        {
            Bind<
                DefaultConfiguration,
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
            >(BindFlags flags = BindFlags.All)
            where TLogger : ILogger
            where TConfiguration : IConfiguration
            where TOrganizationService : IOrganizationService
            where TCrmOrganizationServiceContext : CrmOrganizationServiceContext
        {
            if (!_boundLogger && (flags.HasFlag(BindFlags.All) || flags.HasFlag(BindFlags.Logger)))
            {
                Kernel.Bind<IConfiguration>()
                    .To<TConfiguration>()
                    .InSingletonScope()
                    .WithConstructorArgument("path", Options.Config);

                Kernel.Bind<ILogger>()
                    .To<TLogger>()
                    .InSingletonScope();

                _boundLogger = true;
            }
            
            if (!_boundAllExceptionLogger && Options is CrmOptionBase && (flags.HasFlag(BindFlags.All) || flags.HasFlag(BindFlags.AllExceptLogger)))
            {
                var connection = GetCrmConnection(Kernel.Get<IConfiguration>(), (CrmOptionBase)Options);

                Kernel.Bind<CrmConnection>()
                    .ToMethod(i => connection);

                Kernel.Bind<IOrganizationService>()
                    .ToConstructor(i => new OrganizationService(Kernel.Get<CrmConnection>()))
                    .InThreadScope();

                Kernel.Bind<CrmOrganizationServiceContext>()
                    .ToConstructor(i => new CrmOrganizationServiceContext(Kernel.Get<CrmConnection>()))
                    .InThreadScope();

                _boundAllExceptionLogger = true;
            }
        }

        public ILogger GetLogger()
        {
            if (!_boundLogger) { Bind(BindFlags.Logger); }
            return Kernel.Get<ILogger>();
        }

        public ICommand GetCommand()
        {
            if (!_boundAllExceptionLogger) { Bind(BindFlags.All); }

            var type = Options.GetCommandType();
            var ret = (ICommand)Kernel.Get(type, new ConstructorArgument("options", Options, false));

            return ret;
        }

        public void Dispose()
        {
            if (Kernel != null)
            {
                Kernel.Dispose();
                Kernel = null;
            }
        }


        public CrmConnection GetCrmConnection(IConfiguration configuration, CrmOptionBase options)
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
