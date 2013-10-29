using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    class Program {
        [STAThread]
        public static void Main(string[] args) {
            if (args != null && args.Length > 0) {
                throw new Exception();
            }
            else {
                LoadSettings();

                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

        private static void LoadSettings() {
            if (!System.IO.File.Exists(SettingsFile.Path)) {
                var dlg = new SettingsDialog(true);
                if (dlg.ShowDialog() == true) {
                    SettingsFile.Save(dlg.NewSettings, SettingsFile.Path);
                }
                else {
                    throw new Exception();
                }
            }

            Settings.Instance = SettingsFile.Load(SettingsFile.Path);
        }
    }
}
