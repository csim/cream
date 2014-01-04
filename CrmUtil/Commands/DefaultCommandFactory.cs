using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrmUtil.Logging;
using Ninject;

namespace CrmUtil.Commands
{
    public class DefaultCommandFactory : ICommandFactory, IDisposable
    {
        public DefaultCommandFactory()
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
            where TLogger : Logger
        {
            Kernel = new StandardKernel();

            Kernel.Bind<IConfigurationProvider>()
                .To<TConfiguration>()
                .InSingletonScope();

            Kernel.Bind<Logger>()
                .To<TLogger>()
                .InSingletonScope();

        }

        public virtual Logger GetLogger()
        {
            if (Kernel == null) { Setup(); }

            return Kernel.Get<Logger>();
        }

        public TDependency GetDependency<TDependency>()
        {
            if (Kernel == null) { Setup(); }

            return Kernel.Get<TDependency>();
        }

        public void Dispose()
        {
            Kernel.Dispose();
            Kernel = null;
        }
    }
}
