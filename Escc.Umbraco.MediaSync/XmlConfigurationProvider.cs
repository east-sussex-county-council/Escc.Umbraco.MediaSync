using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Umbraco.Core.Models;
using umbraco.presentation.channels.businesslogic;

namespace Escc.Umbraco.MediaSync
{
    /// <summary>
    /// Reads configuration settings from an XML file
    /// </summary>
    public class XmlConfigurationProvider : IMediaSyncConfigurationProvider
    {
        /// <summary>
        /// Reads a setting in the <c>general</c> section of the configuration file.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string ReadSetting(string key)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(uMediaSyncHelper.configFile);

            string setting = String.Empty;

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "general")
                {
                    foreach (XmlNode subNode in node.ChildNodes)
                    {
                        if (subNode.Name == key)
                        {
                            setting = subNode.InnerText;
                        }
                    }
                }
            }
            return setting;
        }

        /// <summary>
        /// Reads a boolean setting in the <c>general</c> section of the configuration file.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ReadBooleanSetting(string key)
        {
            try
            {
                var configurationValue = ReadSetting(key);
                if (String.IsNullOrWhiteSpace(configurationValue)) return false;
                return Boolean.Parse(configurationValue);
            }
            catch (FormatException)
            {
                return false;
            }

        }

        /// <summary>
        /// Reads an integer setting in the <c>general</c> section of the configuration file.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public int? ReadIntegerSetting(string key)
        {
            try
            {
                var configurationValue = ReadSetting(key);
                if (String.IsNullOrWhiteSpace(configurationValue)) return null;
                return Int32.Parse(configurationValue);
            }
            catch (FormatException)
            {
                return null;
            }

        }

        /// <summary>
        /// Reads property editor aliases compatible with the given media id provider.
        /// </summary>
        /// <param name="mediaIdProvider">The name of the mediaIdProvider element in the configuration file.</param>
        /// <returns></returns>
        public IEnumerable<string> ReadPropertyEditorAliases(string mediaIdProvider)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(uMediaSyncHelper.configFile);

            var propertyEditorAliases = new List<string>();

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "mediaIdProviders")
                {
                    foreach (XmlNode subNode in node.ChildNodes)
                    {
                        if (subNode.Name == mediaIdProvider)
                        {
                            foreach (XmlNode propertyEditorNode in subNode.ChildNodes)
                            {
                                propertyEditorAliases.Add(propertyEditorNode.InnerText.ToUpperInvariant());
                            }
                        }
                    }
                }
            }
            return propertyEditorAliases;
        }

        /// <summary>
        /// Checks whether the given content node should have a related media folder
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public bool SyncNode(IContent content)
        {
            bool sync = false;

            if (Convert.ToBoolean(ReadSetting("syncAllContent")) == true)
            {
                sync = true;
            }
            else if (!IsSyncHide(content))
            {
                string blackListResult = ReadBlacklist("docTypes").FirstOrDefault(x => x == content.ContentType.Alias.ToString());

                string level = ReadBlacklist("level").FirstOrDefault();

                if (content.Level < Convert.ToInt32(level))
                {
                    sync = !String.IsNullOrEmpty(blackListResult) ? false : true;
                }
            }
            return sync;
        }

        private bool IsSyncHide(IContent node)
        {
            List<string> blackListResult = ReadBlacklist("docTypes");

            bool syncHide = false;
            while (node.Level > 1 && syncHide == false)
            {
                if (node.HasProperty("uMediaSyncHide") && node.GetValue<bool>("uMediaSyncHide") != null && node.GetValue<bool>("uMediaSyncHide") == true)
                {
                    syncHide = true;
                }
                if (blackListResult.Where(x => x == node.ContentType.Alias.ToString()).Count() != 0)
                {
                    syncHide = true;
                }

                node = node.Parent();
            }

            return syncHide;
        }

        private List<string> ReadBlacklist(string key)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(uMediaSyncHelper.configFile);

            List<string> settings = new List<string>();

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "blackList")
                {
                    foreach (XmlNode subNode in node.ChildNodes)
                    {
                        if (key == "docTypes" && subNode.Name == "blackListDocTypes")
                        {
                            foreach (XmlNode typeNode in subNode.ChildNodes)
                            {
                                if (typeNode.Name == "docTypeAlias")
                                {
                                    settings.Add(typeNode.InnerText);
                                }
                            }
                        }
                        else if (key == "level" && subNode.Name == "blackListLevel")
                        {
                            settings.Add(subNode.InnerText);
                        }
                    }
                }
            }
            return settings;
        }
    }
}
