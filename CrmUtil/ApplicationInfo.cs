using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CrmUtil
{
    public class ApplicationInfo
    {
        public ApplicationInfo()
        {
            Version = Assembly.GetCallingAssembly().GetName().Version;

            Title = "";
            object[] attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (titleAttribute.Title.Length > 0) Title = titleAttribute.Title;
            } else {
                Title = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }

            attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            ProductName = attributes.Length == 0 ? "" : ((AssemblyProductAttribute)attributes[0]).Product;

            attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            Description = attributes.Length == 0 ? "" : ((AssemblyDescriptionAttribute)attributes[0]).Description;

            attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            Copyright = attributes.Length == 0 ? "" : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            CompanyName = attributes.Length == 0 ? "" : ((AssemblyCompanyAttribute)attributes[0]).Company;
        }

        public Version Version { get; private set; }

        public string Title { get; private set; }

        public string ProductName { get; private set; }

        public string Description { get; private set; }

        public string Copyright { get; private set; }

        public string CompanyName { get; private set; }
    }
}
