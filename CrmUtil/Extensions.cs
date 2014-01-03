//-----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Avanade">
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

namespace CrmUtil
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Xml;
    using System.Xml.XPath;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Integer extension methods.
    /// </summary>
    public static class IntExtensions
    {
        public static string ToUIFormat(this int input, bool base2)
        {
            string[] sizes = { "K", "M", "G" };

            int divisor = 1000;
            if (base2)
            {
                divisor = 1024;
            }

            int order = 0;
            while (input >= divisor && order + 1 < sizes.Length)
            {
                order++;
                input = input / divisor;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = string.Format("{0:0.##} {1}", input, sizes[order]);

            return result;
        }
    }

    /// <summary>
    /// String extension methods.
    /// </summary>
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static List<string> ToList(this string input, string delimiter)
        {
            return new List<string>(input.Split(delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
        }

        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return char.ToUpper(input[0]) + input.Substring(1);
        }

        public static string[] ToArray(this string input, string delimiter)
        {
            return input.Split(delimiter);
        }

        public static string[] Split(this string input, string delimiter)
        {
            return input.Split(delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        public static string Before(this string input, string delimiter)
        {
            var pos = input.IndexOf(delimiter);
            if (pos < 0)
            {
                return string.Empty;
            }

            return input.Substring(0, pos);
        }

        public static string After(this string input, string delimiter)
        {
            var pos = input.IndexOf(delimiter);
            if (pos < 0)
            {
                return string.Empty;
            }

            return input.Substring(pos + delimiter.Length);
        }

        public static string Compose(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static string HtmlFormat(this string s)
        {
            return s.Replace("\r\n", "<br />").Replace("\n", "<br />");
        }

        public static string HtmlEncode(this string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

        public static string HtmlDecode(this string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

        public static string UrlEncode(this string s)
        {
            return HttpUtility.UrlEncode(s);
        }

        public static string UrlDecode(this string s)
        {
            return HttpUtility.UrlDecode(s);
        }

        public static string UrlPathEncode(this string s)
        {
            return HttpUtility.UrlPathEncode(s);
        }

        public static string MD5(this string s)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] newdata = Encoding.Default.GetBytes(s);
            byte[] encrypted = md5.ComputeHash(newdata);
            return BitConverter.ToString(encrypted).Replace("-", string.Empty).ToLower();
        }

        public static string Replace(this string s, string pattern, string replacement, StringComparison comparisonType)
        {
            if (s == null) { return null; }
            if (string.IsNullOrEmpty(pattern)) { return s; }

            int lenPattern = pattern.Length;
            int idxPattern = -1;
            int idxLast = 0;

            StringBuilder result = new StringBuilder();

            while (true)
            {
                idxPattern = s.IndexOf(pattern, idxPattern + 1, comparisonType);
                if (idxPattern < 0)
                {
                    result.Append(s, idxLast, s.Length - idxLast);
                    break;
                }

                result.Append(s, idxLast, idxPattern - idxLast);
                result.Append(replacement);
                idxLast = idxPattern + lenPattern;
            }

            return result.ToString();
        }

        public static string Repeat(this string instr, int n)
        {
            var result = string.Empty;

            for (var i = 0; i < n; i++)
            {
                result += instr;
            }

            return result;
        }
    }

    /// <summary>
    /// string[] extension methods.
    /// </summary>
    public static class StringArrayExtensions
    {
        public static string Join(this string[] s, string separator)
        {
            return string.Join(separator, s);
        }
    }

    /// <summary>
    /// byte extension methods.
    /// </summary>
    public static class ByteExtensions
    {
        public static string MD5(this byte[] b)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] encrypted = md5.ComputeHash(b);
            return BitConverter.ToString(encrypted).Replace("-", string.Empty).ToLower();
        }
    }

    /// <summary>
    /// List extension methods.
    /// </summary>
    public static class ListExtensions
    {
        public static string Join(this List<string> input, string delimiter)
        {
            return string.Join(delimiter, input.ToArray());
        }
    }

    /// <summary>
    /// DataRow extension methods.
    /// </summary>
    public static class DataRowExtensions
    {
        public static Guid? Guid(this DataRow row, string fieldName)
        {
            Guid? ret = null;

            if (row.IsNull(fieldName)) { return null; }

            try
            {
                ret = new Guid(row.String(fieldName));
            }
            catch
            {
                return null;
            }

            return ret;
        }

        public static int? Int(this DataRow row, string fieldName)
        {
            if (row.IsNull(fieldName))
            {
                return null;
            }

            int ret;

            if (int.TryParse(row.String(fieldName), out ret))
            {
                return ret;
            }

            return null;
        }

        public static bool? Boolean(this DataRow row, string fieldName)
        {
            if (row.IsNull(fieldName)) { return null; }
            return Convert.ToBoolean(row[fieldName]);
        }

        public static string String(this DataRow row, string fieldName)
        {
            if (row.IsNull(fieldName)) { return null; }
            return Convert.ToString(row[fieldName]);
        }

        public static DateTime? Date(this DataRow row, string fieldName)
        {
            if (row.IsNull(fieldName)) { return null; }
            return Convert.ToDateTime(row[fieldName]);
        }

        public static bool TryGuid(this DataRow row, string fieldName, out Guid output)
        {
            output = default(Guid);
            if (row.IsNull(fieldName)) { return false; }
            try
            {
                output = new Guid(row.String(fieldName));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryInt(this DataRow row, string fieldName, out int output)
        {
            output = default(int);
            if (row.IsNull(fieldName)) { return false; }
            if (int.TryParse(row.String(fieldName), out output))
            {
                return true;
            }

            return false;
        }

        public static bool TryBoolean(this DataRow row, string fieldName, out bool output)
        {
            output = default(bool);
            if (row.IsNull(fieldName)) { return false; }
            output = Convert.ToBoolean(row[fieldName]);
            return true;
        }

        public static bool TryString(this DataRow row, string fieldName, out string output)
        {
            output = default(string);
            if (row.IsNull(fieldName)) { return false; }
            output = Convert.ToString(row[fieldName]);
            return true;
        }

        public static bool TryDate(this DataRow row, string fieldName, out DateTime output)
        {
            output = default(DateTime);
            if (row.IsNull(fieldName)) { return false; }
            output = Convert.ToDateTime(row[fieldName]);
            return true;
        }
    }

    /// <summary>
    /// SQLDataReader extension methods.
    /// </summary>
    public static class SqlDataReaderExtensions
    {
        public static Guid? Guid(this SqlDataReader reader, string fieldName)
        {
            Guid? ret = null;

            if (reader[fieldName] == null) { return null; }

            try
            {
                ret = new Guid(reader.String(fieldName));
            }
            catch
            {
                return null;
            }

            return ret;
        }

        public static int? Int(this SqlDataReader reader, string fieldName)
        {
            if (reader[fieldName] == null)
            {
                return null;
            }

            int ret;

            if (int.TryParse(reader.String(fieldName), out ret))
            {
                return ret;
            }

            return null;
        }

        public static bool? Boolean(this SqlDataReader reader, string fieldName)
        {
            if (reader[fieldName] == null) { return null; }
            return Convert.ToBoolean(reader[fieldName]);
        }

        public static string String(this SqlDataReader reader, string fieldName)
        {
            if (reader[fieldName] == null) { return null; }
            return Convert.ToString(reader[fieldName]);
        }

        public static DateTime? Date(this SqlDataReader reader, string fieldName)
        {
            if (reader[fieldName] == null) { return null; }
            return Convert.ToDateTime(reader[fieldName]);
        }
    }

    /// <summary>
    /// Object extension methods.
    /// </summary>
    public static class ObjectExtensions
    {
        public static void Dump(this object o)
        {
            var t = o.GetType();
            var output = string.Format("{0}\n\t{1}", t.FullName, o.ToJSON());
            var html = "<pre>" + output + "</pre>";

            Console.WriteLine(output);
        }

        public static string ToJSON(this object o)
        {
            return o.ToJSON(null, 10);
        }

        public static string ToJSON(this object o, IEnumerable<JavaScriptConverter> converters)
        {
            return o.ToJSON(converters, 10);
        }

        public static string ToJSON(this object o, IEnumerable<JavaScriptConverter> converters, int recursionLimit)
        {
            var ser = new JavaScriptSerializer();
            ser.RecursionLimit = recursionLimit;
            if (converters != null)
            {
                ser.RegisterConverters(converters);
            }

            return ser.Serialize(o);
        }
    }

    /// <summary>
    /// XPathNavigator &amp; XmlNode extension methods.
    /// </summary>
    public static class XmlExtensions
    {
        public static string ID(this XPathNavigator nav)
        {
            return nav.Attribute("ID");
        }

        public static string Name(this XPathNavigator nav)
        {
            return nav.Attribute("Name");
        }

        public static string Attribute(this XPathNavigator nav, string attributeName)
        {
            return Attribute(nav, attributeName, null);
        }

        public static string Attribute(this XPathNavigator nav, string attributeName, string defaultValue)
        {
            string val = nav.GetAttribute(attributeName, string.Empty);
            return string.IsNullOrEmpty(val) ? defaultValue : val;
        }

        public static bool AttributeBoolean(this XPathNavigator nav, string attributeName)
        {
            return AttributeBoolean(nav, attributeName, false);
        }

        public static bool AttributeBoolean(this XPathNavigator nav, string attributeName, bool defaultValue)
        {
            string val = nav.GetAttribute(attributeName, string.Empty);
            if (string.IsNullOrEmpty(val)) { return defaultValue; }

            return Convert.ToBoolean(val);
        }

        public static DateTime AttributeDateTime(this XPathNavigator nav, string attributeName)
        {
            return AttributeDateTime(nav, attributeName, DateTime.Now);
        }

        public static DateTime AttributeDateTime(this XPathNavigator nav, string attributeName, DateTime defaultValue)
        {
            string val = nav.GetAttribute(attributeName, string.Empty);
            if (string.IsNullOrEmpty(val)) { return defaultValue; }

            return Convert.ToDateTime(val);
        }

        public static string ID(this XmlNode node)
        {
            return node.Attribute("ID");
        }

        public static string Name(this XmlNode node)
        {
            return node.Attribute("Name");
        }

        public static string Attribute(this XmlNode node, string attributeName)
        {
            return Attribute(node, attributeName, null);
        }

        public static string Attribute(this XmlNode node, string attributeName, string defaultValue)
        {
            XmlAttribute attr = node.Attributes[attributeName];
            return (attr == null || string.IsNullOrEmpty(attr.Value)) ? defaultValue : attr.Value;
        }

        public static bool AttributeBoolean(this XmlNode node, string attributeName)
        {
            return AttributeBoolean(node, attributeName, false);
        }

        public static bool AttributeBoolean(this XmlNode node, string attributeName, bool defaultValue)
        {
            string val = node.Attribute(attributeName, string.Empty);
            if (string.IsNullOrEmpty(val)) { return defaultValue; }

            return Convert.ToBoolean(val);
        }

        public static DateTime AttributeDateTime(this XmlNode node, string attributeName)
        {
            return AttributeDateTime(node, attributeName, DateTime.Now);
        }

        public static DateTime AttributeDateTime(this XmlNode node, string attributeName, DateTime defaultValue)
        {
            string val = node.Attribute(attributeName, string.Empty);
            if (string.IsNullOrEmpty(val)) { return defaultValue; }

            return Convert.ToDateTime(val);
        }
    }

    /// <summary>
    /// DateTime extension methods.
    /// </summary>
    public static class DateTimeExtensions
    {
        public static string ToUIString(this DateTime dt)
        {
            return ToUIString(dt, true);
        }

        public static string ToUIString(this DateTime dt, bool includeTime)
        {
            if (includeTime)
            {
                return ToUIString(dt, "Today {0:h:mm tt}", "Yesterday {0:h:mm tt}", "{0:dddd h:mm tt}", "This {0:dddd h:mm tt}", "{0:dddd M/d/yyyy h:mm tt}");
            }
            else
            {
                return ToUIString(dt, "Today", "Yesterday", "{0:dddd}", "This {0:dddd}", "{0:M/d/yyyy}");
            }
        }

        public static string ToUIString(this DateTime dt, string todayMask, string yesterdayMask, string thisWeekMask, string nextWeekMask, string defaultMask)
        {
            DateTime now = DateTime.Now;
            DateTime today = now.Date;
            DateTime tomorrow = today.AddDays(1);
            DateTime yesterday = today.AddDays(-1);
            DateTime thisWeek = today.AddDays(-6);
            DateTime nextWeek = today.AddDays(6);

            string mask = defaultMask;

            if (today <= dt && dt < tomorrow)
            {
                mask = todayMask;
            }
            else if (yesterday <= dt && dt < today)
            {
                mask = yesterdayMask;
            }
            else if (today <= dt && dt < nextWeek)
            {
                mask = nextWeekMask;
            }
            else if (thisWeek <= dt && dt < today)
            {
                mask = thisWeekMask;
            }

            return string.Format(mask, dt);
        }
    }

    /// <summary>
    /// Exception extension methods.
    /// </summary>
    public static class ExceptionExtensions
    {
        public static void Print(this Exception ex)
        {
            string output = Format(ex);

            //if (HttpContext.Current != null) {
            //    HttpContext.Current.Response.Write(Format(ex, true));
            //    HttpContext.Current.Trace.Warn(output);
            //}

            Console.WriteLine(output);
            Console.Error.WriteLine(output);
            Debug.WriteLine(output);
        }

        public static string Format(StackTrace trace)
        {
            if (trace == null) { return null; }

            string resourceString = "at";
            string format = "in {0}:line {1}";

            bool flag = true;
            StringBuilder builder = new StringBuilder(0xff);

            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);
                MethodBase method = frame.GetMethod();
                if (method != null)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(Environment.NewLine);
                    }

                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0} ", resourceString);
                    Type declaringType = method.DeclaringType;
                    if (declaringType != null)
                    {
                        builder.Append(declaringType.FullName.Replace('+', '.'));
                        builder.Append(".");
                    }

                    builder.Append(method.Name);
                    if ((method is MethodInfo) && ((MethodInfo)method).IsGenericMethod)
                    {
                        Type[] genericArguments = ((MethodInfo)method).GetGenericArguments();
                        builder.Append("[");
                        int index = 0;
                        bool flag2 = true;
                        while (index < genericArguments.Length)
                        {
                            if (!flag2)
                            {
                                builder.Append(",");
                            }
                            else
                            {
                                flag2 = false;
                            }

                            builder.Append(genericArguments[index].Name);
                            index++;
                        }

                        builder.Append("]");
                    }

                    builder.Append("(");
                    ParameterInfo[] parameters = method.GetParameters();
                    bool flag3 = true;
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        if (!flag3)
                        {
                            builder.Append(", ");
                        }
                        else
                        {
                            flag3 = false;
                        }

                        string name = "<UnknownType>";
                        if (parameters[j].ParameterType != null)
                        {
                            name = parameters[j].ParameterType.Name;
                        }

                        builder.Append(name + " " + parameters[j].Name);
                    }

                    builder.Append(")");
                    if (frame.GetILOffset() != -1)
                    {
                        string fileName = null;
                        try
                        {
                            fileName = frame.GetFileName();
                        }
                        catch (SecurityException)
                        {
                        }

                        if (fileName != null)
                        {
                            builder.Append(' ');
                            builder.AppendFormat(CultureInfo.InvariantCulture, format, new object[] { fileName, frame.GetFileLineNumber() });
                        }
                    }
                }
            }

            builder.Append(Environment.NewLine);

            return builder.ToString();
        }

        public static string Format(this Exception ex, bool html)
        {
            string output = Format(ex);

            //using (StringWriter sw = new StringWriter()) {
            //    TextExceptionFormatter formatter = new TextExceptionFormatter(sw, ex);
            //    formatter.Format();
            //    sw.Flush();
            //    output = sw.ToString();
            //}

            string mask = html ? "<pre>{0}</pre>" : "{0}";
            return string.Format(mask, output);
        }

        public static string Format(this Exception ex)
        {
            return Format(ex, null);
        }

        public static string Format(this Exception ex, NameValueCollection additionalInfo)
        {
            if (additionalInfo == null) { additionalInfo = GetAdditionalInfo(ex, null); }

            string header = "** Exception Occurred *********************";
            StringBuilder strInfo = new StringBuilder(string.Format("{1}{0}", Environment.NewLine, header));

            if (ex == null)
            {
                strInfo.AppendFormat("{0}{0}No Exception object has been provided.{0}", Environment.NewLine);
            }
            else
            {
                Exception currentException = ex;  // Temp variable to hold InnerException object during the loop.
                int intExceptionCount = 1;        // Count variable to track the number of exceptions in the chain.
                do
                {
                    // Write title information for the exception object.
                    //strInfo.AppendFormat("{0}{0}{1}) Exception Information{0}{2}{0}", Environment.NewLine, intExceptionCount.ToString(), TEXT_SEPARATOR);
                    strInfo.AppendFormat(" {3,2}) {2,15}: {1}{0}", Environment.NewLine, currentException.GetType().FullName, "Exception Type", intExceptionCount.ToString());

                    // Loop through the public properties of the exception object and record their value.
                    PropertyInfo[] aryPublicProperties = currentException.GetType().GetProperties();
                    NameValueCollection currentAdditionalInfo;
                    foreach (PropertyInfo p in aryPublicProperties)
                    {
                        // Do not log information for the InnerException or StackTrace. This information is 
                        // captured later in the process.
                        try
                        {
                            var val = p.GetValue(currentException, null);

                            if (p.Name != "InnerException" && p.Name != "StackTrace")
                            {
                                if (val == null)
                                {
                                    strInfo.AppendFormat("{1,20}: NULL{0}", Environment.NewLine, p.Name);
                                }
                                else if (val is XmlElement)
                                {
                                    var eval = (XmlElement)val;
                                    var sb = new StringBuilder();
                                    var xsettings = new XmlWriterSettings();
                                    xsettings.Indent = true;
                                    xsettings.OmitXmlDeclaration = true;

                                    using (var xwriter = XmlWriter.Create(sb, xsettings))
                                    {
                                        xwriter.WriteRaw(eval.OuterXml);
                                    }

                                    strInfo.AppendFormat("{1,20}: {0}{2}{0}", Environment.NewLine, p.Name, sb.ToString());
                                }
                                else
                                {
                                    // Loop through the collection of AdditionalInformation if the exception type is a BaseApplicationException.
                                    if (p.Name == "AdditionalInformation")
                                    {
                                        // Verify the collection is not null.
                                        if (p.GetValue(currentException, null) != null)
                                        {
                                            // Cast the collection into a local variable.
                                            currentAdditionalInfo = (NameValueCollection)p.GetValue(currentException, null);

                                            // Check if the collection contains values.
                                            if (currentAdditionalInfo.Count > 0)
                                            {
                                                strInfo.AppendFormat("Additional Information:{0}", Environment.NewLine);

                                                // Loop through the collection adding the information to the string builder.
                                                for (int i = 0; i < currentAdditionalInfo.Count; i++)
                                                {
                                                    strInfo.AppendFormat("{1,20}: {2}{0}", Environment.NewLine, currentAdditionalInfo.GetKey(i), currentAdditionalInfo[i]);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Otherwise just write the ToString() value of the property.
                                        strInfo.AppendFormat("{1,20}: {2}{0}", Environment.NewLine, p.Name, p.GetValue(currentException, null));
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Ignore exceptions from a single property
                        }
                    }

                    if (currentException.StackTrace != null)
                    {
                        strInfo.AppendFormat("{1,20}:{0}{2}{0}{0}", Environment.NewLine, "Trace", currentException.StackTrace);
                    }

                    // Reset the temp exception object and iterate the counter.
                    currentException = currentException.InnerException;
                    intExceptionCount++;
                } 
                while (currentException != null);

                if (ex is FaultException<OrganizationServiceFault>)
                {
                    var fex = (FaultException<OrganizationServiceFault>)ex;
                    if (fex.Detail != null && !string.IsNullOrEmpty(fex.Detail.TraceText))
                    {
                        strInfo.AppendFormat("\n*****************\nPlugin Trace:\n {0}", fex.Detail.TraceText);
                    }
                }

                // Record the contents of the AdditionalInfo collection.
                if (additionalInfo != null)
                {
                    // Record General information.
                    foreach (string i in additionalInfo)
                    {
                        strInfo.AppendFormat(" {1,-20}: {2}{0}", Environment.NewLine, i, additionalInfo.Get(i));
                    }
                }
            }

            return strInfo.ToString();

            ////string user = "";
            ////if (HttpContext.Current == null) user = string.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName);
            ////else user = HttpContext.Current.User.Identity.Name;

            ////string sep = "******************************************************************";
            ////string addi = string.Empty;
            ////string strace = string.Empty;

            ////if (additionalInfo != null)
            ////{
            ////    foreach (string s in additionalInfo)
            ////        addi = string.Format("{0}{4,12}{1,-40}: {2}{3}", addi, s, additionalInfo[s], Environment.NewLine, " ");
            ////    addi = string.Format("{1,10}:{2}{0}", addi, "Info", Environment.NewLine);
            ////}

            ////if (ex.StackTrace != null) strace = string.Format("{0,10}:{2}{1}{2}", "Trace", ex.StackTrace, Environment.NewLine);

            ////string ret = string.Format("{14}{6}{11,10}: {4}{6}{7,10}: {0}{6}{8,10}: {1}{6}{10,10}: {3}{6}{9,10}: {2}{6}{12}{13}{14}",
            ////    ex.GetType().ToString(), ex.Source, user, ex.TargetSite, ex.Message,
            ////    ex.StackTrace, Environment.NewLine, "Exception", "Source", "User",
            ////    "Target", "Message", addi, strace, sep);

            ////return ret;
        }

        private static NameValueCollection GetAdditionalInfo(this Exception ex, NameValueCollection additionalInfo)
        {
            // Create the Additional Information collection if it does not exist.
            if (additionalInfo == null) { additionalInfo = new NameValueCollection(); }

            //if (exception is SoapException) {
            //    additionalInfo.Add("SoapDetails", ((SoapException)exception).Detail.OuterXml);
            //}

            //HttpContext ctx = HttpContext.Current;
            //if (ctx != null) {
            //    additionalInfo.Add("Browser", string.Format("{0} {1}", ctx.Request.Browser.Browser, ctx.Request.Browser.Version));
            //    additionalInfo.Add("Url", ctx.Request.Url.ToString());
            //}

            additionalInfo.Add("MachineName", Environment.MachineName);
            additionalInfo.Add("TimeStamp", DateTime.Now.ToString());
            additionalInfo.Add("FullName", Assembly.GetExecutingAssembly().FullName);
            additionalInfo.Add("AppDomainName", AppDomain.CurrentDomain.FriendlyName);
            additionalInfo.Add("ThreadIdentity", Thread.CurrentPrincipal.Identity.Name);
            additionalInfo.Add("WindowsIdentity", WindowsIdentity.GetCurrent().Name);

            return additionalInfo;
        }

        //public static string Format(StackTrace trace, bool html, int startFrameIndex) {

        //    if (trace == null) return null;
        //    if (startFrameIndex < 0) throw new ArgumentException("startFrameIndex");

        //    StringBuilder sb = new StringBuilder();

        //    MethodBase method;
        //    string fmethod, filename;
        //    int linenumber, linecolumn;
        //    StackFrame f;

        //    for (int i = startFrameIndex; i < trace.FrameCount; i++ ) {
        //        f = trace.GetFrame(i);
        //        method = f.GetMethod();
        //        fmethod = method.ToString();
        //        filename = f.GetFileName();
        //        linenumber = f.GetFileLineNumber();
        //        linecolumn = f.GetFileColumnNumber();

        //        sb.AppendFormat("at {0} {1}{2}{3}\n"
        //            , method
        //            , string.IsNullOrEmpty(filename) ? "" : filename
        //            , linenumber == 0 ? "" : " Line: " + linenumber.ToString()
        //            , linecolumn == 0 ? "" : " Column : " + linecolumn.ToString());
        //    }

        //    string ret = sb.ToString();
        //    return html ? string.Format("<pre></pre>", ret) : ret;

        //}
    }

    /// <summary>
    /// CRM Entity extension methods.
    /// </summary>
    public static class EntityExtensions
    {
        public static Guid? Guid(this Entity record, string fieldName)
        {
            Guid? ret = null;

            if (record[fieldName] == null) { return null; }

            try
            {
                ret = new Guid(record.String(fieldName));
            }
            catch
            {
                return null;
            }

            return ret;
        }

        public static int? Int(this Entity record, string fieldName)
        {
            if (record[fieldName] == null) { return null; }

            int ret;

            if (int.TryParse(record.String(fieldName), out ret))
            {
                return ret;
            }

            return null;
        }

        public static bool? Boolean(this Entity record, string fieldName)
        {
            if (record[fieldName] == null) { return null; }
            return Convert.ToBoolean(record[fieldName]);
        }

        public static string String(this Entity record, string fieldName)
        {
            if (record[fieldName] == null) { return null; }
            return Convert.ToString(record[fieldName]);
        }

        public static DateTime? Date(this Entity record, string fieldName)
        {
            if (record[fieldName] == null) { return null; }
            return Convert.ToDateTime(record[fieldName]);
        }

        public static bool TryGuid(this Entity record, string fieldName, out Guid output)
        {
            output = default(Guid);
            try
            {
                output = new Guid(record.String(fieldName));
                return true;
            }
            catch
            {
            }

            return false;
        }

        public static bool TryInt(this Entity record, string fieldName, out int output)
        {
            if (int.TryParse(record.String(fieldName), out output))
            {
                return true;
            }

            return false;
        }

        public static bool TryBoolean(this Entity record, string fieldName, out bool output)
        {
            if (bool.TryParse(record.String(fieldName), out output))
            {
                return true;
            }

            return false;
        }

        public static bool TryDate(this Entity record, string fieldName, out DateTime output)
        {
            if (DateTime.TryParse(record.String(fieldName), out output))
            {
                return true;
            }

            return false;
        }
    }

    public static class DataTableExtensions
    {
        public static string ToCSV(this DataTable table)
        {
            var result = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
            }

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    result.Append(row[i].ToString());
                    result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
                }
            }

            return result.ToString();
        }
    }
}