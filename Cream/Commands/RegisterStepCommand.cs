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
using Ninject;

namespace Cream.Commands
{
    public class RegisterStepOptions : OptionBase
    {
        [Option('t', "type", Required = true, HelpText = "Fully qualified assembly class type name.")]
        public string Type { get; set; }

        [Option('e', "entity", Required = true, HelpText = "Entity for which the step will be registered.")]
        public string Entity { get; set; }

        [Option('m', "message", Required = true, HelpText = "SDK message associated with this plugin.")]
        public string Message { get; set; }

        [Option("mode", DefaultValue = true, HelpText = "Plugin Mode. [ Synchronous, Asynchronous ]")]
        public PluginMode Mode { get; set; }

        [Option("async", DefaultValue = false, HelpText = "Register asynchronous plugin step.")]
        public bool Asynchronous { get; set; }

        [Option("stage", DefaultValue = PluginStage.Post, HelpText = "Plugin stage. [ Prevalidation, Pre, Post ]")]
        public PluginStage Stage { get; set; }

        [Option("rank", DefaultValue = 1, HelpText = "Rank order at which this step executes.")]
        public int Rank { get; set; }

        public override Type GetCommandType()
        {
            return typeof(RegisterStepCommand);
        }
    }

    public enum PluginStage
    {
        Prevalidation = 10,
        Pre = 20,
        Post = 40
    }

    public enum PluginMode
    {
        Synchronous = 0,
        Asynchronous = 1
    }

    public class RegisterStepCommand : CommandBase<RegisterStepOptions>
    {
        public RegisterStepCommand(IKernel resolver, RegisterStepOptions options)
            : base(resolver, options)
        {
        }

        public override void Execute()
        {
            WarmupCrmService();

            var type = (from r in Context.CreateQuery("plugintype")
                        where (string)r["typename"] == Options.Type
                        select new { Id = r.Id, TypeName = (string)r["typename"] }
                        ).FirstOrDefault();

            if (type == null)
            {
                throw new Exception("PluginType not found. ({0})".Compose(Options.Type));
            }
            
            var sdkmessage = (from r in Context.CreateQuery("sdkmessage")
                              where (string)r["name"] == Options.Message
                              select new { r.Id }
                              ).FirstOrDefault();

            if (sdkmessage == null)
            {
                throw new Exception("SDKMessage not found. ({0})".Compose(Options.Message));
            }

            // TODO: create and associate sdkmessagefilter
            // TODO: Set eventhandler

            var efilter = (from r in Context.CreateQuery("sdkmessageprocessingstep")
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
            nstep["name"] = "{0} {1} {2} {3}".Compose(type.TypeName, Options.Message, Options.Message, Options.Mode, Options.Stage);
            nstep["mode"] = new OptionSetValue((int)Options.Mode);
            nstep["stage"] = new OptionSetValue((int)Options.Stage);
            nstep["plugintypeid"] = new EntityReference("plugintype", type.Id);
            nstep["sdkmessageid"] = new EntityReference("sdkmessage", sdkmessage.Id);
            nstep["rank"] = Options.Rank;

            var estep = (from r in Context.CreateQuery("sdkmessageprocessingstep")
                                    where
                                        ((EntityReference)r["plugintypeid"]).Id == type.Id
                                        && ((EntityReference)r["sdkmessageid"]).Id == sdkmessage.Id
                                    select new { Id = r.Id }
                                    ).FirstOrDefault();

            if (estep != null)
            {
                Logger.Write("Update", type.TypeName);
                nstep.Id = estep.Id;
                request = new UpdateRequest() { Target = nstep };
            }
            else
            {
                Logger.Write("Create", type.TypeName);
                request = new CreateRequest() { Target = nstep };
            }

            Service.Execute(request);

        }

        private int GetObjectTypeCode(string entitylogicalname)
        {
            var entity = new Entity(entitylogicalname);
            var request = new RetrieveEntityRequest();
            request.LogicalName = entity.LogicalName;
            request.EntityFilters = EntityFilters.All;
            var response = (RetrieveEntityResponse)Service.Execute(request);
            var ent = (EntityMetadata)response.EntityMetadata;
            return ent.ObjectTypeCode ?? 0;

        }
    }
}
