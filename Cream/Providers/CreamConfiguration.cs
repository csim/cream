using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cream.Providers
{
    public class CreamConfiguration
    {
        public CreamConfiguration()
        {
            Connections = new Dictionary<string, string>();
            AppSettings = new Dictionary<string, string>();
        }

        public string LogFilePath { get; set; }

        public Dictionary<string, string> Connections { get; set; }

        public Dictionary<string, string> AppSettings { get; set; }
    }
}
