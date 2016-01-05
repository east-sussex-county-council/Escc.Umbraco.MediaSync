using System;
using Umbraco.Inception.Attributes;
using Umbraco.Inception.BL;

namespace Escc.Umbraco.MediaSync.MediaTypes
{
    /// <summary>
    /// Recreation of the File media type which comes by default with Umbraco, allowing it to be extended with extra properties
    /// </summary>
    [UmbracoMediaType("File", "File", new Type[] { }, "", BuiltInUmbracoContentTypeIcons.IconDocument, false, false)]
    public class FileMediaType : UmbracoGeneratedBase
    {
        /// <summary>
        /// Gets or sets the file tab which comes by default with Umbraco.
        /// </summary>
        /// <value>
        /// The file tab.
        /// </value>
        [UmbracoTab("File", SortOrder = 1)]
        public FileTab FileTab { get; set; }
    }
}