using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using umbraco.BasePages;
using User = umbraco.BusinessLogic.User;

namespace umbraco
{

    //TODO: Make the User overloads obsolete, then publicize the IUser object

    //TODO: Convert all of this over to Niels K's localization framework and put into Core proj.

    /// <summary>
    /// The ui class handles the multilingual text in the umbraco back-end.
    /// Provides access to language settings and language files used in the umbraco back-end.
    /// </summary>
    public class ui
    {
        private static readonly string UmbracoDefaultUiLanguage = GlobalSettings.DefaultUILanguage;
        private static readonly string UmbracoPath = SystemDirectories.Umbraco;

        /// <summary>
        /// Gets the current Culture for the logged-in users
        /// </summary>
        /// <param name="u">The user.</param>
        /// <returns></returns>
        public static string Culture(User u)
        {
            return Culture(u.Language);
        }
        
        internal static string Culture(IUser u)
        {
            return Culture(u.Language);
        }

        internal static string Culture(string userLanguage)
        {
            var langFile = getLanguageFile(userLanguage);
            try
            {
                return langFile.SelectSingleNode("/language").Attributes.GetNamedItem("culture").Value;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if th user is logged in, if they are, return their language specified in the database.
        /// If they aren't logged in, check the current thread culture and return it, however if that is
        /// null, then return the default Umbraco culture.
        /// </summary>
        /// <returns></returns>
        private static string GetLanguage()
        {
            var user = UmbracoEnsuredPage.CurrentUser;
            return GetLanguage(user);
        }

        private static string GetLanguage(User u)
        {
            if (u != null)
            {
                return u.Language;
            }
            return GetLanguage("");
        }

        private static string GetLanguage(IUser u)
        {
            if (u != null)
            {
                return u.Language;
            }
            return GetLanguage("");
        }

        private static string GetLanguage(string userLanguage)
        {
            if (userLanguage.IsNullOrWhiteSpace() == false)
            {
                return userLanguage;
            }
            var language = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            if (string.IsNullOrEmpty(language))
                language = UmbracoDefaultUiLanguage;
            return language;
        }

        /// <summary>
        /// Returns translated UI text with a specific key based on the specified user's language settings
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <param name="u">The user.</param>
        /// <returns></returns>
        public static string Text(string Key, User u)
        {
            return GetText(string.Empty, Key, null, GetLanguage(u));
        }

        internal static string Text(string key, IUser u)
        {
            return GetText(string.Empty, key, null, GetLanguage(u));
        }

        /// <summary>
        /// Returns translated UI text with a specific key based on the logged-in user's language settings
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string Text(string Key)
        {
            return GetText(Key);
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area, based on the specified users language settings
        /// </summary>
        /// <param name="Area">The area.</param>
        /// <param name="Key">The key.</param>
        /// <param name="u">The user.</param>
        /// <returns></returns>
        public static string Text(string Area, string Key, User u)
        {
            return GetText(Area, Key, null, GetLanguage(u));
        }

        public static string Text(string area, string key, IUser u)
        {
            return GetText(area, key, null, GetLanguage(u));
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area, based on the logged-in users language settings
        /// </summary>
        /// <param name="Area">The area.</param>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string Text(string Area, string Key)
        {
            return GetText(Area, Key, GetLanguage());
        }

        /// <summary>
        /// Returns translated UI text with a specific area and key. based on the specified users language settings and variables array passed to the method
        /// </summary>
        /// <param name="Area">The area.</param>
        /// <param name="Key">The key.</param>
        /// <param name="Variables">The variables array.</param>
        /// <param name="u">The user.</param>
        /// <returns></returns>
        public static string Text(string Area, string Key, string[] Variables, User u)
        {
            return GetText(Area, Key, Variables, GetLanguage(u));
        }

        internal static string Text(string area, string key, string[] variables)
        {
            return GetText(area, key, variables, GetLanguage((IUser)null));
        }

        internal static string Text(string area, string key, string[] variables, IUser u)
        {
            return GetText(area, key, variables, GetLanguage(u));
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area based on the specified users language settings and single variable passed to the method
        /// </summary>
        /// <param name="Area">The area.</param>
        /// <param name="Key">The key.</param>
        /// <param name="Variable">The variable.</param>
        /// <param name="u">The u.</param>
        /// <returns></returns>
        public static string Text(string Area, string Key, string Variable, User u)
        {
            return GetText(Area, Key, new[] { Variable }, GetLanguage(u));
        }

        internal static string Text(string area, string key, string variable)
        {
            return GetText(area, key, new[] { variable }, GetLanguage((IUser)null));
        }

        internal static string Text(string area, string key, string variable, IUser u)
        {
            return GetText(area, key, new[] { variable }, GetLanguage(u));
        }

        /// <summary>
        /// Returns translated UI text with a specific key based on the logged-in user's language settings
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetText(string key)
        {
            return GetText(string.Empty, key, null, GetLanguage());
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area based on the logged-in users language settings
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetText(string area, string key)
        {
            return GetText(area, key, null, GetLanguage());
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area based on the logged-in users language settings and variables array send to the method.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="key">The key.</param>
        /// <param name="variables">The variables.</param>
        /// <returns></returns>
        public static string GetText(string area, string key, string[] variables)
        {
            return GetText(area, key, variables, GetLanguage());
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area matching the variable send to the method.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="key">The key.</param>
        /// <param name="variable">The variable.</param>
        /// <returns></returns>
        public static string GetText(string area, string key, string variable)
        {
            return GetText(area, key, new[] { variable }, GetLanguage());
        }

        /// <summary>
        /// Returns translated UI text with a specific key, area and language matching the variables send to the method.
        /// </summary>
        /// <param name="area">The area (Optional)</param>
        /// <param name="key">The key (Required)</param>
        /// <param name="variables">The variables (Optional)</param>
        /// <param name="language">The language (Optional)</param>
        /// <returns></returns>
        /// <remarks>This is the underlying call for all Text/GetText method calls</remarks>
        public static string GetText(string area, string key, string[] variables, string language)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            if (string.IsNullOrEmpty(language))
                language = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

            var langFile = getLanguageFile(language);

            if (langFile != null)
            {
                XmlNode node;
                if (string.IsNullOrEmpty(area))
                {
                    node = langFile.SelectSingleNode(string.Format("//key [@alias = '{0}']", key));
                }
                else
                {
                    node = langFile.SelectSingleNode(string.Format("//area [@alias = '{0}']/key [@alias = '{1}']", area, key));
                }

                if (node != null)
                {
                    if (variables != null && variables.Length > 0)
                    {
                        return GetStringWithVars(node, variables);
                    }
                    return xmlHelper.GetNodeValue(node);
                }
            }
            return "[" + key + "]";
        }

        private static string GetStringWithVars(XmlNode node, string[] variables)
        {
            var stringWithVars = xmlHelper.GetNodeValue(node);
            var vars = Regex.Matches(stringWithVars, @"\%(\d)\%",
                                     RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            foreach (Match var in vars)
            {
                stringWithVars = stringWithVars.Replace(
                    var.Value,
                    variables[Convert.ToInt32(var.Groups[0].Value.Replace("%", ""))]);
            }
            return stringWithVars;
        }



        /// <summary>
        /// Gets the language file as a xml document.
        /// Merges with language files found in AppPlugins. If key already exists in language xml it will not get overwritten. 
        /// Use tags which have a 'namespace' eg ajmButtonName
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public static XmlDocument getLanguageFile(string language)
        {
            lock (ApplicationContext.Current.ApplicationCache.RuntimeCache)
            {
                var cacheKey = "uitext_" + language;
                XmlDocument languageDoc = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                    cacheKey
                    ) as XmlDocument;

                if (languageDoc != null)
                {
                    return languageDoc;
                }

                // get core lang files
                List<string> files = new List<string>() { IOHelper.MapPath(UmbracoPath + "/config/lang/" + language + ".xml") };

                // scan for plugin lang files
                files.AddRange(
                    Directory.GetFiles(IOHelper.MapPath(SystemDirectories.AppPlugins), language + ".xml", SearchOption.AllDirectories).
                    Where(f => Directory.GetParent(f).Name.Equals("lang", StringComparison.InvariantCultureIgnoreCase))
                    .ToList<string>()
                );

                foreach (string plugin in files)
                {
                    MergeXmlLanguageFile(ref languageDoc, plugin);
                }

                ApplicationContext.Current.ApplicationCache.RuntimeCache.InsertCacheItem(
                    cacheKey, ()=>{return languageDoc;}, dependentFiles: files.ToArray());

                return languageDoc;
            }
        }

        private static void MergeXmlLanguageFile(ref XmlDocument languageDoc, string file)
        {
            LogHelper.Debug<ui>("Loading language file " + file);
            try
            {
                if (languageDoc == null)
                {
                    languageDoc = new XmlDocument();
                    languageDoc.Load(file);
                    return;
                }
                else
                {
                    XmlDocument pluginXml = new XmlDocument();
                    pluginXml.Load(file);

                    foreach (XmlNode key in pluginXml.SelectNodes("language/area/key"))
                    {

                        string pluginAreaAlias = key.ParentNode.Attributes["alias"].Value;
                        string pluginKeyAlias = key.Attributes["alias"].Value;

                        XmlNode existingNode = languageDoc.SelectSingleNode(string.Format("language/area[@alias='{0}']/key[@alias='{1}']", pluginAreaAlias, pluginKeyAlias));

                        if (existingNode != null)
                        {
                            // do we update or override?
                            LogHelper.Warn<ui>("Key already exists for {0}_{1}. Won't override, use key names with namespaces.", () => new object[] { pluginAreaAlias, pluginKeyAlias });
                        }
                        else
                        {
                            string areaAliasValue = key.ParentNode.Attributes["alias"].Value;
                            XmlNode areaElement = languageDoc.SelectSingleNode(string.Format("language/area[@alias='{0}']", areaAliasValue)) as XmlElement;
                            if (areaElement == null)
                            {
                                areaElement = languageDoc.CreateElement("area");
                                XmlAttribute areaAlias = languageDoc.CreateAttribute("alias");
                                areaAlias.Value = areaAliasValue;
                                languageDoc.SelectSingleNode("/").AppendChild(areaElement);
                            }

                            XmlElement keyElemement = languageDoc.CreateElement("key");
                            XmlAttribute alias = languageDoc.CreateAttribute("alias");
                            alias.Value = key.Attributes["alias"].Value;

                            keyElemement.Attributes.Append(alias);
                            keyElemement.InnerXml = key.InnerXml;

                            areaElement.AppendChild(keyElemement);
                        }
                    }
                }
            }
            catch (Exception x)
            {
                LogHelper.Error<ui>( "Error loading language file: " + file + x.Message ,x);
            }
        }
    }
}
