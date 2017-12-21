using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Escc.Umbraco.MediaSync
{
    /// <summary>
    /// Gets the ids of media items linked within a related links property
    /// </summary>
    public class RelatedLinksMediaIdProvider : IRelatedMediaIdProvider
    {
        private readonly List<string> _propertyTypeAlises = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlMediaIdProvider" /> class.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider.</param>
        public RelatedLinksMediaIdProvider(IMediaSyncConfigurationProvider configurationProvider)
        {
            _propertyTypeAlises.AddRange(configurationProvider.ReadPropertyEditorAliases("relatedLinksIdProvider"));
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

            if (!String.IsNullOrEmpty(property?.Value?.ToString()))
            {
                var relatedLinks = JsonConvert.DeserializeObject<JArray>(property.Value.ToString());
                foreach (var relatedLink in relatedLinks)
                {
                    try
                    {
                        var uri = new Uri(relatedLink.Value<string>("link"), UriKind.RelativeOrAbsolute);
                        string mediaPath = (uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString());
                        if (!mediaPath.StartsWith("/media/", StringComparison.OrdinalIgnoreCase)) continue;

                        var mediaItem = uMediaSyncHelper.mediaService.GetMediaByPath(mediaPath);
                        if (mediaItem != null)
                        {
                            mediaIds.Add(mediaItem.Id);
                        }
                    }
                    catch (UriFormatException)
                    {
                        // if someone entered an invalid URL in a related links field, just ignore it and move on
                    }
                }
                
            }

            return mediaIds;
        }
    }
}
