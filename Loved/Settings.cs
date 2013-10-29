using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    [Serializable()]
    public class Settings : INotifyPropertyChanged {
        public static Settings Instance { get; set; }

        private string _loveExecutablePath = @"C:\Program Files (x86)\LOVE\love.exe";
        public string LoveExecutablePath { 
            get { return _loveExecutablePath; }
            set {
                if (_loveExecutablePath != value) {
                    _loveExecutablePath = value;
                    NotifyPropertyChanged("LoveExecutablePath");
                }
            }
        }

        private double _horizontalSplitterPosition = 250d;
        public double HorizontalSplitterPosition {
            get { return _horizontalSplitterPosition; }
            set {
                if (_horizontalSplitterPosition != value) {
                    _horizontalSplitterPosition = value;
                    NotifyPropertyChanged("HorizontalSplitterPosition");
                }
            }
        }

        private double _verticalSplitterPosition = 200d;
        public double VerticalSplitterPosition {
            get { return _verticalSplitterPosition; }
            set {
                if (_verticalSplitterPosition != value) {
                    _verticalSplitterPosition = value;
                    NotifyPropertyChanged("VerticalSplitterPosition");
                }
            }
        }

        private System.Windows.Input.Key _runKey = System.Windows.Input.Key.F5;
        public System.Windows.Input.Key RunKey {
            get { return _runKey; }
            set {
                if (_runKey != value) {
                    _runKey = value;
                    NotifyPropertyChanged("RunKey");
                }
            }
        }

        private System.Windows.Input.ModifierKeys _runModifierKey = System.Windows.Input.ModifierKeys.None;
        public System.Windows.Input.ModifierKeys RunModifierKey {
            get { return _runModifierKey; }
            set {
                if (_runModifierKey != value) {
                    _runModifierKey = value;
                    NotifyPropertyChanged("RunModifierKey");
                }
            }
        }

        private System.Windows.Input.Key _compileKey = System.Windows.Input.Key.F6;
        public System.Windows.Input.Key CompileKey {
            get { return _compileKey; }
            set {
                if (_compileKey != value) {
                    _compileKey = value;
                    NotifyPropertyChanged("CompileKey");
                }
            }
        }

        private System.Windows.Input.ModifierKeys _compileModifierKey = System.Windows.Input.ModifierKeys.None;
        public System.Windows.Input.ModifierKeys CompileModifierKey {
            get { return _compileModifierKey; }
            set {
                if (_compileModifierKey != value) {
                    _compileModifierKey = value;
                    NotifyPropertyChanged("CompileModifierKey");
                }
            }
        }

        private System.Windows.Input.Key _searchFilesKey = System.Windows.Input.Key.F;
        public System.Windows.Input.Key SearchFilesKey {
            get { return _searchFilesKey; }
            set {
                if (_searchFilesKey != value) {
                    _searchFilesKey = value;
                    NotifyPropertyChanged("SearchFilesKey");
                }
            }
        }

        private System.Windows.Input.ModifierKeys _searchFilesModifierKey = System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift;
        public System.Windows.Input.ModifierKeys SearchFilesModifierKey {
            get { return _searchFilesModifierKey; }
            set {
                if (_searchFilesModifierKey != value) {
                    _searchFilesModifierKey = value;
                    NotifyPropertyChanged("SearchFilesModifierKey");
                }
            }
        }

        private System.Windows.Input.Key _replaceFilesKey = System.Windows.Input.Key.R;
        public System.Windows.Input.Key ReplaceFilesKey {
            get { return _replaceFilesKey; }
            set {
                if (_replaceFilesKey != value) {
                    _replaceFilesKey = value;
                    NotifyPropertyChanged("ReplaceFilesKey");
                }
            }
        }

        private System.Windows.Input.ModifierKeys _replaceFilesModifierKey = System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift;
        public System.Windows.Input.ModifierKeys ReplaceFilesModifierKey {
            get { return _replaceFilesModifierKey; }
            set {
                if (_replaceFilesModifierKey != value) {
                    _replaceFilesModifierKey = value;
                    NotifyPropertyChanged("ReplaceFilesModifierKey");
                }
            }
        }

        private System.Windows.Input.Key _gotoLineKey = System.Windows.Input.Key.G;
        public System.Windows.Input.Key GotoLineKey {
            get { return _gotoLineKey; }
            set {
                if (_gotoLineKey != value) {
                    _gotoLineKey = value;
                    NotifyPropertyChanged("GotoLineKey");
                }
            }
        }

        private System.Windows.Input.ModifierKeys _gotoLineModifierKey = System.Windows.Input.ModifierKeys.Control;
        public System.Windows.Input.ModifierKeys GotoLineModifierKey {
            get { return _gotoLineModifierKey; }
            set {
                if (_gotoLineModifierKey != value) {
                    _gotoLineModifierKey = value;
                    NotifyPropertyChanged("GotoLineModifierKey");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string prop) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
