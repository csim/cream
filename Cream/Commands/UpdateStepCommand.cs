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
using Microsoft.Xrm.Sdk.Metadata;

namespace Cream.Commands
{
    public class UpdateStepOptions : OptionBase
    {
        [Option('t', "type", Required = true, HelpText = "Fully qualified assembly class type name.")]
        public string Type { get; set; }

        [Option('e', "entity", Required = true, HelpText = "Entity for which the step will be registered.")]
        public string Entity { get; set; }

        [Option('m', "message", Required = true, HelpText = "SDK message associated with this plugin.")]
        public string Message { get; set; }

        [Option("sync", DefaultValue = true, HelpText = "Register synchronous plugin step.", MutuallyExclusiveSet = "Mode")]
        public bool Synchronous { get; set; }

        [Option("async", DefaultValue = false, HelpText = "Register asynchronous plugin step.", MutuallyExclusiveSet = "Mode")]
        public bool Asynchronous { get; set; }

        [Option("prevalidation", DefaultValue = false, HelpText = "Register in pre-validation plugin stage.", MutuallyExclusiveSet = "Stage")]
        public bool Prevalidation { get; set; }

        [Option("pre", DefaultValue = false, HelpText = "Register in pre-operation plugin stage.", MutuallyExclusiveSet = "Stage")]
        public bool Pre { get; set; }

        [Option("post", DefaultValue = true, HelpText = "Register in post-operation plugin stage.", MutuallyExclusiveSet = "Stage")]
        public bool Post { get; set; }

        [Option("rank", DefaultValue = 1, HelpText = "Rank order at which this step executes.")]
        public int Rank { get; set; }

        public override Type GetCommandType()
        {
            return typeof(UpdateStepCommand);
        }
    }

    public enum PluginStage
    {
        Prevalidation = 10,
        Pre = 20,
        Post = 40
    }

    public class UpdateStepCommand : CommandBase<UpdateStepOptions>
    {
        public UpdateStepCommand(ICrmServiceProvider crmServiceProvider, LoggerBase logger, UpdateStepOptions options)
            : base(crmServiceProvider, logger, options)
        {
        }

        public override void Execute()
        {
            WarmupCrmService();

            var type = (from r in CrmContext.CreateQuery("plugintype")
                        where (string)r["typename"] == Options.Type
                        select new { Id = r.Id, Name = (string)r["typename"] }
                        ).FirstOrDefault();

            if (type == null)
            {
                throw new Exception("PluginType not found. ({0})".Compose(Options.Type));
            }
            
            var sdkmessage = (from r in CrmContext.CreateQuery("sdkmessage")
                              where (string)r["name"] == Options.Message
                              select new { r.Id }
                              ).FirstOrDefault();

            if (sdkmessage == null)
            {
                throw new Exception("SDKMessage not found. ({0})".Compose(Options.Message));
            }

            var mode = 0;
            if (Options.Synchronous) mode = 0;
            if (Options.Asynchronous) mode = 1;

            var stage = 0;
            if (Options.Prevalidation) stage = 10;
            if (Options.Pre) stage = 20;
            if (Options.Post) stage = 40;


            // TODO: create and associate sdkmessagefilter
            // TODO: Set eventhandler

            var efilter = (from r in CrmContext.CreateQuery("sdkmessageprocessingstep")
                         where
                             ((EntityReference)r["plugintypeid"]).Id == type.Id
                             && ((EntityReference)r["sdkmessageid"]).Id == sdkmessage.Id
                         select new { Id = r.Id }
                                    ).FirstOrDefault();

            OrganizationRequest request;

            //var nfilter = new Entity("sdkmessagefilter");
            //nfilter["primaryobjecttypecode"] = GetObjectTypeCode(Options.Entity);
            //nfilter["sdkmessageid"] = new EntityReference("sdkmessage", sdkmessage.Id);
   
            //request = new CreateRequest() { Target = nfilter };
            //CrmService.Execute(request);

            var nstep = new Entity("sdkmessageprocessingstep");
            nstep["name"] = "{0} {1} {2}".Compose(Options.Message, Options.Message, Options.Synchronous ? "Synchronous" : "Asynchronous");
            nstep["mode"] = new OptionSetValue(mode);
            nstep["stage"] = new OptionSetValue(stage);
            nstep["plugintypeid"] = new EntityReference("plugintype", type.Id);
            nstep["sdkmessageid"] = new EntityReference("sdkmessage", sdkmessage.Id);
            nstep["rank"] = Options.Rank;

            var estep = (from r in CrmContext.CreateQuery("sdkmessageprocessingstep")
                                    where
                                        ((EntityReference)r["plugintypeid"]).Id == type.Id
                                        && ((EntityReference)r["sdkmessageid"]).Id == sdkmessage.Id
                                    select new { Id = r.Id }
                                    ).FirstOrDefault();

            if (estep != null)
            {
                Logger.Write("Update", type.Name);
                nstep.Id = estep.Id;
                request = new UpdateRequest() { Target = nstep };
            }
            else
            {
                Logger.Write("Create", type.Name);
                request = new CreateRequest() { Target = nstep };
            }

            CrmService.Execute(request);

        }

        private int GetObjectTypeCode(string entitylogicalname)
        {
            var entity = new Entity(entitylogicalname);
            var request = new RetrieveEntityRequest();
            request.LogicalName = entity.LogicalName;
            request.EntityFilters = EntityFilters.All;
            var response = (RetrieveEntityResponse)CrmService.Execute(request);
            var ent = (EntityMetadata)response.EntityMetadata;
            return ent.ObjectTypeCode ?? 0;

        }
    }
}
