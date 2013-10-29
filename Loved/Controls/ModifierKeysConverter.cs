using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace Loved.Controls {
    [ValueConversion(typeof(ModifierKeys), typeof(string))]
    class ModifierKeysConverter : MarkupExtension, IValueConverter {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object rawValue, Type targetType, object parameter, CultureInfo culture) {
            /*var value = (ModifierKeys)rawValue;

            var values = 
            foreach (var v in Enum.GetValues(typeof(ModifierKeys))) {
            }*/

            return rawValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            //var val = (GridLength)value;
            //return val.Value;
            return Enum.Parse(typeof(ModifierKeys), value.ToString());
        }
    }
}
