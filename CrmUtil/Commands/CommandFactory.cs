using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public IKernel Kernel { get; set; }

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

        public virtual LoggerBase GetLogger()
        {
            if (Kernel == null) { Setup(); }
            return Kernel.Get<LoggerBase>();
        }

        public ICommand GetCommand(object options)
        {
            if (options == null) return null;
            if (Kernel == null) Setup();
            if (!(options is CrmCommonOptions)) return null;

            var targetType = ((CrmCommonOptions)options).GetCommandType();
            var instance = Kernel.Get(targetType, new ConstructorArgument("options", options, false));
            if (!(instance is ICommand)) return null;
            return (ICommand)instance;
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
