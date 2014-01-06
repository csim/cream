using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace CrmUtil.Commands.Crm
{
    public abstract class CrmCommonOptionBase : CommonOptionsBase
    {
        [Option("server", Required = false, HelpText = "CRM Server URL. http://localhost/contoso or https://contoso.api.crm.dynamics.com")]
        public string ServerUrl { get; set; }

        [Option("environment", Required = false, HelpText = "Prefix used in app.config settings for each environment.")]
        public string Environment { get; set; }

        [Option("username", Required = false, HelpText = "CRM Username.")]
        public string Username { get; set; }

        [Option("password", Required = false, HelpText = "CRM Password.")]
        public string Password { get; set; }

        [Option("domain", Required = false, HelpText = "CRM Domain.")]
        public string Domain { get; set; }
    }
}
