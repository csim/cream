namespace Cream.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Cream.Providers;

    public class ConsoleLogger : LoggerBase
    {
        public ConsoleLogger(IConfigurationProvider configuration) 
            : base(configuration)
        {
            Writers.Add(new ConsoleLogWriter());
        }
    }
}
