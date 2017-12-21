using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Umbraco.Core.Models;

namespace Escc.Umbraco.MediaSync
{
    /// <summary>
    /// Gets the id of a media item when a property contains just its URL
    /// </summary>
    public class UrlMediaIdProvider : IRelatedMediaIdProvider
    {
        private readonly List<string> _propertyTypeAlises = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlMediaIdProvider" /> class.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider.</param>
        public UrlMediaIdProvider(IMediaSyncConfigurationProvider configurationProvider)
        {
            _propertyTypeAlises.AddRange(configurationProvider.ReadPropertyEditorAliases("urlMediaIdProvider"));
        }

        /// <summary>
        /// Determines whether this instance can read the type of property identified by its property editor alias
        /// </summary>
        /// <param name="propertyType">The property defined on the document type.</param>
        /// <returns></returns>
        public bool CanReadPropertyType(PropertyType propertyType)
        {
            return _propertyTypeAlises.Contains(propertyType.PropertyEditorAlias.ToUpperInvariant());
        }

        /// <summary>
        /// Reads media ids from the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public IEnumerable<int> ReadProperty(Property property)
        {
            var mediaIds = new List<int>();

            if (property != null && property.Value != null && !String.IsNullOrEmpty(property.Value.ToString()))
            {
                var uri = new Uri(property.Value.ToString(), UriKind.RelativeOrAbsolute);
                string mediaPath = (uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString());

                if (mediaPath.StartsWith("/media/", StringComparison.OrdinalIgnoreCase))
                {
                    var mediaItem = uMediaSyncHelper.mediaService.GetMediaByPath(mediaPath);
                    if (mediaItem != null)
                    {
                        mediaIds.Add(mediaItem.Id);
                    }
                }
            }

            return mediaIds;
        }
    }
}
