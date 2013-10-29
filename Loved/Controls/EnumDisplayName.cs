using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumDisplayNameAttribute : Attribute {
        public EnumDisplayNameAttribute(string displayName) {
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }
}
