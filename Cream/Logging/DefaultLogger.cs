﻿namespace Cream.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Cream.Providers;

    public class DefaultLogger : LoggerBase
    {
        public DefaultLogger(IConfiguration configuration) 
            : base()
        {
            Writers.Add(new ConsoleLogWriter());
            Writers.Add(new FileLogWriter(configuration));
        }
    }
}
