using System.Collections.Generic;
using Escc.EastSussexGovUK.UmbracoDocumentTypes.DataTypes;
using Umbraco.Core.Models;

namespace Escc.Umbraco.MediaSync.DataTypes
{
    public static class MediaContentUsageDataType
    {
        public const string DataTypeName = "Media Content Usage";
        public const string PropertyEditor = "Escc.MediaContentUsage";

        public static void CreateDataType()
        {
            IDictionary<string, PreValue> preValues = new Dictionary<string, PreValue>();

            
            UmbracoDataTypeService.InsertDataType(DataTypeName, "Escc.MediaContentUsage", DataTypeDatabaseType.Ntext, preValues);
        }

    }
}
