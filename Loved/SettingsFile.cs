using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Loved {
    class SettingsFile {
        public const string Path = "Settings.xml";

        public static void Save(Settings settings, string path) {
            using (var stream = new StreamWriter(path)) {
                var wtr = new XmlSerializer(typeof(Settings));
                wtr.Serialize(stream, settings);
            }
        }

        public static Settings Load(string path) {
            using (var stream = new StreamReader(path)) {
                var wtr = new XmlSerializer(typeof(Settings));
                return wtr.Deserialize(stream) as Settings;
            }
        }
    }
}
