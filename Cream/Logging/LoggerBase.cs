//-----------------------------------------------------------------------
// <copyright file="Logger.cs" company="Avanade">
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
    using System.Text;
    using Cream.Providers;

    /// <summary>
    /// Logs information to the console and a text log file.
    /// </summary>
    public abstract class LoggerBase : IDisposable
    {
        public LoggerBase(IConfigurationProvider configuration)
        {
            Configuration = configuration;
            Writers = new List<ILogWriter>();
        }

        public List<ILogWriter> Writers { get; set; }

        private IConfigurationProvider Configuration { get; set; }

        public void Write(string category, Exception ex)
        {
            string fex = "{0}\n\n{1}\n".Compose(ex.Message, ex.Format());
            Write(category, fex, true);
        }

        public void Write(StringBuilder builder, string category, Exception ex)
        {
            string fex = string.Format("{0}\n\n{1}\n", ex.Message, ex.Format());
            Write(builder, category, fex, true);
        }

        public void Write(string category, string message, bool timestamp = false)
        {
            if (message == null) { message = string.Empty; }
            var msg = FormatMessage(category, message, timestamp);
            Write(msg);
        }

        public void Write(StringBuilder builder, string category, string message, bool timestamp = false)
        {
            if (string.IsNullOrEmpty(message)) { message = string.Empty; }
            var msg = FormatMessage(category, message, timestamp);
            builder.AppendLine(msg);
        }

        public void Write(string message)
        {
            lock (this)
            {
                foreach (var writer in Writers)
                {
                    writer.Write(message);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool managed)
        {
            foreach (var writer in Writers)
            {
                writer.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        private string FormatMessage(string category, string message, bool timestamp)
        {
            var smask = "[ {1,11} ] {2}";
            var vmask = "[ {1,11} | {0:yyyy-MM-dd HH:mm:ss} ] {2}";
            var mask = timestamp ? vmask : smask;

            return string.Format(mask, DateTime.Now, category, message);
        }
    }
}
