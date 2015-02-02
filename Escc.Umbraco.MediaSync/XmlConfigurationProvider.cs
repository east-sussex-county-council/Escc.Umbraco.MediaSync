using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Umbraco.Core.Models;

namespace Escc.Umbraco.MediaSync
{
    public class XmlConfigurationProvider : IMediaSyncConfigurationProvider
    {
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

        public bool ReadBooleanSetting(string key)
        {
            try
            {
                return Boolean.Parse(ReadSetting(key));
            }
            catch (FormatException)
            {
                return false;
            }

        }

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

        public bool IsSyncHide(IContent node)
        {
            IContent nodeCopy = node;
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

        public List<string> ReadBlacklist(string key)
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
