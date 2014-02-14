namespace Cream.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    /// <summary>
    /// Access values from the app.config file.
    /// </summary>
    public class Configuration : IConfiguration
    {
        private const string ENCRYPTION_KEY = "9jx2RBb3wOs5kPMHY5hPUzlbACPoefXq6QtysSeHeMYvZ1cYf7SXcqipdPKpYVTDLzKnoXz8s0waHZLN78nz";

        public CreamConfiguration ConfigurationData { get; set; }


        public Configuration()
        {
        }

        public void Load(string path)
        {
            var file = new FileInfo(path);
            if (file.Exists)
            {
                var txt = File.ReadAllText(file.FullName);
                ConfigurationData = JsonConvert.DeserializeObject<CreamConfiguration>(txt);
            }
            else
            {
                ConfigurationData = new CreamConfiguration();
            }
        }

        public void Save(string path)
        {
            var file = new FileInfo(path);
            var content = JsonConvert.SerializeObject(ConfigurationData, Formatting.Indented);
            File.WriteAllText(file.FullName, content);
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
            if (!ConfigurationData.Connections.ContainsKey(name))
            {
                throw new Exception("ConnectionString {0} not found.".Compose(name));
            }

            var connectionString = ConfigurationData.Connections[name];

            var pattern = "(password)=#(.+?)#;";
            var rflags = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnoreCase;
            var regex = new Regex(pattern, rflags);
            var match = regex.Match(connectionString);
            if (match.Success)
            {
                var cipherText = Decrypt(match.Groups[2].Value);
                connectionString = Regex.Replace(connectionString, pattern, i => string.Format("{0}={1};", match.Groups[1].Value, cipherText), rflags);
            }

            //Console.WriteLine(connectionString);
            return connectionString;
        }

        public void SetConnection(string name, string connectionString)
        {

            var rflags = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnoreCase;
            var pattern = "(password)=(.+?);";
            var regex = new Regex(pattern, rflags);
            var match = regex.Match(connectionString);
            if (match.Success)
            {
                var cipherText = Encrypt(match.Groups[2].Value);
                connectionString = Regex.Replace(connectionString, pattern, i => string.Format("{0}=#{1}#;", match.Groups[1].Value, cipherText), rflags);
            }

            ConfigurationData.Connections[name] = connectionString;
        }

        public void RemoveConnection(string name)
        {
            if (ConfigurationData.Connections.ContainsKey(name)) {
                ConfigurationData.Connections.Remove(name);
            }
        }

        public string Encrypt(string clearText)
        {
            // Compute key 
            SHA384Managed sha = new SHA384Managed();
            byte[] b = sha.ComputeHash(new ASCIIEncoding().GetBytes(ENCRYPTION_KEY));
            byte[] Key = new byte[32];
            byte[] Vector = new byte[16];
            Array.Copy(b, 0, Key, 0, 32);
            Array.Copy(b, 32, Vector, 0, 16);

            byte[] data = new ASCIIEncoding().GetBytes(clearText);

            var crypto = new RijndaelManaged();
            var encryptor = crypto.CreateEncryptor(Key, Vector);

            var memoryStream = new MemoryStream();
            var crptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            crptoStream.Write(data, 0, data.Length);
            crptoStream.FlushFinalBlock();

            crptoStream.Close();
            memoryStream.Close();

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public string Decrypt(string cipherString)
        {
            SHA384Managed sha = new SHA384Managed();
            byte[] b = sha.ComputeHash(new ASCIIEncoding().GetBytes(ENCRYPTION_KEY));
            byte[] Key = new byte[32];
            byte[] Vector = new byte[16];
            Array.Copy(b, 0, Key, 0, 32);
            Array.Copy(b, 32, Vector, 0, 16);

            byte[] cipher = Convert.FromBase64String(cipherString);

            var crypto = new RijndaelManaged();
            var encryptor = crypto.CreateDecryptor(Key, Vector);

            var memoryStream = new MemoryStream(cipher);
            var crptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Read);

            var data = new byte[cipher.Length];
            var dataLength = crptoStream.Read(data, 0, data.Length);

            memoryStream.Close();
            crptoStream.Close();

            return (new ASCIIEncoding()).GetString(data, 0, dataLength);
        }

    }
}
