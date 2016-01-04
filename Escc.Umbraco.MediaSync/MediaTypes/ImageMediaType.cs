using System;
using Umbraco.Inception.Attributes;

namespace Escc.Umbraco.MediaSync.MediaTypes
{
    /// <summary>
    /// Recreation of the Image media type which comes by default with Umbraco, allowing it to be extended with extra properties
    /// </summary>
    [UmbracoMediaType("Image", "Image", new Type[] { }, "", BuiltInUmbracoContentTypeIcons.IconPicture, false, false)]
    public class ImageMediaType
    {
        /// <summary>
        /// Gets or sets the image tab which comes by default with Umbraco.
        /// </summary>
        /// <value>
        /// The file tab.
        /// </value>
        [UmbracoTab("Image", SortOrder = 1)]
        public ImageTab ImageTab { get; set; }
    }
}
