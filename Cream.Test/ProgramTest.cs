using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cream.Test
{
    [TestClass]
    public class ProgramTest : TestBase
    {
        [TestMethod]
        public void TestMethod1()
        {
            var target = new Program();

            string[] args = "SetConnection --name deathstar --value t".Split(' ');

            var output = new Dictionary<string, string>();
            MockConfigurationProvider.SetConnectionStringString = (name, connectionString) =>
            {
                output[name] = connectionString;
            };

            target.Execute(args, Factory);

            using (ShimsContext.Create())
            {
                ShimDirectoryInfo.ConstructorString = (t, path) =>
                {
                    var info = new ShimDirectoryInfo(t)
                    {
                        NameGet = () => path
                    };
                };

                var d = new DirectoryInfo("tttt");
                Assert.IsTrue(d.Name == "tttt");

            }

            //test change


            // blah master

            Assert.IsTrue(output.ContainsKey("deathstar"));
        }
    }
}
