//-----------------------------------------------------------------------
// <copyright file="FileLogWriter.cs" company="Avanade">
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

namespace Cream.Logging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Cream.Providers;

    public class FileLogWriter : ILogWriter, IDisposable
    {
        /// <summary>
        /// Fully composed log file path.
        /// </summary>
        public string LogFilePath { get; set; }

        private object WriteLock { get; set; }

        /// <summary>
        /// Stream used to write the log file.
        /// </summary>
        private StreamWriter LogStream { get; set; }

        public FileLogWriter()
        {
            WriteLock = new object();
        }

        private void InitLogStream() 
        {
            LogFilePath = "{1}_{0:yyyy-MM-dd_HH}.log";

            var baseName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Replace(".exe", string.Empty));
            LogFilePath = LogFilePath.Compose(DateTime.Now, baseName);
            LogFilePath = Path.GetFullPath(LogFilePath);

            EnsureLogDirectory();
            LogStream = File.AppendText(LogFilePath);
        }

        private void EnsureLogDirectory()
        {
            var dir = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(dir)) 
            {
                Directory.CreateDirectory(dir);
            }
        }

        public void Write(string message)
        {
            if (LogStream == null)
            {
                InitLogStream();
            }

            lock (WriteLock)
            {
                if (message.EndsWith(Environment.NewLine))
                {
                    LogStream.Write(message);
                }
                else
                {
                    LogStream.WriteLine(message);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool managed)
        {
            if (LogStream != null)
            {
                //LogStream.Flush();
                LogStream.Close();
                LogStream.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
