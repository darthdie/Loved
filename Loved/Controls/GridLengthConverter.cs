using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Loved {
    [ValueConversion(typeof(double), typeof(GridLength))]
    public class GridLengthConverter : MarkupExtension, IValueConverter {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double val = (double)value;
            var gridLength = new GridLength(val);
            return gridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var val = (GridLength)value;
            return val.Value;
        }
    }
}
