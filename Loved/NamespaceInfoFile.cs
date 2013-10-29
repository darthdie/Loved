using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Loved {
    class NamespaceInfoFile {
        public static List<NamespaceInfo> LoadLuaNamespace() {
            var list = new List<NamespaceInfo>();

            using (var rdr = XmlReader.Create("LoveNamespace.xml")) {
                while (rdr.Read()) {
                    switch (rdr.NodeType) {
                        case XmlNodeType.Element:
                            var element = ParseElement(rdr);
                            list.Add(element);
                            break;
                    }
                }
            }

            return list;
        }

        public static NamespaceInfo ParseElement(XmlReader rdr) {
            var item = new NamespaceInfo();
            var isEmptyElement = rdr.IsEmptyElement;

            if (rdr.Name == "Info") {
                for (int i = 0; i < rdr.AttributeCount; i++) {
                    rdr.MoveToAttribute(i);

                    if (rdr.Name == "Name") {
                        item.Name = rdr.Value;
                    }
                    else if (rdr.Name == "Description") {
                        item.Description = rdr.Value;
                    }
                    else if (rdr.Name == "Type") {
                        item.Type = (NamespaceInfoType)Enum.Parse(typeof(NamespaceInfoType), rdr.Value);
                    }
                    else if (rdr.Name == "Prototype") {
                        item.Prototype = rdr.Value;
                    }
                }
            }

            if (isEmptyElement) {
                return item;
            }

            while (rdr.Read()) {
                switch (rdr.NodeType) {
                    case XmlNodeType.Element:
                        item.Children.Add(ParseElement(rdr));
                        break;
                    case XmlNodeType.EndElement:
                        item.Children.Sort((x, y) => { return x.Name.CompareTo(y.Name); });
                        return item;
                }
            }

            throw new Exception();
        }
    }

    public class NamespaceInfo {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Prototype { get; set; }
        public NamespaceInfoType Type { get; set; }
        public List<NamespaceInfo> Children { get; private set; }

        public NamespaceInfo() {
            Children = new List<NamespaceInfo>();
        }

        public NamespaceInfo(string name, string desc, List<NamespaceInfo> children) {
            Name = name;
            Description = desc;
            Children = children ?? new List<NamespaceInfo>();
        }
    }

    public enum NamespaceInfoType {
        Table,
        Function,
        Enum,
        EnumValue,
        Value
    }
}
