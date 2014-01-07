using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Cream.Commands;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Ninject;
using Ninject.Parameters;

namespace Cream.Providers
{
    public class DefaultCrmServiceProvider<TOrganizationService, TOrganizationContext> : ICrmServiceProvider
        where TOrganizationService : IOrganizationService
        where TOrganizationContext : CrmOrganizationServiceContext
    {

        private IConfigurationProvider Configuration { get; set; }

        private IKernel Kernel { get; set; }

        private string _crmConnectionString;

        public DefaultCrmServiceProvider(IConfigurationProvider configuration, IKernel kernel)
        {
            Configuration = configuration;
            Kernel = kernel;
        }

        public CrmConnection Connection
        {
            get
            {
                if (string.IsNullOrEmpty(_crmConnectionString))
                {
                    throw new Exception("Not Initialized.");
                }

                var settings = new ConnectionStringSettings("connectionString", _crmConnectionString);
                return Kernel.Get<CrmConnection>(new ConstructorArgument("connectionString", settings));
            }
        }

        public IOrganizationService Service 
        {
            get
            {
                if (string.IsNullOrEmpty(_crmConnectionString))
                {
                    throw new Exception("Not Initialized.");
                }

                return Kernel.Get<IOrganizationService>();
            }
        }

        public CrmOrganizationServiceContext Context
        {
            get
            {
                if (string.IsNullOrEmpty(_crmConnectionString))
                {
                    throw new Exception("Not Initialized.");
                }

                return Kernel.Get<CrmOrganizationServiceContext>();
            }
        }

        public void Initialize(OptionBase options)
        {
            var connectionString = "";

            if (!string.IsNullOrEmpty(options.Connection))
            {
                connectionString = Configuration.GetConnectionString(options.Connection);
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

            var _connection = CrmConnection.Parse(connectionString);

            Kernel.Bind<IOrganizationService>()
                .To<TOrganizationService>()
                .InThreadScope()
                .WithConstructorArgument("connection", _connection);

            Kernel.Bind<CrmOrganizationServiceContext>()
                .To<TOrganizationContext>()
                .InThreadScope()
                .WithConstructorArgument("connection", _connection);

        }
    }
}