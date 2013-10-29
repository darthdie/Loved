using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Win32;

namespace Loved {
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window {
        private bool canExit;

        public bool IsNotFirstRun { get; set; }

        public Settings NewSettings {
            get { return (Settings)GetValue(NewSettingsProperty); }
            set { SetValue(NewSettingsProperty, value); }
        }

        public static readonly DependencyProperty NewSettingsProperty =
            DependencyProperty.Register("NewSettings", typeof(Settings), typeof(SettingsDialog));

        public ObservableCollection<ShortcutSetting> Shortcuts { get; set; }

        public ShortcutSetting SelectedShortcut {
            get { return (ShortcutSetting)GetValue(SelectedShortcutProperty); }
            set { SetValue(SelectedShortcutProperty, value); }
        }

        public static readonly DependencyProperty SelectedShortcutProperty =
            DependencyProperty.Register("SelectedShortcut", typeof(ShortcutSetting), typeof(SettingsDialog));

        public SettingsDialog(bool isFirstRun = false, Settings currentSettings = null) {
            canExit = !isFirstRun;
            IsNotFirstRun = !isFirstRun;
            NewSettings = currentSettings ?? new Settings();

            Shortcuts = new ObservableCollection<ShortcutSetting> {
                new ShortcutSetting { Name = "Run", Key = NewSettings.RunKey, ModifierKeys = NewSettings.RunModifierKey},
                new ShortcutSetting { Name = "Compile", Key = NewSettings.CompileKey, ModifierKeys = NewSettings.CompileModifierKey},
                new ShortcutSetting { Name = "Goto Line", Key = NewSettings.GotoLineKey, ModifierKeys = NewSettings.GotoLineModifierKey },
                new ShortcutSetting { Name = "Search Files", Key = NewSettings.SearchFilesKey, ModifierKeys = NewSettings.SearchFilesModifierKey },
                new ShortcutSetting { Name = "Replace Files", Key = NewSettings.ReplaceFilesKey, ModifierKeys = NewSettings.ReplaceFilesModifierKey }
            };

            SelectedShortcut = Shortcuts[0];

            InitializeComponent();
        }

        private void OnOKButtonClicked(object sender, RoutedEventArgs e) {
            if (!ValidSettings()) {
                MessageBox.Show("Please make sure the settings are valid. (They're not)");
                return;
            }

            var runShortcut = Shortcuts.First(c => c.Name == "Run");
            NewSettings.RunKey = runShortcut.Key;
            NewSettings.RunModifierKey = runShortcut.ModifierKeys;

            var compileShortcut = Shortcuts.First(c => c.Name == "Compile");
            NewSettings.CompileKey = compileShortcut.Key;
            NewSettings.CompileModifierKey = compileShortcut.ModifierKeys;

            var gotolineCommand = Shortcuts.First(c => c.Name == "Goto Line");
            NewSettings.GotoLineKey = gotolineCommand.Key;
            NewSettings.GotoLineModifierKey = gotolineCommand.ModifierKeys;

            var searchFilesCommand = Shortcuts.First(c => c.Name == "Search Files");
            NewSettings.SearchFilesKey = searchFilesCommand.Key;
            NewSettings.SearchFilesModifierKey = searchFilesCommand.ModifierKeys;

            Settings.Instance = NewSettings;
            canExit = true;
            DialogResult = true;
        }

        private bool ValidSettings() {
            if (string.IsNullOrEmpty(NewSettings.LoveExecutablePath) || !System.IO.File.Exists(NewSettings.LoveExecutablePath)) {
                return false;
            }

            return true;
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void OnDialogClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = !canExit;
        }

        private void OnLoveExecutableBrowseClicked(object sender, RoutedEventArgs e) {
            var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog {
                Filter = "love.exe|love.exe"
            };

            if (dlg.ShowDialog() == true) {
                NewSettings.LoveExecutablePath = dlg.FileName;
            }
        }
    }

    public class ShortcutSetting {
        public string Name { get; set; }
        public ModifierKeys ModifierKeys { get; set; }
        public Key Key { get; set; }
    }
}
