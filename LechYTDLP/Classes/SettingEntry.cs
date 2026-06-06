using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LechYTDLP.Classes
{
    [XmlRoot("Settings")]
    public class SettingsExportData
    {
        [XmlElement("Setting")]
        public List<SettingEntry> Entries { get; set; } = [];
    }

    public class SettingEntry
    {
        [XmlAttribute] public string Key { get; set; } = "";
        [XmlAttribute] public string Type { get; set; } = "";  // string|int|long|double|bool
        [XmlAttribute] public string Value { get; set; } = "";
    }
}
