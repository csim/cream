﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace CrmUtil.Commands
{
    public abstract class CommonOptions
    {
        [Option("host", Required = false, HelpText = "CRM Host Server. https://contoso.api.crm.dynamics.com")]
        public string HostUrl { get; set; }

        [Option("username", Required = false, HelpText = "CRM Username.")]
        public string Username { get; set; }

        [Option("password", Required = false, HelpText = "CRM Password.")]
        public string Password { get; set; }

        [Option("domain", Required = false, HelpText = "CRM Domain.")]
        public string Domain { get; set; }

        [Option("debug", Required = false, HelpText = "Launch debugger.")]
        public bool Debug { get; set; }

        [Option("nopublish", Required = false, DefaultValue = false, HelpText = "CRM Domain.")]
        public bool NoPublish { get; set; }

    }
}
