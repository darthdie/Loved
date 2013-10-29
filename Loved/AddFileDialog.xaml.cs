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
    /// Interaction logic for AddFileDialog.xaml
    /// </summary>
    public partial class AddFileDialog : Window {
        private string rootPath;

        public string FileName {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(AddFileDialog));

        public string PromptText {
            get { return (string)GetValue(PromptTextProperty); }
            set { SetValue(PromptTextProperty, value); }
        }

        public static readonly DependencyProperty PromptTextProperty =
            DependencyProperty.Register("PromptText", typeof(string), typeof(MainWindow), new PropertyMetadata("ERgdlely blargedlaga"));

        public AddFileDialog(string basePath, string promptText) {
            rootPath = basePath;
            PromptText = promptText;

            InitializeComponent();

            this.Loaded += delegate {
                FileNameTextbox.Focus();
            };
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void OnAddButtonClicked(object sender, RoutedEventArgs e) {
            if (ValidateFilename()) {
                DialogResult = true;
            }
        }

        private bool ValidateFilename() {
            var path = System.IO.Path.Combine(rootPath, FileName);
            if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
                MessageBox.Show("The item already exists...so pick a different name or something.");
                return false;
            }

            return true;
        }
    }
}
