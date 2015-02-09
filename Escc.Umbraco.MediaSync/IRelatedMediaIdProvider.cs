using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Escc.Umbraco.MediaSync
{
    /// <summary>
    /// A provider which identifies media items related to a content node
    /// </summary>
    interface IRelatedMediaIdProvider
    {
        /// <summary>
        /// Determines whether this instance can read the type of property
        /// </summary>
        /// <param name="propertyType">The property defined on the document type.</param>
        /// <returns></returns>
        bool CanReadPropertyType(PropertyType propertyType);

        /// <summary>
        /// Reads media ids from the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        IEnumerable<int> ReadProperty(Property property);
    }
}
