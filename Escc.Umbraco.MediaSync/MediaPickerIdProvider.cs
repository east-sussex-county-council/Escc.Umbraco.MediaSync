using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using umbraco.presentation.actions;

namespace Escc.Umbraco.MediaSync
{
    /// <summary>
    /// Provides related media items where the property value contains comma-separated media item ids
    /// </summary>
    public class MediaPickerIdProvider : IRelatedMediaIdProvider
    {
        private readonly List<string> _propertyTypeAlises = new List<string>();
        private readonly IDataTypeService _dataTypeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerIdProvider" /> class.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider.</param>
        /// <param name="dataTypeService">The Umbraco data type service.</param>
        public MediaPickerIdProvider(IMediaSyncConfigurationProvider configurationProvider, IDataTypeService dataTypeService)
        {
            _propertyTypeAlises.AddRange(configurationProvider.ReadPropertyEditorAliases("mediaPickerIdProvider"));
            _dataTypeService = dataTypeService;
        }

        /// <summary>
        /// Determines whether this instance can read the type of property identified by its property editor alias
        /// </summary>
        /// <param name="propertyType">The property defined on the document type.</param>
        /// <returns></returns>
        public bool CanReadPropertyType(PropertyType propertyType)
        {
            var canRead = _propertyTypeAlises.Contains(propertyType.PropertyEditorAlias.ToUpperInvariant());
            if (canRead)
            {
                // A multi-node tree picker can be set to read media nodes or other types of node. Check for the media node
                // setting while still supporting other media picker property editor types which don't have the prevalue.
                foreach (var preValue in _dataTypeService.GetPreValuesByDataTypeId(propertyType.DataTypeDefinitionId))
                {
                    if (preValue == null) continue;
                    var sanitisedPreValue = Regex.Replace(preValue.ToUpperInvariant(), "[^A-Z:]", String.Empty);
                    if (sanitisedPreValue.StartsWith("TYPE:") && sanitisedPreValue != "TYPE:MEDIA")
                    {
                        canRead = false;
                    }
                }
            }
            return canRead;
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
