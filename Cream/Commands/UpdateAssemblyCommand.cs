using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Cream.Providers;
using Cream.Logging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Cream.Commands
{
    public class UpdateAssemblyOptions : UpdateResourceCommandBaseOptions
    {
        [OptionArray('f', "filters", DefaultValue = new string[] { "*.dll" }, HelpText = "Set of wildcard patterns.")]
        public override string[] Filters { get; set; }

        [Option("database", DefaultValue = true, HelpText = "Assembly is deployed to database.", MutuallyExclusiveSet = "Isolation")]
        public bool Database { get; set; }

        [Option("disk", DefaultValue = false, HelpText = "Assembly is deployed to disk.", MutuallyExclusiveSet = "Isolation")]
        public bool Disk { get; set; }

        [Option("sandbox", DefaultValue = false, HelpText = "Assembly is deployed in sandbox isolation mode.")]
        public bool Sandbox { get; set; }

        [Option("notypes", DefaultValue = false, HelpText = "Do not create plugintype records.")]
        public bool NoTypes { get; set; }

        public override Type GetCommandType()
        {
            return typeof(UpdateAssemblyCommand);
        }
    }

    public class UpdateAssemblyCommand : ResourceCommandBase<UpdateAssemblyOptions>
    {
        public UpdateAssemblyCommand(ICrmServiceProvider crmServiceProvider, LoggerBase logger, UpdateAssemblyOptions options)
            : base(crmServiceProvider, logger, options)
        {
        }

        protected override bool UpdateSingle(FileInfo file, int? index = null)
        {
            var ret = true;
            var log = new StringBuilder();

            try
            {
                var category = "";
                if (index.HasValue)
                {
                    category = "({0:N0})".Compose(index.Value);
                }
                category += " {0}";

                var relativeFilePath = GetRelativePath(file, Options.Path);
                var name = Path.GetFileNameWithoutExtension(file.FullName);
                //var existingResource = GetRecord("pluginassembly", i => (string)i["name"] == name, i => new { Name = i["name"] });

                //var existingResource1 = CrmContext.CreateQuery("pluginassembly")
                //                                .Select(i => new { 
                //                                                    Id = i.Id, 
                //                                                    Name = (string)i["name"], 
                //                                                    ModifiedOn = (DateTime)i["modifiedon"] 
                //                                })
                //                                .FirstOrDefault(i => i.Name == name);

                var existingResource = (from record in CrmContext.CreateQuery("pluginassembly")
                                        where (string)record["name"] == name
                                        select new
                                        {
                                            Id = record.Id,
                                            Name = (string)record["name"],
                                            ModifiedOn = (DateTime)record["modifiedon"]
                                        }).FirstOrDefault();
                              


                var newResource = new Entity("pluginassembly");
                newResource["name"] = name;

                var sourcetype = 0;
                if (Options.Database)
                {
                    sourcetype = 0;
                }
                
                if (Options.Disk)
                {
                    sourcetype = 1;
                }

                newResource["sourcetype"] = new OptionSetValue(sourcetype);

                if (Options.Sandbox)
                {
                    newResource["isolationmode"] = new OptionSetValue(2);
                }
                else
                {
                    newResource["isolationmode"] = new OptionSetValue(1);
                }

                OrganizationRequest request;
                if (existingResource != null)
                {
                    if (!Options.Force && (DateTime)existingResource.ModifiedOn >= file.LastWriteTime.ToUniversalTime())
                    {
                        Logger.Write(log, category.Compose("Ignore"), relativeFilePath);
                        Logger.Write(log.ToString());
                        return false;
                    }

                    Logger.Write(log, category.Compose("Update"), relativeFilePath);
                    newResource.Id = existingResource.Id;
                    request = new UpdateRequest() { Target = newResource };
                }
                else
                {
                    Logger.Write(log, category.Compose("Create"), relativeFilePath);
                    request = new CreateRequest() { Target = newResource };
                }

                if (!string.IsNullOrEmpty(category) && index.HasValue)
                {
                    category = category.Compose(index);
                }

                var fileBytes = File.ReadAllBytes(file.FullName);
                newResource.Attributes["content"] = Convert.ToBase64String(fileBytes);

                var response = CrmService.Execute(request);
                if (response is CreateResponse)
                {
                    newResource.Id = ((CreateResponse)response).id;
                }

                if (!Options.NoTypes)
                {
                    UpdateAssemblyTypes(newResource.Id, file, log);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(log, BaseName, ex);
                ret = false;
            }

            Logger.Write(log.ToString());

            return ret;
        }

        public bool UpdateAssemblyTypes(Guid pluginId, FileInfo file, StringBuilder log)
        {
            var assembly = Assembly.LoadFile(file.FullName);
            var types = assembly.GetTypes().Where(i => i.IsClass && typeof(IPlugin).IsAssignableFrom(i));

            foreach (var type in types)
            {
                var name = type.FullName;
                var lname = "Type: {0}".Compose(name);

                var existingResource = (from record in CrmContext.CreateQuery("plugintype")
                                        where (string)record["typename"] == name
                                        select new
                                        {
                                            Id = record.Id,
                                            ModifiedOn = (DateTime)record["modifiedon"]
                                        }).FirstOrDefault();
                

                var newResource = new Entity("plugintype");
                newResource["typename"] = type.FullName;
                newResource["friendlyname"] = type.FullName;
                newResource["pluginassemblyid"] = new EntityReference("pluginassembly", pluginId);

                OrganizationRequest request;
                if (existingResource != null)
                {
                    if (!Options.Force && (DateTime)existingResource.ModifiedOn >= file.LastWriteTime.ToUniversalTime())
                    {
                        Logger.Write(log, "Ignore", lname);
                        Logger.Write(log.ToString());
                        return false;
                    }

                    Logger.Write(log, "Update", lname);
                    newResource.Id = existingResource.Id;
                    request = new UpdateRequest() { Target = newResource };
                }
                else
                {
                    Logger.Write(log, "Create", lname);
                    request = new CreateRequest() { Target = newResource };
                }

                CrmService.Execute(request);
            }

            return true;
        }

    }
}
