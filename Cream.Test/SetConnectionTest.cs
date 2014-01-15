using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cream.Fakes;
using Cream.Commands.Fakes;
using Cream.Providers.Fakes;
using Cream.Providers;
using Cream.Commands;
using Cream.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Fakes;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Fakes;
using System.Collections.Generic;

namespace Cream.Test
{
    [TestClass]
    public class SetConnectionTest : TestBase
    {
        private Dictionary<string, string> _output;

        [TestInitialize]
        public void Init()
        {
            _output = new Dictionary<string, string>();
            MockConfigurationProvider.SetConnectionStringString = (name, connectionString) =>
            {
                _output[name] = connectionString;
            };
        }

        [TestMethod]
        public void FullValue()
        {
            var options = new SetConnectionOption();
            options.Name = "test1";
            options.Value = "Url=http://alderaan.com; Username=han; Password=solo;";
            options.Config = ".cream";
            Factory.Options = options;

            var target = Factory.GetCommand();
            target.Execute();

            Assert.IsTrue(_output.ContainsKey(options.Name));
            Assert.IsTrue(_output[options.Name] == options.Value);
        }

        [TestMethod]
        public void PartialNoDomain()
        {
            var options = new SetConnectionOption();
            options.Name = "test1";
            options.Username = "han";
            options.Password = "solo";
            options.ServerUrl = "alderaan.com";
            options.Config = ".cream";
            Factory.Options = options;

            var target = Factory.GetCommand();
            target.Execute();

            Assert.IsTrue(_output.ContainsKey(options.Name));
            Assert.IsTrue(_output[options.Name] == string.Format("Url={0}; Username={1}; Password={2};",
                options.ServerUrl, options.Username, options.Password));
        }

        [TestMethod]
        public void PartialWithDomain()
        {
            var options = new SetConnectionOption();
            options.Name = "test1";
            options.Username = "han";
            options.Password = "solo";
            options.ServerUrl = "alderaan.com";
            options.Domain = "deathstar";
            options.Config = ".cream";
            Factory.Options = options;

            var target = Factory.GetCommand();
            target.Execute();

            Assert.IsTrue(_output.ContainsKey(options.Name));
            Assert.IsTrue(_output[options.Name] == string.Format("Url={0}; Username={1}; Password={2}; Domain={3};",
                options.ServerUrl, options.Username, options.Password, options.Domain));
        }
    }
}
