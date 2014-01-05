using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using CrmUtil.Configuration;
using CrmUtil.Logging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmUtil.Commands.Crm
{
    public class UpdateAssemblyOptions : ResourceCommandBaseOptions
    {
        [OptionArray('f', "filters", DefaultValue = new string[] { "*.dll" }, HelpText = "Set of wildcard patterns.")]
        public override string[] Filters { get; set; }

        [Option("database", DefaultValue = true, HelpText = "Assembly is deployed to database.", MutuallyExclusiveSet = "Isolation")]
        public bool Database { get; set; }

        [Option("disk", DefaultValue = false, HelpText = "Assembly is deployed to disk.", MutuallyExclusiveSet = "Isolation")]
        public bool Disk { get; set; }

        [Option("sandbox", DefaultValue = false, HelpText = "Assembly is deployed in sandbox isolation mode.")]
        public bool Sandbox { get; set; }

        public override Type GetCommandType()
        {
            return typeof(UpdateAssemblyCommand);
        }
    }

    public class UpdateAssemblyCommand : ResourceCommandBase<UpdateAssemblyOptions>
    {
        public UpdateAssemblyCommand(IConfigurationProvider configurationProvider, LoggerBase logger, UpdateAssemblyOptions options)
            : base(configurationProvider, logger, options)
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
                var resource = CrmContext.CreateQuery("pluginassembly").FirstOrDefault(i => (string)i["name"] == name);

                var nresource = new Entity("pluginassembly");
                nresource["name"] = name;

                var sourcetype = 0;
                if (Options.Database)
                {
                    sourcetype = 0;
                }
                
                if (Options.Disk)
                {
                    sourcetype = 1;
                }

                nresource["sourcetype"] = new OptionSetValue(sourcetype);

                if (Options.Sandbox)
                {
                    nresource["isolationmode"] = new OptionSetValue(2);
                }
                else
                {
                    nresource["isolationmode"] = new OptionSetValue(1);
                }

                OrganizationRequest request;
                if (resource != null)
                {
                    if (!Options.Force && (DateTime)resource["modifiedon"] >= file.LastWriteTime.ToUniversalTime())
                    {
                        Logger.Write(log, category.Compose("Ignore"), relativeFilePath);
                        Logger.Write(log.ToString());
                        return false;
                    }

                    Logger.Write(log, category.Compose("Update"), relativeFilePath);
                    nresource.Id = resource.Id;
                    request = new UpdateRequest() { Target = nresource };
                }
                else
                {
                    Logger.Write(log, category.Compose("Create"), relativeFilePath);
                    request = new CreateRequest() { Target = nresource };
                }

                if (!string.IsNullOrEmpty(category) && index.HasValue)
                {
                    category = category.Compose(index);
                }

                var fileBytes = File.ReadAllBytes(file.FullName);
                nresource.Attributes["content"] = Convert.ToBase64String(fileBytes);

                CrmService.Execute(request);
            }
            catch (Exception ex)
            {
                Logger.Write(log, BaseName, ex);
                ret = false;
            }

            Logger.Write(log.ToString());

            return ret;
        }
    }
}
