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
        /// Determines whether this instance can read the type of property identified by its property editor alias
        /// </summary>
        /// <param name="propertyEditorAlias">The property editor alias.</param>
        /// <returns></returns>
        bool CanReadPropertyType(string propertyEditorAlias);

        /// <summary>
        /// Reads media ids from the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        IEnumerable<int> ReadProperty(Property property);
    }
}
