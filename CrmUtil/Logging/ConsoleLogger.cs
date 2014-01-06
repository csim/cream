﻿namespace CrmUtil.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CrmUtil.Providers;

    public class ConsoleLogger : LoggerBase
    {
        public ConsoleLogger(IConfigurationProvider configuration) 
            : base(configuration)
        {
            Writers.Add(new ConsoleLogWriter());
        }
    }
}
