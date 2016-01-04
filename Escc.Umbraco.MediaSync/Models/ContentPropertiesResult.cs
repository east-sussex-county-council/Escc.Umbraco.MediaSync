namespace Escc.Umbraco.MediaSync.Models
{
    public class ContentPropertiesResult
    {
        public int contentNodeId { get; set; }
        public int propertyTypeId { get; set; }
        public string nodeName { get; set; }
        public string propertyName { get; set; }
        public string dataCombined { get; set; }
    }
}