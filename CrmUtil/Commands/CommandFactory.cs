using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrmUtil.Providers;
using CrmUtil.Logging;
using Ninject;
using Ninject.Parameters;
using CrmUtil.Commands.Crm;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using System.Configuration;

namespace CrmUtil.Commands
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

        public ICommand GetCommand(CommonOptionsBase options)
        {
            if (Kernel == null) { Bind(); }

            var type = options.GetCommandType();
            var ret = (ICommand)Kernel.Get(type, new ConstructorArgument("options", options, false));

            if (ret is ICrmCommand && options is CrmCommonOptionBase)
            {
                ((ICrmCommand)ret).CrmServiceProvider.Initialize((CrmCommonOptionBase)options);
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
