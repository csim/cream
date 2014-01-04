using System;
namespace CrmUtil.Commands
{
    interface ICommandFactory
    {
        TDependency GetDependency<TDependency>();
        void Setup();
        void Setup<TConfiguration, TLogger>()
            where TConfiguration : CrmUtil.IConfigurationProvider
            where TLogger : CrmUtil.Logging.Logger;
    }
}
