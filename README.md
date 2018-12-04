# Escc.Umbraco.MediaSync

This is [uMediaSync](http://our.umbraco.org/projects/backoffice-extensions/umediasync) for Umbraco 7 by Sören Deger, repackaged as a NuGet package rather than an Umbraco package. 

In addition to the features of uMediaSync, this adds:

* Create media folders retrospectively for pages which existed before Escc.Umbraco.MediaSync was installed.
* Delete the related media folders from the media recycle bin when the content recycle bin is emptied.
* When a page is deleted, and its media folder contains files that are also used by other pages, those files are moved to the media folder for one of the other pages.
* An extra option to decide, when copying a media folder tree with a page, whether also to copy the files contained in its folders. The copied page still refers to the original files, so it may not be helpful to make copies.
* When a page is copied, it is not automatically published. This replicates how Umbraco works by default.
* When modifying Umbraco content from an Umbraco Web API, supports getting the userId from `uMediaSync.config` instead of using the identify of the current user. 
* An property editor assigned to an extra property on the Image and File media types to show where the media item is used.

## uMediaSync

uMediaSync implemented an automatic one-way synchronization between content and media nodes within the CMS Umbraco backend.

It is often useful if the structure in the media section and content section is the same.  After installing a new media node is always created from the media type `Folder` if a new content node was created. There is a configuration file in config folder. With this config file you can made any settings, such as add documenttypes to blacklist.

### Configuration settings

#### syncFromContentRootNode
NodeID of the start node in the content section, may be used to start the synchronization. All content nodes which are not located below this node, but above or next to it will not be synchronized.

Default: -1

#### syncToMediaRootNode
NodeID of the start node in media section, to which the start node of the content area (determined in syncFromContentRootNode) synchronized.

Default: -1

#### syncAllContent
Allowed values:

* true: All nodes are always synchronized (blacklist will NOT be considered)
* false: There are only synchronized nodes which are not excluded in the blacklist and which are not in a level equal to or below the value specified in the blacklist level.

#### renameMedia
Allowed values:

* true: Media nodes are renamed automatically if the related content node is renamed.
* false: The renaming of a content node has no effect on the related media nodes.

#### deleteMedia
Allowed values:

* true: media node is moved to the recycle bin or deleted if the related content node is moved to the recycle bin or deleted.
* false: The delete or move to the recycle bin of the content node has no effect to the related media node. This will continue exist.

#### copyMediaFiles

Allowed values:

* true: when copying a content node and its related media folder tree, copy the files in the media folders too 
* false: when copying a content node copy its related media folder and subfolders, but without the files they contain

#### checkForMissingRelations

If you install Escc.Umbraco.MediaSync on a site which already has content, the matching related media nodes may be missing. Setting this property to `true` checks for these and puts them in place if they're missing any time a save, copy or move is attempted. To force the creation of a media library for an existing page which doesn't have one, simply save the page. 

If you are installing on a new site without any content you can set this to `false` to avoid the extra queries this involves.

#### moveMediaFilesStillInUse

Allowed values:

* true: When a content node is deleted, and its media node contains files that are also used by other content nodes, those files are moved to the media node for one of the other content nodes.
* false: When a content node is deleted its related media folder and all its files are deleted.

#### userId

When you're logged in to the Umbraco back office, the current user id is used by Escc.Umbraco.MediaSync to makes its updates to Umbraco. When you call the content service from within an `UmbracoApiController` you don't necessarily have a current user, so the user id from here is used as a fallback option.

### Excluding content from synchronisation

Pages can be added to a blacklist in `uMediaSync.config`:

	<configuration>
	  <blackList>
	    <blackListDocTypes>
	      <docTypeAlias>Textpage</docTypeAlias>
	    </blackListDocTypes>
	    <blackListLevel>10</blackListLevel>
	  </blackList>
	</configuration>

`docTypeAlias` lists the aliases of one or more document types which should not be synchronized. Multiple aliases should be separated with spaces within a single `docTypeAlias` tag. uMediaSync checks all aliases recursively.

`blackListLevel` specifies the level of content section at and below which no content nodes should be synchronized.



#### Document type property: uMediaSyncHide

There is another way to exclude individual single content nodes or even the whole branches from synchronization. Add a property with alias `uMediaSyncHide` of datatype `true/false` to your document type. If you select this property on a content node, this and all nodes under this are automatically excluded from synchronization.

## Known issues

* When you copy a content node which has child nodes, you get multiple copies of the media folder, only one of which has the correct relationship.

* If an image is uploaded using the `File` media type, copying files fails with an error saying it cannot find the `umbracoWidth` property.