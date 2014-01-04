//-----------------------------------------------------------------------
// <copyright file="AppConfiguration.cs" company="Avanade">
//     MS-PL
// </copyright>
//-----------------------------------------------------------------------

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Suppressed.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "Suppressed.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1005:SingleLineCommentsMustBeginWithSingleSpace", Justification = "Suppressed.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1115:ParameterMustFollowComma", Justification = "Suppressed.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:ParametersMustBeOnSameLineOrSeparateLines", Justification = "Suppressed.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Suppressed.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Suppressed.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Suppressed.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1512:SingleLineCommentsMustNotBeFollowedByBlankLine", Justification = "Suppressed.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName", Justification = "Reviewed.")]

namespace CrmUtil.Configuration
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
