using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Escc.Umbraco.MediaSync
{
    /// <summary>
    /// Provides related media items where the property value contains comma-separated media item ids
    /// </summary>
    public class MediaPickerIdProvider : IRelatedMediaIdProvider
    {
        private readonly List<string> _propertyTypeAlises = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerIdProvider" /> class.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider.</param>
        public MediaPickerIdProvider(IMediaSyncConfigurationProvider configurationProvider)
        {
            _propertyTypeAlises.AddRange(configurationProvider.ReadPropertyEditorAliases("mediaPickerIdProvider"));
         }

        /// <summary>
        /// Determines whether this instance can read the type of property identified by its property editor alias
        /// </summary>
        /// <param name="propertyEditorAlias">The property editor alias.</param>
        /// <returns></returns>
        public bool CanReadPropertyType(string propertyEditorAlias)
        {
            return _propertyTypeAlises.Contains(propertyEditorAlias.ToUpperInvariant());
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
                try
                {
                    var savedMediaIds = property.Value.ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var savedMediaId in savedMediaIds)
                    {
                        mediaIds.Add(Int32.Parse(savedMediaId, CultureInfo.InvariantCulture));
                    }
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
            }

            return mediaIds;
        }
    }
}
