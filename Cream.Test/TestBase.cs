using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Fakes;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Fakes;
using Cream.Providers.Fakes;
using Cream.Commands;
using Cream.Logging;
using Cream.Providers;

namespace Cream.Test
{
    public abstract class TestBase
    {
        public StubIConfiguration MockConfigurationProvider { get; set; }

        public IConfiguration RealConfigurationProvider { get; set; }

        public CreamConfiguration ConfigurationData { get; set; }

        public CrmConnection Connection { get; set; }

        public StubIOrganizationService MockService { get; set; }
        
        public StubCrmOrganizationServiceContext MockContext { get; set; }

        public CommandFactory Factory { get; set; }

        public TestBase()
        {
            ConfigurationData = new CreamConfiguration();
            MockConfigurationProvider = new StubIConfiguration()
            {
                ConfigurationDataGet = () => ConfigurationData
            };

            RealConfigurationProvider = new Configuration();
            RealConfigurationProvider.ConfigurationData = ConfigurationData;

            Connection = new CrmConnection();
            MockService = new StubIOrganizationService();
            MockContext = new StubCrmOrganizationServiceContext(Connection);
            Factory = new CommandFactory();

            Factory.Bind(MockConfigurationProvider, new ConsoleLogger(), MockService, MockContext, Connection);
        }
    }
}
