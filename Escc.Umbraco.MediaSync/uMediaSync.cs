using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;

namespace Escc.Umbraco.MediaSync
{
    public class uMediaSync : ApplicationEventHandler
    {
        private void EnsureRelationTypeExists()
        {
            if (uMediaSyncHelper.relationService.GetRelationTypeByAlias("uMediaSyncRelation") == null)
            {
                var relationType = new RelationType(new Guid("b796f64c-1f99-4ffb-b886-4bf4bc011a9c"), new Guid("c66ba18e-eaf3-4cff-8a22-41b16d66a972"), "uMediaSyncRelation", "uMediaSyncRelation");
                uMediaSyncHelper.relationService.Save(relationType);
            }
        }

        public uMediaSync()
        {
            ContentService.Saving += ContentService_Saving;
            ContentService.Saved += ContentService_Saved;
            ContentService.Moving += ContentService_Moving;
            ContentService.Moved += ContentService_Moved;
            ContentService.Copied += ContentService_Copied;
            ContentService.Trashed += ContentService_Trashed;
            ContentService.Deleting += ContentService_Deleting;
        }

        void ContentService_Saving(IContentService sender, global::Umbraco.Core.Events.SaveEventArgs<IContent> e)
        {
            // Rename Media-NodeName

            if (ReadSetting("renameMedia").ToString() == "true")
            {
               EnsureRelationTypeExists();
                
                foreach (var node in e.SavedEntities)
                {
                    if (node.HasIdentity == true)
                    {
                        if (uMediaSyncHelper.contentService.GetPublishedVersion(node.Id) != null)
                        {
                            IContent oldContent = uMediaSyncHelper.contentService.GetPublishedVersion(node.Id);

                            if (oldContent.Name != node.Name)
                            {
                                IEnumerable<IRelation> uMediaSyncRelations = uMediaSyncHelper.relationService.GetByRelationTypeAlias("uMediaSyncRelation");
                                IRelation uMediaSyncRelation = uMediaSyncRelations.Where(r => r.ParentId == node.Id).FirstOrDefault();

                                if (uMediaSyncRelation != null)
                                {
                                    int mediaId = uMediaSyncRelation.ChildId;

                                    IMedia media = uMediaSyncHelper.mediaService.GetById(mediaId);
                                    media.Name = node.Name;
                                    uMediaSyncHelper.mediaService.Save(media, uMediaSyncHelper.userId);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ContentService_Saved(IContentService sender, global::Umbraco.Core.Events.SaveEventArgs<IContent> e)
        {
            foreach (var node in e.SavedEntities)
            {
                var dirty = (IRememberBeingDirty)node;
                var isNew = dirty.WasPropertyDirty("Id");

                if (isNew)
                {
                    // Create Media-Node and set Relation
                    int contentRoot = String.IsNullOrWhiteSpace(ReadSetting("syncFromContentRootNode").ToString()) ? -1 : Convert.ToInt32(ReadSetting("syncFromContentRootNode").ToString());
                    int mediaParent = String.IsNullOrWhiteSpace(ReadSetting("syncToMediaRootNode").ToString()) ? -1 : Convert.ToInt32(ReadSetting("syncToMediaRootNode").ToString());

                    IContent contentRootNode = uMediaSyncHelper.contentService.GetById(contentRoot);

                    if (contentRoot == -1 || node.Path.Contains(contentRootNode.Path))
                    {
                        if (syncNode(node) == true)
                        {

                            if (node.ParentId != contentRoot)
                            {
                                IContent contentParent = uMediaSyncHelper.contentService.GetById(node.ParentId);
                                IEnumerable<IRelation> uMediaSyncRelations = uMediaSyncHelper.relationService.GetByRelationTypeAlias("uMediaSyncRelation");
                                IRelation uMediaSyncRelation = uMediaSyncRelations.Where(r => r.ParentId == node.ParentId).FirstOrDefault();
                                mediaParent = uMediaSyncRelation.ChildId;
                            }

                            IMedia media = uMediaSyncHelper.mediaService.CreateMedia(node.Name, mediaParent, "Folder", uMediaSyncHelper.userId);
                            uMediaSyncHelper.mediaService.Save(media);
                            IRelation relation = uMediaSyncHelper.relationService.Relate(node, media, "uMediaSyncRelation");
                            uMediaSyncHelper.relationService.Save(relation);
                        }
                    }
                }
            }
        }

        void ContentService_Moving(IContentService sender, global::Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            if (syncNode(e.Entity) == false)
            {
                HttpCookie uMediaSyncCookie = new HttpCookie("uMediaSyncNotMove_" + e.ParentId);
                uMediaSyncCookie.Value = e.ParentId.ToString();
                uMediaSyncCookie.Expires = DateTime.Now.AddSeconds(30);
                HttpContext.Current.Response.Cookies.Add(uMediaSyncCookie);
            }
        }

        void ContentService_Moved(IContentService sender, global::Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            if (HttpContext.Current.Request.Cookies["uMediaSyncNotMove_" + e.Entity.ParentId] == null)
            {
                IContent contentParent = uMediaSyncHelper.contentService.GetById(e.Entity.ParentId);

                IEnumerable<IRelation> uMediaSyncRelations = uMediaSyncHelper.relationService.GetByRelationTypeAlias("uMediaSyncRelation");
                IRelation uMediaSyncRelation = uMediaSyncRelations.Where(r => r.ParentId == e.Entity.Id).FirstOrDefault();
                if (uMediaSyncRelation != null)
                {
                    int mediaId = uMediaSyncRelation.ChildId;

                    IRelation uMediaSyncRelationNew = uMediaSyncRelations.Where(r => r.ParentId == e.Entity.ParentId).FirstOrDefault();
                    if (uMediaSyncRelationNew != null)
                    {
                        int mediaParentNewId = uMediaSyncRelationNew.ChildId;

                        IMedia media = uMediaSyncHelper.mediaService.GetById(mediaId);
                        IMedia mediaParentNew = uMediaSyncHelper.mediaService.GetById(mediaParentNewId);

                        uMediaSyncHelper.mediaService.Move(media, mediaParentNew.Id, uMediaSyncHelper.userId);
                    }
                }
            }
            else
            {
                HttpCookie uMediaSyncCookie = HttpContext.Current.Request.Cookies["uMediaSyncNotMove_" + e.Entity.ParentId];
                uMediaSyncCookie.Expires = DateTime.Now.AddHours(-1);
                HttpContext.Current.Response.Cookies.Add(uMediaSyncCookie);
            }
        }

        void ContentService_Copied(IContentService sender, global::Umbraco.Core.Events.CopyEventArgs<IContent> e)
        {
            if (syncNode(e.Original))
            {
                IContent content1 = uMediaSyncHelper.contentService.GetById(e.Original.Id);
                IContent content2 = uMediaSyncHelper.contentService.GetById(e.Copy.Id);

                IContent content2Parent = uMediaSyncHelper.contentService.GetById(e.Copy.ParentId);

                IEnumerable<IRelation> uMediaSyncRelations = uMediaSyncHelper.relationService.GetByRelationTypeAlias("uMediaSyncRelation");
                IRelation uMediaSyncRelation1 = uMediaSyncRelations.Where(r => r.ParentId == content1.Id).FirstOrDefault();
                if (uMediaSyncRelation1 != null)
                {
                    int media1Id = uMediaSyncRelation1.ChildId;

                    IMedia media1 = uMediaSyncHelper.mediaService.GetById(media1Id);


                    IRelation uMediaSyncRelation2Parent = uMediaSyncRelations.Where(r => r.ParentId == content2.ParentId).FirstOrDefault();
                    if (uMediaSyncRelation2Parent != null)
                    {
                        int media2ParentId = uMediaSyncRelation2Parent.ChildId;

                        IMedia media2Parent = uMediaSyncHelper.mediaService.GetById(media2ParentId);

                        IMedia media2 = uMediaSyncHelper.mediaService.CreateMedia(content1.Name, media2Parent, "Folder", uMediaSyncHelper.userId);

                        uMediaSyncHelper.mediaService.Save(media2, uMediaSyncHelper.userId);

                        CopyMedia(media1, media2);

                        IRelation relation = uMediaSyncHelper.relationService.Relate(content2, media2, "uMediaSyncRelation");
                        uMediaSyncHelper.relationService.Save(relation);

                        uMediaSyncHelper.contentService.SaveAndPublishWithStatus(content2, uMediaSyncHelper.userId);
                    }
                }
            }
        }

        void ContentService_Deleting(IContentService sender, global::Umbraco.Core.Events.DeleteEventArgs<IContent> e)
        {
            if (ReadSetting("deleteMedia").ToString() == "true")
            {
                foreach (var node in e.DeletedEntities)
                {
                    if (syncNode(node))
                    {
                        IEnumerable<IRelation> uMediaSyncRelations = uMediaSyncHelper.relationService.GetByRelationTypeAlias("uMediaSyncRelation");
                        IRelation uMediaSyncRelation = uMediaSyncRelations.Where(r => r.ParentId == node.Id).FirstOrDefault();
                        if (uMediaSyncRelation != null)
                        {
                            int mediaId = uMediaSyncRelation.ChildId;
                            IMedia media = uMediaSyncHelper.mediaService.GetById(mediaId);
                            uMediaSyncHelper.mediaService.Delete(media, uMediaSyncHelper.userId);
                        }
                    }
                }
            }
        }

        void ContentService_Trashed(IContentService sender, global::Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            if (ReadSetting("deleteMedia").ToString() == "true")
            {
                if (syncNode(e.Entity))
                {
                    IEnumerable<IRelation> uMediaSyncRelations = uMediaSyncHelper.relationService.GetByRelationTypeAlias("uMediaSyncRelation");
                    IRelation uMediaSyncRelation = uMediaSyncRelations.Where(r => r.ParentId == e.Entity.Id).FirstOrDefault();
                    if (uMediaSyncRelation != null)
                    {
                        int mediaId = uMediaSyncRelation.ChildId;
                        IMedia media = uMediaSyncHelper.mediaService.GetById(mediaId);
                        uMediaSyncHelper.mediaService.MoveToRecycleBin(media, uMediaSyncHelper.userId);
                    }
                }
            }
        }

       

        

        protected string ReadSetting(string key)
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


        protected List<string> ReadBlacklist(string key)
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
                            foreach(XmlNode typeNode in subNode.ChildNodes)
                            {
                                if (typeNode.Name == "docTypeAlias")
                                {
                                    settings.Add(typeNode.InnerText);
                                }
                            }
                        }
                        else if(key=="level" && subNode.Name == "blackListLevel")
                        {
                            settings.Add(subNode.InnerText);
                        }
                    }
                }
            }
            return settings;
        }

        protected bool syncNode(IContent content)
        {
            bool sync = false;

            if (Convert.ToBoolean(ReadSetting("syncAllContent")) == true)
            {
                sync = true;
            } else if (!IsSyncHide(content))
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

        protected bool IsSyncHide(IContent node)
        {
            IContent nodeCopy = node;
            List<string> blackListResult = ReadBlacklist("docTypes");

            bool syncHide = false;
            while (node.Level>1 && syncHide==false) 
            {
                if (node.HasProperty("uMediaSyncHide") && node.GetValue<bool>("uMediaSyncHide") != null &&  node.GetValue<bool>("uMediaSyncHide")==true)
                {
                    syncHide = true;
                }
                if (blackListResult.Where(x => x == node.ContentType.Alias.ToString()).Count()!=0)
                {
                    syncHide = true;
                }

                node = node.Parent();
            }
            
            return syncHide;
        }

        

        private void CopyMedia(IMedia media1Parent, IMedia media2Parent)
        {
            if (uMediaSyncHelper.mediaService.HasChildren(media1Parent.Id))
            {
                foreach (IMedia item in uMediaSyncHelper.mediaService.GetChildren(media1Parent.Id))
                {
                    var mediaItem = uMediaSyncHelper.mediaService.CreateMedia(item.Name, media2Parent, item.ContentType.Alias, uMediaSyncHelper.userId);

                    if (item.HasProperty("umbracoFile") && !String.IsNullOrEmpty(item.GetValue("umbracoFile").ToString()))
                    {
                        string mediaFile = item.GetValue("umbracoFile").ToString();

                        string newFile = HttpContext.Current.Server.MapPath(mediaFile);

                        if (System.IO.File.Exists(newFile))
                        {
                            string fName = mediaFile.Substring(mediaFile.LastIndexOf('/') + 1);

                            FileStream fs = System.IO.File.OpenRead(HttpContext.Current.Server.MapPath(mediaFile));

                            mediaItem.SetValue("umbracoFile", fName, fs);
                        }
                    }

                    uMediaSyncHelper.mediaService.Save(mediaItem, uMediaSyncHelper.userId);
                    if (uMediaSyncHelper.mediaService.GetChildren(item.Id).Count() != 0)
                    {
                        CopyMedia(item, mediaItem);
                    }
                }
            }
        }   
    }
}