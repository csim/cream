using System;
using System.Collections.Generic;
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
        }

        protected CrmConnection GetCrmConnection()
        {
            var connstring = string.Format("Url={0}; Username={1}; Password={2}; DeviceID=yusamjdmckaj; DevicePassword=alkjdsfaldsjfewrqr;", Options.HostUrl, Options.Username, Options.Password);
            if (!string.IsNullOrEmpty(Options.Domain))
            {
                connstring += string.Format(" Domain={0}", Options.Domain);
            }

            return CrmConnection.Parse(connstring);
        }

        protected OrganizationService GetCrmService()
        {
            var conn = GetCrmConnection();
            return new OrganizationService(conn);
        }

        protected CrmOrganizationServiceContext GetCrmContext()
        {
            var conn = GetCrmConnection();
            return new CrmOrganizationServiceContext(conn);
        }

        protected void PublishAllCustomizations()
        {
            Console.Write("Publishing All Customizations... ");
            var request = new PublishAllXmlRequest();
            Service.Execute(request);
            Console.WriteLine("Done.");
        }

    }
}
