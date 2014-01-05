using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrmUtil.Configuration;
using CrmUtil.Logging;
using Ninject;
using Ninject.Parameters;

namespace CrmUtil.Commands
{
    public class CommandFactory : IDisposable
    {
        public CommandFactory()
        {
        }

        protected IKernel Kernel { get; set; }

        public virtual void Setup()
        {
            Setup<
                DefaultConfigurationProvider,
                DefaultLogger
            >();
        }

        public virtual void Setup<
                TConfiguration,
                TLogger
            >()
            where TConfiguration : IConfigurationProvider
            where TLogger : LoggerBase
        {
            Kernel = new StandardKernel();

            Kernel.Bind<IConfigurationProvider>()
                .To<TConfiguration>()
                .InSingletonScope();

            Kernel.Bind<LoggerBase>()
                .To<TLogger>()
                .InSingletonScope();
        }

        public ICommand GetCommand(CommonOptionsBase options)
        {
            if (Kernel == null) { Setup(); }

            var type = options.GetCommandType();
            return (ICommand)Kernel.Get(type, new[] { new ConstructorArgument("options", options, false) });
        }

        public TDependency GetDependency<TDependency>()
        {
            if (Kernel == null) { Setup(); }

            return Kernel.Get<TDependency>();
        }

        public void Dispose()
        {
            if (Kernel != null)
            {
                Kernel.Dispose();
                Kernel = null;
            }
        }
    }
}
