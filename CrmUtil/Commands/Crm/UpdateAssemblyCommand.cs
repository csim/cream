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

        public override Type GetCommandType()
        {
            return typeof(UpdateAssemblyCommand);
        }
    }

    public class UpdateAssemblyCommand : ResourceCommandBase
    {
        public UpdateAssemblyCommand(IConfigurationProvider configurationProvider, LoggerBase logger, UpdateAssemblyOptions options)
            : base(configurationProvider, logger, options)
        {
        }

        protected override bool UpdateSingle(FileInfo file, int? index = null)
        {
            var ret = true;
            string category = null;
            var log = new StringBuilder();

            try
            {
                category = null;

                var name = file.Name;
                var fileBytes = File.ReadAllBytes(file.FullName);
                var resource = CrmContext.CreateQuery("webresource").FirstOrDefault(i => (string)i["name"] == name);

                var nresource = new Entity("webresource");
                nresource.Attributes["name"] = name;
                nresource.Attributes["description"] = name;
                //resource.Attributes["logicalname"] = name;
                nresource.Attributes["displayname"] = name;
                nresource.Attributes["content"] = Convert.ToBase64String(fileBytes);
                //nresource.Attributes["webresourcetype"] = new OptionSetValue(type);
                
                OrganizationRequest request;
                if (resource != null)
                {
                    category = "({0:N0}) Update";
                    nresource.Id = resource.Id;
                    request = new UpdateRequest() { Target = nresource };
                }
                else
                {
                    category = "({0:N0}) Create";
                    request = new CreateRequest() { Target = nresource };
                }
                
                if (!string.IsNullOrEmpty(category) && index.HasValue)
                {
                    category = category.Compose(index);
                }
                Logger.Write(log, category, "{0} ... ".Compose(GetRelativePath(file, Options.Path)));
                CrmService.Execute(request);
                Logger.Write(log, category, "Done.");
            }
            catch (Exception ex)
            {
                Logger.Write(log, !string.IsNullOrEmpty(category) ? category : BaseName, ex);
                ret = false;
            }

            Logger.Write(log.ToString());

            return ret;
        }
    }
}
