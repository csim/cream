﻿namespace Cream.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Access values from the app.config file.
    /// </summary>
    public class DefaultConfiguration : IConfiguration
    {
        public CreamConfiguration ConfigurationData { get; set; }

        public FileInfo ConfigurationFile { get; set; }

        public DefaultConfiguration()
        {
        }

        public void Load(string path)
        {
            ConfigurationFile = new FileInfo(path);
            if (ConfigurationFile.Exists)
            {
                var txt = File.ReadAllText(ConfigurationFile.FullName);
                ConfigurationData = JsonConvert.DeserializeObject<CreamConfiguration>(txt);
            }
            else
            {
                ConfigurationData = new CreamConfiguration();
            }
        }

        public void Save() {
            var content = JsonConvert.SerializeObject(ConfigurationData, Formatting.Indented);
            File.WriteAllText(ConfigurationFile.FullName, content);
        }

        /// <summary>
        /// Gets an AppSetting value from the app.config file.
        /// </summary>
        /// <param name="key">AppSetting key from the app.config file.</param>
        /// <returns>AppSetting value.</returns>
        public string GetSetting(string key)
        {
            return GetSetting(key, (string)null);
        }

        /// <summary>
        /// Gets a typed value from the app.config file.
        /// </summary>
        /// <typeparam name="T">Data type of the value.</typeparam>
        /// <param name="keys">Keys to be searched for the value.</param>
        /// <param name="defaultValue">Default value to be return in the event that no key is available</param>
        /// <returns>AppSetting value.</returns>
        public T GetSetting<T>(string[] keys, T defaultValue = default(T))
        {
            foreach (var key in keys)
            {
                var value = ConfigurationManager.AppSettings[key];
                if (value != null)
                {
                    if (defaultValue is bool)
                    {
                        return (T)Convert.ChangeType(true, typeof(T));
                    }
                    else
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets a typed value from the app.config file
        /// </summary>
        /// <typeparam name="T">Data type of the AppSetting value</typeparam>
        /// <param name="key">Key to be searched for the value.</param>
        /// <param name="defaultValue">Default value to be return in the event that no key is available</param>
        /// <returns>AppSetting value.</returns>
        public T GetSetting<T>(string key, T defaultValue = default(T))
        {
            if (ConfigurationData.AppSettings.ContainsKey(key))
            {
                var value = ConfigurationData.AppSettings[key];
                if (value != null)
                {
                    if (defaultValue is bool)
                    {
                        return (T)Convert.ChangeType(true, typeof(T));
                    }
                    else
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                }
            }

            return defaultValue;
        }

        public string GetConnectionString(string name)
        {
            if (ConfigurationData.Connections.ContainsKey(name))
            {
                return ConfigurationData.Connections[name];
            }

            return null;
        }
    }
}