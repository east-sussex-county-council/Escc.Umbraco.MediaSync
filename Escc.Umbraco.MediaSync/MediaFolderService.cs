﻿using System;
using System.Collections.Generic;
using System.Linq;
using Escc.Umbraco.MediaSync.Helpers;
using Umbraco.Core.Models;

namespace Escc.Umbraco.MediaSync
{
    public class MediaFolderService
    {
        private readonly IMediaSyncConfigurationProvider _config = new MediaSyncConfigurationFromXml();

        /// <summary>
        /// Checks that the given content node has a relation to a media node, and creates a related media node if not
        /// </summary>
        /// <param name="node">The content node.</param>
        public void EnsureRelatedMediaNodeExists(IContent node)
        {
            try
            { 
            int contentRoot = String.IsNullOrWhiteSpace(_config.ReadSetting("syncFromContentRootNode")) ? -1 : Convert.ToInt32(_config.ReadSetting("syncFromContentRootNode"));

            IContent contentRootNode = uMediaSyncHelper.contentService.GetById(contentRoot);

            if (contentRoot == -1 || node.Path.Contains(contentRootNode.Path))
            {
                if (_config.SyncNode(node))
                {
                    IRelation uMediaSyncRelation = uMediaSyncHelper.relationService.GetByParentId(node.Id).FirstOrDefault(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);

                    if (uMediaSyncRelation == null)
                    {
                        CreateRelatedMediaNode(node);
                    }
                }
            }
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("Content node id"))
                {
                    ex.Data.Add("Content node id", node.Id);
                }
                throw;
            }
        }


