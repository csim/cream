using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Cream.Commands;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Ninject;
using Ninject.Parameters;

namespace Cream.Providers
{
    public class DefaultCrmServiceProvider : ICrmServiceProvider
    {

        private IConfigurationProvider Configuration { get; set; }

        private IKernel Kernel { get; set; }

        private string _crmConnectionString;

        public DefaultCrmServiceProvider(IConfigurationProvider configuration, IKernel kernel)
        {
            Configuration = configuration;
            Kernel = kernel;
        }

        public CrmConnection GetCrmConnection()
        {
            if (string.IsNullOrEmpty(_crmConnectionString))
            {
                throw new Exception("Not Initialized.");
            }

            var settings = new ConnectionStringSettings("connectionString", _crmConnectionString);
            return Kernel.Get<CrmConnection>(new ConstructorArgument("connectionString", settings));
        }

        public OrganizationService GetCrmService()
        {
            if (string.IsNullOrEmpty(_crmConnectionString))
            {
                throw new Exception("Not Initialized.");
            }

            return Kernel.Get<OrganizationService>(new ConstructorArgument("connection", GetCrmConnection()));
        }

        public CrmOrganizationServiceContext GetCrmContext()
        {
            if (string.IsNullOrEmpty(_crmConnectionString))
            {
                throw new Exception("Not Initialized.");
            }

            return Kernel.Get<CrmOrganizationServiceContext>(new ConstructorArgument("connection", GetCrmConnection()));
        }

        public void Initialize(OptionBase options)
        {
            var smask = "{0}";
            if (!string.IsNullOrEmpty(options.Environment))
            {
                smask = options.Environment.ToLower() + ".{0}";
            }

            if (string.IsNullOrEmpty(options.ServerUrl))
            {
                options.ServerUrl = Configuration.GetSetting<string>(smask.Compose("serverurl"));
                if (string.IsNullOrEmpty(options.ServerUrl))
                {
                    throw new Exception("Unable to determine CRM server url.");
                }
            }

            if (string.IsNullOrEmpty(options.Username))
            {
                options.Username = Configuration.GetSetting<string>(smask.Compose("username"));
                if (string.IsNullOrEmpty(options.Username))
                {
                    throw new Exception("Unable to determine CRM username.");
                }
            }

            if (string.IsNullOrEmpty(options.Password))
            {
                options.Password = Configuration.GetSetting<string>(smask.Compose("password"));
                if (string.IsNullOrEmpty(options.Password))
                {
                    throw new Exception("Unable to determine CRM password.");
                }
            }

            if (string.IsNullOrEmpty(options.Domain))
            {
                options.Domain = Configuration.GetSetting<string>(smask.Compose("domain"));
            }

            _crmConnectionString = string.Format("Url={0}; Username={1}; Password={2}; DeviceID=yusamjdmckaj; DevicePassword=alkjdsfaldsjfewrqr;",
            options.ServerUrl, options.Username, options.Password);
            if (!string.IsNullOrEmpty(options.Domain))
            {
                _crmConnectionString += string.Format(" Domain={0};", options.Domain);
            }
        }
    }
}
