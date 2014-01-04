//-----------------------------------------------------------------------
// <copyright file="ILoggerWriter.cs" company="Avanade">
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

namespace CrmUtil.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    public interface ILogWriter
    {
        void Write(string message);

        void Dispose();
    }
}