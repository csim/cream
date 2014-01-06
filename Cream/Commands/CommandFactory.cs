using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cream.Providers;
using Cream.Logging;
using Ninject;
using Ninject.Parameters;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using System.Configuration;

namespace Cream.Commands
{
    public class CommandFactory : IDisposable
    {
        public CommandFactory()
        {
        }

        protected IKernel Kernel { get; set; }

        public virtual void Bind()
        {
            Bind<
                DefaultCrmServiceProvider,
                DefaultLogger,
                DefaultConfigurationProvider
            >();
        }

        public virtual void Bind<
                TCrmServiceProvider,
                TLogger,
                TConfiguration
            >()
            where TCrmServiceProvider : ICrmServiceProvider
            where TLogger : LoggerBase
            where TConfiguration : IConfigurationProvider
        {
            Kernel = new StandardKernel();

            Kernel.Bind<ICrmServiceProvider>()
                .To<TCrmServiceProvider>()
                .InThreadScope();

            Kernel.Bind<OrganizationService>()
                .To<OrganizationService>()
                .InThreadScope();

            Kernel.Bind<CrmOrganizationServiceContext>()
                .To<CrmOrganizationServiceContext>()
                .InThreadScope();

            Kernel.Bind<IConfigurationProvider>()
                .To<TConfiguration>()
                .InSingletonScope();

            Kernel.Bind<LoggerBase>()
                .To<TLogger>()
                .InSingletonScope();
        }

        public ICommand GetCommand(OptionBase options)
        {
            if (Kernel == null) { Bind(); }

            var type = options.GetCommandType();
            var ret = (ICommand)Kernel.Get(type, new ConstructorArgument("options", options, false));

            if (options is OptionBase)
            {
                ret.CrmServiceProvider.Initialize((OptionBase)options);
            }

            return ret;
        }


        public TDependency GetDependency<TDependency>()
        {
            if (Kernel == null) { Bind(); }

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
