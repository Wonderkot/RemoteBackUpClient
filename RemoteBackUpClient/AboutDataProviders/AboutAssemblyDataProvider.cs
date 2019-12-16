using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Xml;

namespace RemoteBackUpClient.AboutDataProviders
{
    public class AboutAssemblyDataProvider : IAboutDataProvider
    {
        #region Private members
        private XmlDocument _xmlDoc;

        #endregion

        #region Static data
        private const string DefaultAboutProviderKey = "AboutProvider";
        private const string PropertyNameTitle = "Title";
        private const string PropertyNameDescription = "Description";
        private const string PropertyNameProduct = "Product";
        private const string PropertyNameCopyright = "Copyright";
        private const string PropertyNameCompany = "Company";
        private const string XPathRoot = "ApplicationInfo/";
        private const string XPathTitle = XPathRoot + PropertyNameTitle;
        private const string XPathVersion = XPathRoot + "Version";
        private const string XPathDescription = XPathRoot + PropertyNameDescription;
        private const string XPathProduct = XPathRoot + PropertyNameProduct;
        private const string XPathCopyright = XPathRoot + PropertyNameCopyright;
        private const string XPathCompany = XPathRoot + PropertyNameCompany;
        private const string XPathLink = XPathRoot + "Link";
        private const string XPathLinkUri = XPathRoot + "Link/@Uri";
        #endregion

        #region Public properties
        /// <summary>
        /// Gets and sets the resource key for the XmlDataProvider to retrieve
        /// from Application resources.
        /// </summary>
        public string ResourceKey { get; set; } = DefaultAboutProviderKey;

        #endregion

        #region IAboutDataProvider Members
        /// <summary>
        /// Gets the title property, which is display in the About dialogs window title.
        /// </summary>
        public string Title
        {
            get
            {
                string result = CalculatePropertyValue<AssemblyTitleAttribute>("Title", XPathTitle);

                if (string.IsNullOrEmpty(result))
                {
                    // otherwise, just get the name of the assembly itself.
                    result = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the application's version information to show.
        /// </summary>
        public string Version
        {
            get
            {
                string result = "";
                // first, try to get the version string from the assembly.
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version != null)
                {
                    result = version.ToString();
                }
                else
                {
                    // if that fails, try to get the version from a resource in the Application.
                    result = GetLogicalResourceString(XPathVersion);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the description about the application.
        /// </summary>
        public string Description => CalculatePropertyValue<AssemblyDescriptionAttribute>("Description", XPathDescription);

        /// <summary>
        ///  Gets the product's full name.
        /// </summary>
        public string Product => CalculatePropertyValue<AssemblyProductAttribute>("Product", XPathProduct);

        /// <summary>
        /// Gets the copyright information for the product.
        /// </summary>
        public string Copyright => CalculatePropertyValue<AssemblyCopyrightAttribute>("Copyright", XPathCopyright);

        /// <summary>
        /// Gets the product's company name.
        /// </summary>
        public string Company => CalculatePropertyValue<AssemblyCompanyAttribute>("Company", XPathCompany);

        /// <summary>
        /// Gets the link text to display in the About dialog.
        /// </summary>
        public string LinkText => GetLogicalResourceString(XPathLink);

        /// <summary>
        /// Gets the link uri that is the navigation target of the link.
        /// </summary>
        public string LinkUri => GetLogicalResourceString(XPathLinkUri);

        #endregion

        #region Helper method
        /// <summary>
        /// Gets the specified property value either from a specific attribute,
        /// or from a resource in the Application.
        /// </summary>
        /// <typeparam name="T">Attribute type that we're trying to retrieve.</typeparam>
        /// <param name="propertyName">Property name to use on the attribute.</param>
        /// <param name="xpathQuery">XPath to the element in the XML data resource.</param>
        /// <returns>The resulting string to use for a property.
        /// Returns null if no data could be retrieved.</returns>
        private string CalculatePropertyValue<T>(string propertyName, string xpathQuery)
        {
            string result = "";
            // first, try to get the property value from an attribute.
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false);
            if (attributes.Length > 0)
            {
                T attrib = (T)attributes[0];
                PropertyInfo property = attrib.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    result = property.GetValue(attributes[0], null) as string;
                }
            }
            else
            {
                // if that fails, try to get it from a resource in the Application.
                result = GetLogicalResourceString(xpathQuery);
            }

            return result;
        }
        #endregion

        #region Resource location methods
        /// <summary>
        /// Gets the XmlDataProvider's document from an Application resource.
        /// </summary>
        protected virtual XmlDocument ResourceXmlDocument
        {
            get
            {
                if (_xmlDoc == null)
                {
                    // if we haven't already found the resource XmlDocument, then
                    // try to find it.
                    Debug.Assert(App.Current != null, "The current application should no tbe null at this point.");
                    XmlDataProvider provider = App.Current.TryFindResource(ResourceKey) as XmlDataProvider;
                    if (provider != null)
                    {
                        // save away the XmlDocument, so we don't have to get it multiple times.
                        _xmlDoc = provider.Document;
                    }
                }

                return _xmlDoc;
            }
        }

        /// <summary>
        /// Gets the specified data element from the XmlDataProvider in the
        /// Application resources.
        /// </summary>
        /// <param name="xpathQuery">An XPath query to the XML element to retrieve.</param>
        /// <returns>The resulting string value for the specified XML element. 
        /// Returns empty string if resource element couldn't be found.</returns>
        protected virtual string GetLogicalResourceString(string xpathQuery)
        {
            string result = "";

            // get the About xml information from the application's resources.
            XmlDocument doc = this.ResourceXmlDocument;
            if (doc != null)
            {
                // if we found the XmlDocument, then look for the specified data. 
                XmlNode node = doc.SelectSingleNode(xpathQuery);
                if (node != null)
                {
                    if (node is XmlAttribute)
                    {
                        // only an XmlAttribute has a Value set.
                        result = node.Value;
                    }
                    else
                    {
                        // otherwise, need to just return the inner text.
                        result = node.InnerText;
                    }
                }
            }

            return result;
        }
        #endregion
    }
}