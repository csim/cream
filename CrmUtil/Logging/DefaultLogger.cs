namespace CrmUtil.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CrmUtil.Configuration;

    public class DefaultLogger : LoggerBase
    {
        public DefaultLogger(IConfigurationProvider configuration) 
            : base(configuration)
        {
            Writers.Add(new ConsoleLogWriter());
            Writers.Add(new FileLogWriter(configuration));
        }
    }
}
