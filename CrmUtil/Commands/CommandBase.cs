using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;

namespace CrmUtil.Commands
{
    public abstract class CommandBase
    {
        private CrmConnection _connection;
        private OrganizationService _service;
        private CrmOrganizationServiceContext _context;

        protected CommonOptions Options { get; private set; }
        
        protected CrmConnection Connection {
            get {
                if (_connection == null)
                {
                    _connection = GetCrmConnection();
                }
                return _connection;
            }
        }

        protected OrganizationService Service
        {
            get
            {
                if (_service == null)
                {
                    _service = GetCrmService();
                }
                return _service;
            }
        }

        protected CrmOrganizationServiceContext Context
        {
            get
            {
                if (_context == null)
                {
                    _context = GetCrmContext();
                }
                return _context;
            }
        }

        public abstract void Execute();

        public CommandBase(CommonOptions options)
        {
            Options = options;
            WarmupService();
        }

        protected CrmConnection GetCrmConnection()
        {
            var host = Options.HostUrl;
            var username = Options.Username;
            var password = Options.Password;
            var domain = Options.Domain;

            if (string.IsNullOrEmpty(host))
            {
                host = ConfigurationManager.AppSettings["host"];
                if (string.IsNullOrEmpty(host))
                {
                    throw new Exception("Unable to determine CRM host.");
                }
            }

            if (string.IsNullOrEmpty(username))
            {
                username = ConfigurationManager.AppSettings["username"];
                if (string.IsNullOrEmpty(username))
                {
                    throw new Exception("Unable to determine CRM username.");
                }
            }

            if (string.IsNullOrEmpty(password))
            {
                password = ConfigurationManager.AppSettings["password"];
                if (string.IsNullOrEmpty(password))
                {
                    throw new Exception("Unable to determine CRM password.");
                }
            }

            if (string.IsNullOrEmpty(domain))
            {
                domain = ConfigurationManager.AppSettings["domain"];
            }

            var connstring = string.Format("Url={0}; Username={1}; Password={2}; DeviceID=yusamjdmckaj; DevicePassword=alkjdsfaldsjfewrqr;", 
                host, username, password);
            if (!string.IsNullOrEmpty(domain))
            {
                connstring += string.Format(" Domain={0};", Options.Domain);
            }

            //Console.WriteLine(connstring);

            var connection = CrmConnection.Parse(connstring);
            connection.ProxyTypesEnabled = false;
            return connection;
        }

        protected OrganizationService GetCrmService()
        {
            return new OrganizationService(Connection);
        }

        protected CrmOrganizationServiceContext GetCrmContext()
        {
            return new CrmOrganizationServiceContext(Service);
        }

        protected void PublishAllCustomizations()
        {
            Console.Write("Publishing All Customizations... ");
            var request = new PublishAllXmlRequest();
            Service.Execute(request);
            Console.WriteLine("Done.");
        }

        protected void WarmupService()
        {
            Console.Write("Connecting to CRM ... ");
            var request = new WhoAmIRequest();
            Service.Execute(request);
            Console.WriteLine("Done.");
        }

    }
}
