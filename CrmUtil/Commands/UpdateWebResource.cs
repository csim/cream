using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmUtil.Commands
{
    public class UpdateWebResourceOptions : CommonOptions
    {
        [Option('f', "filename", Required = false, HelpText = "Input file to be processed.")]
        public string Filename { get; set; }

        [Option('d', "directory", DefaultValue = ".", Required = false, HelpText = "Input directory to be processed.")]
        public string Directory { get; set; }

        [OptionArray("patterns", DefaultValue = new string[] { "*.html", "*.htm", "*.css", "*.js", "*.gif", "*.png", "*.jpg" }, HelpText = "Set of wildcard patterns.")]
        public string[] Patterns { get; set; }

        [Option('m', "monitor", Required = false, HelpText = "Monitor a file or directory for changes.")]
        public bool Monitor { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class UpdateWebResource : CommandBase
    {
        public UpdateWebResource(UpdateWebResourceOptions options) : base(options)
        {
        }

        public override void Execute() //UpdateWebResourceOptions options
        {
            var options = (UpdateWebResourceOptions)Options;
            if (options.Debug) System.Diagnostics.Debugger.Launch();

            List<string> files = new List<string>();

            if (!string.IsNullOrEmpty(options.Filename))
            {
                files.Add(options.Filename);
            }
            else
            {
                var dir = options.Directory;
                if (string.IsNullOrEmpty(dir))
                {
                    dir = Environment.CurrentDirectory;
                }

                foreach (var pat in options.Patterns) {
                    files.AddRange(Directory.GetFiles(dir, pat));
                }
            }

            foreach (var file in files)
            {
                UpdateSingleResource(file, options);
            }

            if (!options.NoPublish)
            {
                PublishAllCustomizations();
            }

        }

        private void UpdateSingleResource(string filePath, UpdateWebResourceOptions options)
        {
            var name = Path.GetFileName(filePath);
            var fileBytes = File.ReadAllBytes(filePath);
            var resource = Context.CreateQuery("webresource").FirstOrDefault(i => (string)i["name"] == name);

            var nresource = new Entity("webresource");
            nresource.Attributes["name"] = name;
            nresource.Attributes["description"] = name;
            //resource.Attributes["logicalname"] = name;
            nresource.Attributes["displayname"] = name;
            nresource.Attributes["content"] = Convert.ToBase64String(fileBytes);
            nresource.Attributes["webresourcetype"] = new OptionSetValue(3);

            OrganizationRequest request;
            if (resource != null)
            {
                nresource.Id = resource.Id;
                request = new UpdateRequest() { Target = nresource };
            }
            else
            {
                request = new CreateRequest() { Target = nresource };
            }

            Console.WriteLine("Updating {0}", filePath);
            Service.Execute(request);
        }

    }
}
