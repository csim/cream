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
using Microsoft.Xrm.Sdk;

namespace Cream.Commands
{
    public class CommandFactory : IDisposable
    {
        public CommandFactory()
        {
        }

        public IKernel Kernel { get; set; }

        public virtual void Bind()
        {
            Bind<
                DefaultConfigurationProvider,
                DefaultLogger,
                DefaultCrmServiceProvider<OrganizationService, CrmOrganizationServiceContext>
            >();
        }

        public virtual void Bind<
                TConfiguration,
                TLogger,
                TCrmServiceProvider
            >()
            where TLogger : LoggerBase
            where TConfiguration : IConfigurationProvider
            where TCrmServiceProvider : ICrmServiceProvider
        {
            Kernel = new StandardKernel();

            Kernel.Bind<ICrmServiceProvider>()
                .To<TCrmServiceProvider>()
                .InSingletonScope();

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
