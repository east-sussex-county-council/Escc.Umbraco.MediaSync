using System;
using System.Collections.Generic;
using System.Linq;
using Escc.Umbraco.Media;
using Exceptionless;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Constants = Escc.Umbraco.MediaSync.Helpers.Constants;

namespace Escc.Umbraco.MediaSync
{
    /// <summary>
    /// Create a relationship between content pages and the media items used by those pages
    /// </summary>
    public class MediaFileSync : ApplicationEventHandler
    {
        private readonly IMediaSyncConfigurationProvider _config = new MediaSyncConfigurationFromXml();
        private IEnumerable<IMediaIdProvider> _mediaIdProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFileSync"/> class.
        /// </summary>
        public MediaFileSync()
        {
            ContentService.Saved += ContentService_Saved;
            ContentService.Copied += ContentService_Copied;
        }


        /// <summary>
        /// Updates relations after a content node has been saved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ContentService_Saved(IContentService sender, SaveEventArgs<IContent> e)
        {
            try
            {
                if (_config.ReadBooleanSetting("moveMediaFilesStillInUse"))
                {
                    EnsureRelationTypeExists();

                    if (_mediaIdProviders == null)
                    {
                        _mediaIdProviders = new MediaIdProvidersFromConfig(ApplicationContext.Current.Services.MediaService, ApplicationContext.Current.Services.DataTypeService).LoadProviders();
                    }

                    foreach (var node in e.SavedEntities)
                    {
                        UpdateRelationsBetweenContentAndMediaItems(node);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                throw; // throw to the generic handler that writes to the Umbraco log
            }

        }


        /// <summary>
        /// Copy relations to media items when a page is copied.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void ContentService_Copied(IContentService sender, CopyEventArgs<IContent> e)
        {
            try
            {
                if (_config.ReadBooleanSetting("moveMediaFilesStillInUse"))
                {
                    var fileRelations = uMediaSyncHelper.relationService.GetByParent(e.Original).Where(r => r.RelationType.Alias == Constants.FileRelationTypeAlias);
                    foreach (var relation in fileRelations)
                    {
                        var media = uMediaSyncHelper.mediaService.GetById(relation.ChildId);
                        var newRelation = uMediaSyncHelper.relationService.Relate(e.Copy, media, Constants.FileRelationTypeAlias);
                        uMediaSyncHelper.relationService.Save(newRelation);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                throw; // throw to the generic handler that writes to the Umbraco log
            }

        }

        private void UpdateRelationsBetweenContentAndMediaItems(IContent node)
        {
            // Tried using ICanBeDirty and IRememberBeingDirty using the pattern from 
            // http://stackoverflow.com/questions/24035586/umbraco-memberservice-saved-event-trigger-during-login-and-get-operations
            // but although it identified the node as dirty, it didn't identify any media picker properties as dirty when they changed.

            // Get the relations as they stood before the latest save

            var uMediaSyncRelations = uMediaSyncHelper.relationService.GetByParentId(node.Id).Where(r => r.RelationType.Alias == Constants.FileRelationTypeAlias);
            var relationsForPageBeforeSave = uMediaSyncRelations.ToList();

            var relatedMediaIds = relationsForPageBeforeSave.Select(r => r.ChildId).ToList();

            // Look through the properties to see what relations we have now
            var relatedMediaIdsInCurrentVersion = new List<int>();

            foreach (var propertyType in node.PropertyTypes)
            {
                foreach (var provider in _mediaIdProviders)
                {
                    if (!provider.CanReadPropertyType(propertyType)) continue;

                    var mediaIds = provider.ReadProperty(node.Properties[propertyType.Alias]);
                    foreach (var mediaNodeId in mediaIds)
                    {
                        // We've got a property linking to a media node. 
                        // Has there already been a link to the same media node on this page?
                        if (!relatedMediaIdsInCurrentVersion.Contains(mediaNodeId))
                        {
                            relatedMediaIdsInCurrentVersion.Add(mediaNodeId);
                        }

                        // Was there a link to this item before the current save?
                        if (!relatedMediaIds.Contains(mediaNodeId))
                        {
                            // If not, create a new relation
                            var mediaItem = uMediaSyncHelper.mediaService.GetById(mediaNodeId);
                            if (mediaItem != null)
                            {
                                IRelation relation = uMediaSyncHelper.relationService.Relate(node, mediaItem, Constants.FileRelationTypeAlias);
                                uMediaSyncHelper.relationService.Save(relation);

                                relatedMediaIds.Add(mediaNodeId);
                            }
                        }
                    }

                }
            }

            // Remove relations for any media items which were in use before the save but are now gone
            relationsForPageBeforeSave.RemoveAll(r => relatedMediaIdsInCurrentVersion.Contains(r.ChildId));
            foreach (var mediaRelation in relationsForPageBeforeSave)
            {
                uMediaSyncHelper.relationService.Delete(mediaRelation);
            }

            // go through each relatedMediaId and check the media files are in the right place for each relation
            EnsureMediaFileInCorrectFolder(relatedMediaIds, node.Id);
        }

        /// <summary>
        /// Ensure media items are in the folder related to the content node, unless it is already in use elsewhere
        /// </summary>
        /// <param name="relatedMediaIds">A list of all media items to check</param>
        /// <param name="contentNodeId">Id of the content node</param>
        private void EnsureMediaFileInCorrectFolder(IEnumerable<int> relatedMediaIds, int contentNodeId)
        {
            // Get the Content to Media Folder relation
            var contentMediaFolderRelation = uMediaSyncHelper.relationService.GetByParentId(contentNodeId).FirstOrDefault(p => p.RelationType.Alias == Constants.FolderRelationTypeAlias);

            // Should this ever happen?
            if (contentMediaFolderRelation == null) return;

            foreach (var mediaId in relatedMediaIds)
            {
                // Get all file relations for the Media Item
                // the datetime field on the umbracoRelation table is not available, so sort by Id to get "oldest first" order.
                var relations = uMediaSyncHelper.relationService.GetByChildId(mediaId).Where(r => r.RelationType.Alias == Constants.FileRelationTypeAlias).OrderBy(o => o.Id);
                var mediaItemParentFolderId = -1;

                // Check that there is at least one relation
                if (!relations.Any())
                {
                    // no action, leave media where it is
                    continue;
                }

                // At least one Relation exists. Get the first / oldest added relation and ensure the media item is in the related media folder

                // Check that the media item for the first relation is in the correct folder
                var relation = relations.First();
                var mediaItemParentFolder = uMediaSyncHelper.mediaService.GetParent(relation.ChildId);

                // If the media item is at the root, GetParent seems to return null rather than the Media Root node which has an id of -1
                if (mediaItemParentFolder != null)
                {
                    mediaItemParentFolderId = mediaItemParentFolder.Id;
                }

                // Get the Content to Media Folder relation for the first relation
                contentMediaFolderRelation = uMediaSyncHelper.relationService.GetByParentId(relation.ParentId).FirstOrDefault(p => p.RelationType.Alias == Constants.FolderRelationTypeAlias);
                // Should this ever happen?
                if (contentMediaFolderRelation == null) continue;

                // Get the Media folder for this Content item
                var contentMediaFolderId = contentMediaFolderRelation.ChildId;

                if (mediaItemParentFolderId == contentMediaFolderId) continue;

                // Media is in the wrong place, so move it
                var mediaItem = uMediaSyncHelper.mediaService.GetById(relation.ChildId);
                uMediaSyncHelper.mediaService.Move(mediaItem, contentMediaFolderId);
            }
        }

        /// <summary>
        /// Ensures the relation type exists between content nodes and the media items they use.
        /// </summary>
        private void EnsureRelationTypeExists()
        {
            if (uMediaSyncHelper.relationService.GetRelationTypeByAlias(Constants.FileRelationTypeAlias) == null)
            {
                var relationType = new RelationType(new Guid("b796f64c-1f99-4ffb-b886-4bf4bc011a9c"), new Guid("c66ba18e-eaf3-4cff-8a22-41b16d66a972"), Constants.FileRelationTypeAlias, Constants.FileRelationTypeAlias);
                uMediaSyncHelper.relationService.Save(relationType);
            }
        }
    }
}
