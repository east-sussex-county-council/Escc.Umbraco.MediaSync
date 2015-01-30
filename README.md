# Escc.Umbraco.MediaSync

This is [uMediaSync](http://our.umbraco.org/projects/backoffice-extensions/umediasync) for Umbraco 7 by Sören Deger, repackaged as a NuGet package rather than an Umbraco package. We use [NuBuild](https://github.com/bspell1/NuBuild) to make creating the NuGet package really easy, and [reference our private feed using a nuget.config file](http://blog.davidebbo.com/2014/01/the-right-way-to-restore-nuget-packages.html).

In addition to the features of uMediaSync, this adds:

* Create media folders retrospectively for pages which existed before Escc.Umbraco.MediaSync was installed
* Delete the related media folders from the media recycle bin when the content recycle bin is emptied

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

#### checkForMissingRelations

If you install Escc.Umbraco.MediaSync on a site which already has content, the matching related media nodes may be missing. Setting this property to `true` checks for these and puts them in place if they're missing any time a save, copy or move is attempted. To force the creation of a media library for an existing page which doesn't have one, simply save the page. 

If you are installing on a new site without any content you can set this to `false` to avoid the extra queries this involves.

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