using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Escc.Umbraco.MediaSync
{
    public class MediaFolderService
    {
        private readonly IMediaSyncConfigurationProvider _config = new XmlConfigurationProvider();

        /// <summary>
        /// Checks that the given content node has a relation to a media node, and creates a related media node if not
        /// </summary>
        /// <param name="node">The content node.</param>
        public void EnsureRelatedMediaNodeExists(IContent node)
        {
            int contentRoot = String.IsNullOrWhiteSpace(_config.ReadSetting("syncFromContentRootNode")) ? -1 : Convert.ToInt32(_config.ReadSetting("syncFromContentRootNode"));

            IContent contentRootNode = uMediaSyncHelper.contentService.GetById(contentRoot);

            if (contentRoot == -1 || node.Path.Contains(contentRootNode.Path))
            {
                if (_config.SyncNode(node) == true)
                {
                    IRelation uMediaSyncRelation = uMediaSyncHelper.relationService.GetByParentId(node.Id).FirstOrDefault(r => r.RelationType.Alias == "uMediaSyncRelation");

                    if (uMediaSyncRelation == null)
                    {
                        CreateRelatedMediaNode(node);
                    }
                }
            }
        }


        /// <summary>
        /// Creates a media node to match a content node, and relates them.
        /// </summary>
        /// <param name="node">The content node.</param>
        public void CreateRelatedMediaNode(IContent node)
        {
            int contentRoot = String.IsNullOrWhiteSpace(_config.ReadSetting("syncFromContentRootNode")) ? -1 : Convert.ToInt32(_config.ReadSetting("syncFromContentRootNode"));
            int mediaParent = String.IsNullOrWhiteSpace(_config.ReadSetting("syncToMediaRootNode")) ? -1 : Convert.ToInt32(_config.ReadSetting("syncToMediaRootNode"));

            IContent contentRootNode = uMediaSyncHelper.contentService.GetById(contentRoot);

            if (contentRoot == -1 || node.Path.Contains(contentRootNode.Path))
            {
                if (_config.SyncNode(node) == true)
                {
                    if (node.ParentId != contentRoot)
                    {
                        IEnumerable<IRelation> uMediaSyncRelationsBefore = uMediaSyncHelper.relationService.GetByParentId(node.ParentId).Where(r => r.RelationType.Alias == "uMediaSyncRelation");
                        IRelation uMediaSyncRelation = uMediaSyncRelationsBefore.FirstOrDefault();

                        if (uMediaSyncRelation == null && Boolean.Parse(_config.ReadSetting("checkForMissingRelations")))
                        {
                            // parent node doesn't have a media folder yet, probably because uMediaSync was installed after the node was created
                            CreateRelatedMediaNode(node.Parent());

                            // get the new relation for the parent
                            IEnumerable<IRelation> uMediaSyncRelationsAfter = uMediaSyncHelper.relationService.GetByParentId(node.ParentId).Where(r => r.RelationType.Alias == "uMediaSyncRelation");
                            uMediaSyncRelation = uMediaSyncRelationsAfter.FirstOrDefault();
                        }

                        mediaParent = uMediaSyncRelation.ChildId;
                    }

                    IMedia media = uMediaSyncHelper.mediaService.CreateMedia(node.Name, mediaParent, "Folder", uMediaSyncHelper.userId);
                    uMediaSyncHelper.mediaService.Save(media);
                    EnsureRelationTypeExists();
                    IRelation relation = uMediaSyncHelper.relationService.Relate(node, media, "uMediaSyncRelation");
                    uMediaSyncHelper.relationService.Save(relation);
                }
            }
        }

        /// <summary>
        /// Deletes a media node, preserving any files that are still needed by moving them to another folder.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        public void DeleteRelatedMediaNode(int nodeId)
        {
            IEnumerable<IRelation> uMediaSyncRelations = uMediaSyncHelper.relationService.GetByParentId(nodeId).Where(r => r.RelationType.Alias == "uMediaSyncRelation");
            IRelation uMediaSyncRelation = uMediaSyncRelations.FirstOrDefault();
            if (uMediaSyncRelation != null)
            {
                int mediaId = uMediaSyncRelation.ChildId;
                IMedia media = uMediaSyncHelper.mediaService.GetById(mediaId);

                MoveFilesInFolderIfStillInUse(nodeId, media);

                uMediaSyncHelper.mediaService.Delete(media, uMediaSyncHelper.userId);
            }
        }


        private void MoveFilesInFolderIfStillInUse(int contentNodeId, IMedia folder)
        {
            var decendants = uMediaSyncHelper.mediaService.GetDescendants(folder);
            foreach (var mediaItem in decendants)
            {
                if (mediaItem.ContentType.Alias.ToUpperInvariant() == "FOLDER") continue;

                // Check whether another page uses this media item. Could be more than one but just grab the first - we have no way of working out which is the "most appropriate".
                var anotherPageUsingThisMediaItem = uMediaSyncHelper.relationService.GetByChildId(mediaItem.Id)
                    .FirstOrDefault(r => r.RelationType.Alias == "uMediaSyncFileRelation" && r.ParentId != contentNodeId);

                if (anotherPageUsingThisMediaItem != null)
                {
                    // Look up media folder for that page
                    var mediaFolderForPage = uMediaSyncHelper.relationService.GetByParentId(anotherPageUsingThisMediaItem.ParentId).FirstOrDefault(r => r.RelationType.Alias == "uMediaSyncRelation");
                    if (mediaFolderForPage == null && _config.ReadBooleanSetting("checkForMissingRelations"))
                    {
                        // parent node doesn't have a media folder yet, probably because uMediaSync was installed after the node was created
                        var contentNode = uMediaSyncHelper.contentService.GetById(anotherPageUsingThisMediaItem.ParentId);
                        CreateRelatedMediaNode(contentNode);

                        mediaFolderForPage = uMediaSyncHelper.relationService.GetByParentId(anotherPageUsingThisMediaItem.ParentId).FirstOrDefault(r => r.RelationType.Alias == "uMediaSyncRelation");
                    }

                    // Move the media item to the media folder for the other page, so that it doesn't get deleted
                    if (mediaFolderForPage != null)
                    {
                        uMediaSyncHelper.mediaService.Move(mediaItem, mediaFolderForPage.ChildId);
                    }

                }
            }
        }

        /// <summary>
        /// Ensures the relation type exists between a content node and a media folder for that node.
        /// </summary>
        private static void EnsureRelationTypeExists()
        {
            if (uMediaSyncHelper.relationService.GetRelationTypeByAlias("uMediaSyncRelation") == null)
            {
                var relationType = new RelationType(new Guid("b796f64c-1f99-4ffb-b886-4bf4bc011a9c"), new Guid("c66ba18e-eaf3-4cff-8a22-41b16d66a972"), "uMediaSyncRelation", "uMediaSyncRelation");
                uMediaSyncHelper.relationService.Save(relationType);
            }
        }
    }
}
