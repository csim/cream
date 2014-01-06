namespace Cream.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Access values from the app.config file.
    /// </summary>
    public class DefaultConfigurationProvider : IConfigurationProvider
    {
        public DefaultConfigurationProvider()
        {
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
                var value = GetVirtualSetting(key);
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
            var value = GetVirtualSetting(key);
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

            return defaultValue;
        }

        private string GetVirtualSetting(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                return ConfigurationManager.AppSettings[key];
            }

            return null;
        }
    }
}
