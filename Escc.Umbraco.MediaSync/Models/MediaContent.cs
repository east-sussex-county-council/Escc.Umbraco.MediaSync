using System.Collections.Generic;

namespace Escc.Umbraco.MediaSync.Models
{
    public class MediaContent
    {
        public int MediaNodeId { get; set; }
        public bool IsUsed { get; set; }
        public List<ContentNode> Content { get; set; }
    }
}