﻿namespace Cream.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;

    public interface IConfiguration
    {
        CreamConfiguration ConfigurationData { get; set; }

        T GetSetting<T>(string keys, T defaultValue = default(T));

        T GetSetting<T>(string[] keys, T defaultValue = default(T));

        string GetConnectionString(string name);

        void Load(string path);
        
        void Save();
    }

}