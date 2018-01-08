using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.Umbraco.MediaSync.Models
{
    /// <summary>
    /// A deserialised value stored in an Umbraco Grid editor
    /// </summary>
    public class GridValue
    {
        public string name { get; set; }
        public Section[] sections { get; set; }
    }

    public class Section
    {
        public int grid { get; set; }
        public Row[] rows { get; set; }
    }

    public class Row
    {
        public string name { get; set; }
        public Area[] areas { get; set; }
        public string label { get; set; }
        public bool hasConfig { get; set; }
        public string id { get; set; }
        public bool hasActiveChild { get; set; }
        public bool active { get; set; }
    }

    public class Area
    {
        public int grid { get; set; }
        public bool allowAll { get; set; }
        public string[] allowed { get; set; }
        public bool hasConfig { get; set; }
        public Control[] controls { get; set; }
        public bool hasActiveChild { get; set; }
        public bool active { get; set; }
    }

    public class Control
    {
        public string value { get; set; }
        public Editor editor { get; set; }
        public bool active { get; set; }
    }

    public class Editor
    {
        public string alias { get; set; }
    }
}