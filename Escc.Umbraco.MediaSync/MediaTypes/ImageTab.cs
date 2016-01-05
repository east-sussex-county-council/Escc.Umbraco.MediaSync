using Umbraco.Inception.Attributes;
using Umbraco.Inception.BL;

namespace Escc.Umbraco.MediaSync.MediaTypes
{
    public class ImageTab : TabBase
    {
        /// <summary>
        /// Built-in field to upload an image.
        /// </summary>
        /// <value>
        /// The upload image.
        /// </value>
        [UmbracoProperty("Upload image", "umbracoFile", BuiltInUmbracoDataTypes.UploadField, sortOrder: 1, addTabAliasToPropertyAlias: false)]
        public string UploadFile { get; set; }

        [UmbracoProperty("Width", "umbracoWidth", BuiltInUmbracoDataTypes.NoEdit, sortOrder: 2, addTabAliasToPropertyAlias: false)]
        public string Width { get; set; }

        [UmbracoProperty("Height", "umbracoHeight", BuiltInUmbracoDataTypes.NoEdit, sortOrder: 3, addTabAliasToPropertyAlias: false)]
        public string Height { get; set; }

        [UmbracoProperty("Size", "umbracoBytes", BuiltInUmbracoDataTypes.NoEdit, sortOrder: 4, addTabAliasToPropertyAlias: false)]
        public string Size { get; set; }

        [UmbracoProperty("Type", "umbracoExtension", BuiltInUmbracoDataTypes.NoEdit, sortOrder: 5, addTabAliasToPropertyAlias: false)]
        public string Type { get; set; }

        /// <summary>
        /// New field to show where media item is used.
        /// </summary>
        [UmbracoProperty("Media Content Usage", "MediaContentUsage", "Escc.MediaContentUsage", sortOrder: 6, addTabAliasToPropertyAlias: false)]
        public string MediaContentUsage { get; set; }
    }
}
