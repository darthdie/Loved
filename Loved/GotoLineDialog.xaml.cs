using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Loved {
    /// <summary>
    /// Interaction logic for GotoLineDialog.xaml
    /// </summary>
    public partial class GotoLineDialog : Window {
        public string LineString {
            get { return (string)GetValue(LineStringProperty); }
            set { SetValue(LineStringProperty, value); }
        }

        public static readonly DependencyProperty LineStringProperty =
            DependencyProperty.Register("LineString", typeof(string), typeof(GotoLineDialog));

        public int LineNumber {
            get { return Convert.ToInt32(LineString); }
        }

        public GotoLineDialog() {
            InitializeComponent();
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void OnOKButtonClicked(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }
    }
}
