using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Cream.Sdk;

namespace Cream.Commands
{
    public class UpdateWebResourceOptions : UpdateResourceCommandBaseOptions
    {
        [OptionArray('f', "filters", DefaultValue = new string[] { "*.html", "*.htm", "*.css", "*.js", "*.gif", "*.png", "*.jpg", "*.xml", "*.zap" }, HelpText = "Set of wildcard patterns.")]
        public override string[] Filters { get; set; }

        public override Type GetCommandType()
        {
            return typeof(UpdateWebResourceCommand);
        }
    }

    public class UpdateWebResourceCommand : ResourceCommandBase<UpdateWebResourceOptions>
    {
        public UpdateWebResourceCommand(ICrmServiceProvider crmServiceProvider, LoggerBase logger, UpdateWebResourceOptions options)
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
                if (index.HasValue) {
                    category = "({0:N0})".Compose(index.Value);
                }
                category += " {0}";

                var relativeFilePath = GetRelativePath(file, Options.Path);
                var type = GetWebResourceType(file);
                if (type == 0)
                {
                    if (Options.Force)
                    {
                        type = 4; // XML Data
                    }
                    else
                    {
                        Logger.Write(log, category.Compose("Invalid"), relativeFilePath);
                        Logger.Write(log.ToString());
                        return false;
                    }                    
                }

                var name = file.Name;
                var eresource = (from record in CrmContext.CreateQuery("webresource")
                                        where (string)record["name"] == name
                                        select new
                                        {
                                            Id = record.Id,
                                            ModifiedOn = (DateTime)record["modifiedon"]
                                        }).FirstOrDefault();

                var nresource = new WebResource();
                nresource.Name = name;
                //nresource.Attributes["description"] = name;
                nresource.DisplayName = name;
                nresource.WebResourceType = new OptionSetValue(type);
                
                OrganizationRequest request;
                if (eresource != null)
                {
                    if (!Options.Force && eresource.ModifiedOn >= file.LastWriteTime.ToUniversalTime())
                    {
                        Logger.Write(log, category.Compose("Ignore"), relativeFilePath);
                        Logger.Write(log.ToString());
                        return false;
                    }

                    Logger.Write(log, category.Compose("Update"), relativeFilePath);
                    nresource.Id = eresource.Id;
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
                nresource.Content = Convert.ToBase64String(fileBytes);

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

        private int GetWebResourceType(FileInfo file)
        {
            var ext = file.Extension.ToLower();

            if (ext == ".html" || ext == ".htm")
            {
                return 1;
            }
            else if (ext == ".css")
            {
                return 2;
            }
            else if (ext == ".js")
            {
                return 3;
            }
            else if (ext == ".xml")
            {
                return 4;
            }
            else if (ext == ".png")
            {
                return 5;
            }
            else if (ext == ".jpg")
            {
                return 6;
            }
            else if (ext == ".gif")
            {
                return 7;
            }
            else if (ext == ".xap")
            {
                return 8;
            }
            else if (ext == ".xsl" || ext == ".xslt")
            {
                return 9;
            }
            else if (ext == ".ico")
            {
                return 10;
            }

            return 0;
        }

    }

}