        /// <summary>
        /// Creates a media node to match a content node, and relates them.
        /// </summary>
        /// <param name="node">The content node.</param>
        public void CreateRelatedMediaNode(IContent node)
        {
            IMedia media = null;
            try
            {
                int contentRoot = String.IsNullOrWhiteSpace(_config.ReadSetting("syncFromContentRootNode")) ? -1 : Convert.ToInt32(_config.ReadSetting("syncFromContentRootNode"));
                int mediaParent = String.IsNullOrWhiteSpace(_config.ReadSetting("syncToMediaRootNode")) ? -1 : Convert.ToInt32(_config.ReadSetting("syncToMediaRootNode"));

                IContent contentRootNode = uMediaSyncHelper.contentService.GetById(contentRoot);

                if (contentRoot == -1 || node.Path.Contains(contentRootNode.Path))
                {
                    if (_config.SyncNode(node))
                    {
                        if (node.ParentId != contentRoot)
                        {
                            IEnumerable<IRelation> uMediaSyncRelationsBefore = uMediaSyncHelper.relationService.GetByParentId(node.ParentId).Where(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
                            IRelation uMediaSyncRelation = uMediaSyncRelationsBefore.FirstOrDefault();

                            if (uMediaSyncRelation == null && Boolean.Parse(_config.ReadSetting("checkForMissingRelations")))
                            {
                                // parent node doesn't have a media folder yet, probably because uMediaSync was installed after the node was created
                                CreateRelatedMediaNode(node.Parent());

                                // get the new relation for the parent
                                IEnumerable<IRelation> uMediaSyncRelationsAfter = uMediaSyncHelper.relationService.GetByParentId(node.ParentId).Where(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
                                uMediaSyncRelation = uMediaSyncRelationsAfter.FirstOrDefault();
                            }

                            if (uMediaSyncRelation != null) mediaParent = uMediaSyncRelation.ChildId;
                        }

                        media = uMediaSyncHelper.mediaService.CreateMedia(node.Name, mediaParent, "Folder", uMediaSyncHelper.userId);
                        uMediaSyncHelper.mediaService.Save(media);
                        EnsureRelationTypeExists();
                        IRelation relation = uMediaSyncHelper.relationService.Relate(node, media, Constants.FolderRelationTypeAlias);
                        uMediaSyncHelper.relationService.Save(relation);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("Content node id"))
                {
                    ex.Data.Add("Content node id", node.Id);
                }
                if (!ex.Data.Contains("Media node id"))
                {
                    ex.Data.Add("Media node id", media?.Id);
                }
                throw;
            }
        }
        /// <summary>
        /// For a content node, moves its related media node to the correct location.
        /// </summary>
        /// <param name="contentNode">The content node.</param>
        public void MoveRelatedMediaNode(IContent contentNode)
        {
            int? mediaId = null;
            int? mediaParentNewId = null;
            try
            {
                IEnumerable<IRelation> uMediaSyncRelationsBefore = uMediaSyncHelper.relationService.GetByParentId(contentNode.Id).Where(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
                IRelation uMediaSyncRelation = uMediaSyncRelationsBefore.FirstOrDefault();
                if (uMediaSyncRelation != null)
                {
                    mediaId = uMediaSyncRelation.ChildId;

                    IEnumerable<IRelation> uMediaSyncRelationsNew = uMediaSyncHelper.relationService.GetByParentId(contentNode.ParentId).Where(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
                    IRelation uMediaSyncRelationNew = uMediaSyncRelationsNew.FirstOrDefault();

                    if (uMediaSyncRelationNew == null && _config.ReadBooleanSetting("checkForMissingRelations"))
                    {
                        // parent node doesn't have a media folder yet, probably because uMediaSync was installed after the node was created
                        CreateRelatedMediaNode(contentNode.Parent());

                        // get the new relation for the parent
                        IEnumerable<IRelation> uMediaSyncRelationsAfter = uMediaSyncHelper.relationService.GetByParentId(contentNode.ParentId).Where(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
                        uMediaSyncRelationNew = uMediaSyncRelationsAfter.FirstOrDefault();
                    }

                    if (uMediaSyncRelationNew != null)
                    {
                        mediaParentNewId = uMediaSyncRelationNew.ChildId;

                        IMedia media = uMediaSyncHelper.mediaService.GetById(mediaId.Value);
                        IMedia mediaParentNew = uMediaSyncHelper.mediaService.GetById(mediaParentNewId.Value);

                        uMediaSyncHelper.mediaService.Move(media, mediaParentNew.Id, uMediaSyncHelper.userId);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("Content node id"))
                {
                    ex.Data.Add("Content node id", contentNode.Id);
                }
                if (!ex.Data.Contains("Media node id") && mediaId.HasValue)
                {
                    ex.Data.Add("Media node id", mediaId.Value);
                }
                if (!ex.Data.Contains("New parent media node id") && mediaParentNewId.HasValue)
                {
                    ex.Data.Add("New parent media node id", mediaParentNewId.Value);
                }
                throw;
            }
        }

        /// <summary>
        /// Moves the related media node to the recycle bin, preserving any files that are still needed by moving them to another folder.
        /// </summary>
        /// <param name="contentNodeId">The content node identifier.</param>
        public void MoveRelatedMediaNodeToRecycleBin(int contentNodeId)
        {
            int? mediaId = null;
            try
            {
                IEnumerable<IRelation> uMediaSyncRelations = uMediaSyncHelper.relationService.GetByParentId(contentNodeId).Where(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
                IRelation uMediaSyncRelation = uMediaSyncRelations.FirstOrDefault();
                if (uMediaSyncRelation != null)
                {
                    mediaId = uMediaSyncRelation.ChildId;
                    IMedia media = uMediaSyncHelper.mediaService.GetById(mediaId.Value);

                    // Check - does this media folder have another associated content node? It shouldn't, because it should be a one-to-one relationship, 
                    // but it is possible somehow to get into a situation where it does. If there's another content node related to this media folder, just
                    // remove the relationship to the content node being trashed. 
                    var contentRelatedToMedia = uMediaSyncHelper.relationService.GetByChildId(mediaId.Value).Where(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
                    if (contentRelatedToMedia.Count() > 1)
                    {
                        uMediaSyncHelper.relationService.Delete(uMediaSyncRelation);
                    }
                    else
                    {
                        // If all is normal and there's just one relationship, move any files that have a relationship with another content node, 
                        // then move the media folder to the media recycle bin as the content node moves to the content recycle bin.
                        MoveFilesInFolderIfStillInUse(contentNodeId, media);

                        uMediaSyncHelper.mediaService.MoveToRecycleBin(media, uMediaSyncHelper.userId);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("Content node id"))
                {
                    ex.Data.Add("Content node id", contentNodeId);
                }
                if (!ex.Data.Contains("Media node id") && mediaId.HasValue)
                {
                    ex.Data.Add("Media node id", mediaId.Value);
                }
                throw;
            }
        }

        /// <summary>
        /// Deletes a media node, preserving any files that are still needed by moving them to another folder.
        /// </summary>
        /// <param name="contentNodeId">The content node identifier.</param>
        public void DeleteRelatedMediaNode(int contentNodeId)
        {
            int? mediaId = null;
            try
            { 
            IEnumerable<IRelation> uMediaSyncRelations = uMediaSyncHelper.relationService.GetByParentId(contentNodeId).Where(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
            IRelation uMediaSyncRelation = uMediaSyncRelations.FirstOrDefault();
            if (uMediaSyncRelation != null)
            {
                mediaId = uMediaSyncRelation.ChildId;
                IMedia media = uMediaSyncHelper.mediaService.GetById(mediaId.Value);

                // Check - does this media folder have another associated content node? It shouldn't, because it should be a one-to-one relationship, 
                // but it is possible somehow to get into a situation where it does. 
                var contentRelatedToMedia = uMediaSyncHelper.relationService.GetByChildId(mediaId.Value).Where(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias && r.ParentId != contentNodeId);
                var nextContentRelation = contentRelatedToMedia.FirstOrDefault();
                if (nextContentRelation != null)
                {
                    // If there's another content node related to this media folder, remove the relationship to the content node being deleted. 
                    uMediaSyncHelper.relationService.Delete(uMediaSyncRelation);
                    
                    // Then move the media node to the correct place for the next related content node
                    MoveRelatedMediaNode(uMediaSyncHelper.contentService.GetById(nextContentRelation.ParentId));
                }
                else
                {
                    // If all is normal and there's just one relationship, move any files that have a relationship with another content node, 
                    // then delete the media folder and any remaining files

                    MoveFilesInFolderIfStillInUse(contentNodeId, media);

                    uMediaSyncHelper.mediaService.Delete(media, uMediaSyncHelper.userId);
                }
            }
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("Content node id"))
                {
                    ex.Data.Add("Content node id", contentNodeId);
                }
                if (!ex.Data.Contains("Media node id") && mediaId.HasValue)
                {
                    ex.Data.Add("Media node id", mediaId.Value);
                }
                throw;
            }
        }


        private void MoveFilesInFolderIfStillInUse(int contentNodeId, IMedia folder)
        {
            int? mediaParentNewId = null;
            try
            {
                var decendants = uMediaSyncHelper.mediaService.GetDescendants(folder);
                foreach (var mediaItem in decendants)
                {
                    if (mediaItem.ContentType.Alias.ToUpperInvariant() == "FOLDER") continue;

                    // Check whether another page uses this media item. Could be more than one but just grab the first - we have no way of working out which is the "most appropriate".
                    var anotherPageUsingThisMediaItem = uMediaSyncHelper.relationService.GetByChildId(mediaItem.Id)
                        .FirstOrDefault(r => r.RelationType.Alias == Constants.FileRelationTypeAlias && r.ParentId != contentNodeId);

                    if (anotherPageUsingThisMediaItem != null)
                    {
                        // Look up media folder for that page
                        var mediaFolderForPage = uMediaSyncHelper.relationService.GetByParentId(anotherPageUsingThisMediaItem.ParentId).FirstOrDefault(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
                        if (mediaFolderForPage == null && _config.ReadBooleanSetting("checkForMissingRelations"))
                        {
                            // parent node doesn't have a media folder yet, probably because uMediaSync was installed after the node was created
                            var contentNode = uMediaSyncHelper.contentService.GetById(anotherPageUsingThisMediaItem.ParentId);
                            CreateRelatedMediaNode(contentNode);

                            mediaFolderForPage = uMediaSyncHelper.relationService.GetByParentId(anotherPageUsingThisMediaItem.ParentId).FirstOrDefault(r => r.RelationType.Alias == Constants.FolderRelationTypeAlias);
                        }

                        // Move the media item to the media folder for the other page, so that it doesn't get deleted
                        if (mediaFolderForPage != null)
                        {
                            mediaParentNewId = mediaFolderForPage.ChildId;
                            uMediaSyncHelper.mediaService.Move(mediaItem, mediaParentNewId.Value);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("Content node id"))
                {
                    ex.Data.Add("Content node id", contentNodeId);
                }
                if (!ex.Data.Contains("Media node id"))
                {
                    ex.Data.Add("Media folder id", folder.Id);
                }
                if (!ex.Data.Contains("New parent media node id") && mediaParentNewId.HasValue)
                {
                    ex.Data.Add("New parent media node id", mediaParentNewId.Value);
                }
                throw;
            }
        }

        /// <summary>
        /// Ensures the relation type exists between a content node and a media folder for that node.
        /// </summary>
        private static void EnsureRelationTypeExists()
        {
            if (uMediaSyncHelper.relationService.GetRelationTypeByAlias(Constants.FolderRelationTypeAlias) == null)
            {
                var relationType = new RelationType(new Guid("b796f64c-1f99-4ffb-b886-4bf4bc011a9c"), new Guid("c66ba18e-eaf3-4cff-8a22-41b16d66a972"), Constants.FolderRelationTypeAlias, Constants.FolderRelationTypeAlias);
                uMediaSyncHelper.relationService.Save(relationType);
            }
        }
    }
}
