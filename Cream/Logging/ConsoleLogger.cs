namespace Cream.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Cream.Providers;

    public class ConsoleLogger : LoggerBase
    {
        public ConsoleLogger() 
            : base()
        {
            Writers.Add(new ConsoleLogWriter());
        }
    }
}
