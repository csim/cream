namespace CrmUtil.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;

    public interface IConfigurationProvider
    {
        T GetSetting<T>(string keys, T defaultValue = default(T));

        T GetSetting<T>(string[] keys, T defaultValue = default(T));
    }

}