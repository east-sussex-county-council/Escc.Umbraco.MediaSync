using Umbraco.Inception.Attributes;
using Umbraco.Inception.BL;

namespace Escc.Umbraco.MediaSync.MediaTypes
{
    /// <summary>
    /// The File tab of the File media type which comes with Umbraco, but with additional properties added
    /// </summary>
    public class FileTab : TabBase
    {
        /// <summary>
        /// Built-in field to upload a file.
        /// </summary>
        /// <value>
        /// The upload file.
        /// </value>
        [UmbracoProperty("Upload file", "umbracoFile", BuiltInUmbracoDataTypes.UploadField, sortOrder: 1, addTabAliasToPropertyAlias: false)]
        public string UploadFile { get; set; }

        /// <summary>
        /// Built-in field which records the uploaded file type
        /// </summary>
        [UmbracoProperty("Type", "umbracoExtension", BuiltInUmbracoDataTypes.NoEdit, sortOrder: 2, addTabAliasToPropertyAlias: false)]
        public string Type { get; set; }

        /// <summary>
        /// Built-in field which recors the file size
        /// </summary>
        [UmbracoProperty("Size", "umbracoBytes", BuiltInUmbracoDataTypes.NoEdit, sortOrder: 3, addTabAliasToPropertyAlias: false)]
        public string Size { get; set; }

        /// <summary>
        /// New field to add a description for the file
        /// </summary>
        [UmbracoProperty("Description", "Description", BuiltInUmbracoDataTypes.TextboxMultiple, sortOrder: 4, addTabAliasToPropertyAlias: false)]
        public string Description { get; set; }

        /// <summary>
        /// New field to show where media item is used.
        /// </summary>
        [UmbracoProperty("Media Content Usage", "MediaContentUsage", "Escc.MediaContentUsage", sortOrder: 5, addTabAliasToPropertyAlias: false)]
        public string MediaContentUsage { get; set; }
    }
}