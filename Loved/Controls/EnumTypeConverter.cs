using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    public class EnumTypeConverter : EnumConverter {
        public EnumTypeConverter(Type enumType) : base(enumType) { }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string) && value != null) {
                var enumType = value.GetType();
                if (enumType.IsEnum)
                    return GetDisplayName(value);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
        private string GetDisplayName(object enumValue) {
            var displayNameAttribute = EnumType.GetField(enumValue.ToString())
                                                                 .GetCustomAttributes(typeof(EnumDisplayNameAttribute), false)
                                                                 .FirstOrDefault() as EnumDisplayNameAttribute;
            if (displayNameAttribute != null)
                return displayNameAttribute.DisplayName;

            return Enum.GetName(EnumType, enumValue);
        }
    }
}
