using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cream.Providers;
using Cream.Logging;
using Ninject;
using Ninject.Parameters;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
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

        public OptionBase Options { get; set; }

        public IKernel Resolver { get; set; }

        public CommandFactory(OptionBase options = null)
        {
            Resolver = new StandardKernel();
        }

        public virtual void Bind(IConfiguration configuration, ILogger logger, IOrganizationService service, CrmOrganizationServiceContext context,  CrmConnection connection)
        {
            Resolver.Bind<IConfiguration>().ToMethod(i => configuration);
            
            Resolver.Bind<ILogger>().ToMethod(i => logger);
            
            Resolver.Bind<IOrganizationService>().ToMethod(i => service);

            Resolver.Bind<CrmConnection>()
                .ToMethod(i => connection);

            Resolver.Bind<CrmOrganizationServiceContext>()
                //.ToConstructor(i => new CrmOrganizationServiceContext(Resolver.Get<CrmConnection>()))
                .ToMethod(i => context)
                .InThreadScope();
            
            //Resolver.Bind<CrmOrganizationServiceContext>().ToMethod(i => context);
            
            _boundLogger = true;
            _boundAllExceptionLogger = true;
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
                Resolver.Bind<IConfiguration>()
                    .To<TConfiguration>()
                    .InSingletonScope();

                var config = Resolver.Get<IConfiguration>();
                config.Load(Options.Config);

                Resolver.Bind<ILogger>()
                    .To<TLogger>()
                    .InSingletonScope();

                _boundLogger = true;
            }
            
            if (!_boundAllExceptionLogger && Options is CrmOptionBase && (flags.HasFlag(BindFlags.All) || flags.HasFlag(BindFlags.AllExceptLogger)))
            {
                var connection = GetCrmConnection(Resolver.Get<IConfiguration>(), (CrmOptionBase)Options);

                Resolver.Bind<CrmConnection>()
                    .ToMethod(i => connection);

                Resolver.Bind<IOrganizationService>()
                    .ToConstructor(i => new OrganizationService(Resolver.Get<CrmConnection>()))
                    .InThreadScope();

                Resolver.Bind<CrmOrganizationServiceContext>()
                    .ToConstructor(i => new CrmOrganizationServiceContext(Resolver.Get<CrmConnection>()))
                    .InThreadScope();

                _boundAllExceptionLogger = true;
            }
        }

        public ILogger GetLogger()
        {
            if (!_boundLogger) { Bind(BindFlags.Logger); }
            return Resolver.Get<ILogger>();
        }

        public ICommand GetCommand()
        {
            if (!_boundAllExceptionLogger) { Bind(BindFlags.All); }

            var type = Options.GetCommandType();
            var ret = (ICommand)Resolver.Get(type, new ConstructorArgument("options", Options, false));

            return ret;
        }

        public void Dispose()
        {
            if (Resolver != null)
            {
                Resolver.Dispose();
                Resolver = null;
            }
        }


        public CrmConnection GetCrmConnection(IConfiguration configuration, CrmOptionBase options)
        {
            var connectionString = "";

            if (!string.IsNullOrEmpty(options.Connection))
            {
                if (options.Connection.Contains(";"))
                {
                    connectionString = options.Connection;
                }
                else
                {
                    connectionString = configuration.GetConnectionString(options.Connection);
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new Exception("ConnectionString does not exist.");
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(options.ServerUrl)) throw new Exception("Unable to determine CRM server url.");
                if (string.IsNullOrEmpty(options.Username)) throw new Exception("Unable to determine CRM username.");
                if (string.IsNullOrEmpty(options.Password)) throw new Exception("Unable to determine CRM password.");

                connectionString = string.Format("Url={0}; Username={1}; Password={2};",
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
