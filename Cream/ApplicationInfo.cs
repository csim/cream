using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cream
{
    public class ApplicationInfo
    {
        public ApplicationInfo()
        {
            var assm = Assembly.GetCallingAssembly();
            Version = assm.GetName().Version;

            Title = "";
            var attrib = assm.GetCustomAttributes(typeof(AssemblyTitleAttribute), false).FirstOrDefault();
            if (attrib == null)
            {
                Title = ((AssemblyTitleAttribute)attrib).Title;
            } else {
                Title = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }

            attrib = assm.GetCustomAttributes(typeof(AssemblyProductAttribute), false).FirstOrDefault();
            if (attrib != null)
            {
                ProductName = ((AssemblyProductAttribute)attrib).Product;
            }

            attrib = assm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).FirstOrDefault();
            if (attrib != null)
            {
                Description = ((AssemblyDescriptionAttribute)attrib).Description;
            }

            attrib = assm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).FirstOrDefault();
            if (attrib != null)
            {
                Copyright = ((AssemblyCopyrightAttribute)attrib).Copyright;
            }

            attrib = assm.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).FirstOrDefault();
            if (attrib != null)
            {
                CompanyName = ((AssemblyCompanyAttribute)attrib).Company;
            }
        }

        public Version Version { get; private set; }

        public string Title { get; private set; }

        public string ProductName { get; private set; }

        public string Description { get; private set; }

        public string Copyright { get; private set; }

        public string CompanyName { get; private set; }
    }
}
