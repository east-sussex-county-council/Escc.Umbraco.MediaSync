using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Inception.Attributes;
using Umbraco.Inception.BL;

namespace Escc.Umbraco.MediaSync.DataTypes
{
    [UmbracoDataType(DataTypeName, PropertyEditor, typeof(MediaContentUsageDataType), DataTypeDatabaseType.Ntext)]
    public class MediaContentUsageDataType : IPreValueProvider
    {
        public const string DataTypeName = "Media Content Usage";
        public const string PropertyEditor = "Escc.MediaContentUsage";
        public IDictionary<string, PreValue> PreValues { get {return new Dictionary<string, PreValue>();} }
    }
}
