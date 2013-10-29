using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;

namespace Loved {
    public abstract class ProjectInfoItemViewModel : INotifyPropertyChanged, IProjectItemViewModel {
        public virtual string Name { get; set; }
        public abstract int SortOrder { get; }
        public abstract bool CanAcceptChildren { get; }

        private bool _isModified = false;
        public bool IsModified {
            get { return _isModified; }
            set {
                if (_isModified != value) {
                    _isModified = value;
                    NotifyPropertyChanged("IsModified");
                }
            }
        }
        
        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set { 
                if (_isSelected != value) { 
                    _isSelected = value; 
                    NotifyPropertyChanged("IsSelected");
                } 
            }
        }

        private bool _isExpanded;
        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded != value) {
                    _isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        private bool _isOpen = false;
        public bool IsOpen {
            get { return _isOpen; }
            set {
                if (_isOpen != value) {
                    _isOpen = value;
                    NotifyPropertyChanged("IsOpen");
                }
            }
        }

        private bool _isCurrent = false;
        public bool IsCurrent {
            get { return _isCurrent; }
            set {
                if (_isCurrent != value) {
                    _isCurrent = value;
                    NotifyPropertyChanged("IsCurrent");
                }
            }
        }

        [ChangeListenerIgnore]
        public IProjectParentViewModel Parent { get; set; }

        public bool Edit { get; set; }
        public ContextMenu Menu { get; private set; }

        public string Path { get { return Data.FullName; } }

        public FileSystemInfo Data { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string prop) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public ProjectInfoItemViewModel(IProjectParentViewModel parent, FileSystemInfo data) {
            Menu = new ContextMenu();

            Data = data;
            Parent = parent;
        }

        public void Delete() {
            if (IsSelected) {
                IsSelected = false;
            }

            if (System.IO.Path.GetExtension(Path) == "") {
                System.IO.Directory.Delete(Path, true);
            }
            else {
                System.IO.File.Delete(Path);
            }
            
            Parent.Children.Remove(this);
        }
    }

    public abstract class ProjectInfoFileItemViewModel : ProjectInfoItemViewModel {
        public override int SortOrder {
            get { return 1; }
        }

        public override bool CanAcceptChildren {
            get { return false; }
        }

        public override string Name {
            get { return Data.Name; }
            set {
                if (Data.Name != value && !string.IsNullOrEmpty(value)) {
                    if (!string.IsNullOrEmpty(Data.Name)) {
                        var newPath = System.IO.Path.Combine(Parent.Path, value);

                        if (System.IO.File.Exists(newPath)) {
                            return;
                        }

                        try {
                            File.MoveTo(newPath);
                        }
                        catch (System.IO.IOException ex) {
                            MessageBox.Show(string.Format("Error renaming file: {0}", ex.Message));
                            return;
                        }
                    }

                    NotifyPropertyChanged("Name");
                    NotifyPropertyChanged("Path");
                }
            }
        }

        public FileInfo File {
            get { return Data as FileInfo; }
        }

        public ProjectInfoFileItemViewModel(IProjectParentViewModel parent, FileInfo data) : base(parent, data) {
            var openItem = new MenuItem {
                Header = "Open"
            };

            openItem.Click += delegate {
                IsSelected = true;
                CustomCommands.OpenFileCommand.Execute(this, null);
            };

            Menu.Items.Add(openItem);

            var openWithItem = new MenuItem {
                Header = "Open With..."
            };

            openWithItem.Click += delegate {
                System.Diagnostics.Process.Start("rundll32.exe", string.Format("{0}\\{1},OpenAs_RunDLL {2}",Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll", Path));
            };

            Menu.Items.Add(openWithItem);

            var openFolderItem = new MenuItem {
                Header = "Open Containing Folder"
            };

            openFolderItem.Click += delegate {
                var folderPath = System.IO.Path.GetDirectoryName(Path);
                System.Diagnostics.Process.Start("explorer.exe", string.Format("/Select, \"{0}\"", Path));
            };

            Menu.Items.Add(openFolderItem);

            Menu.Items.Add(new Separator());

            var filePropertiesItem = new MenuItem {
                Header = "File Properties"
            };

            filePropertiesItem.Click += delegate {
                ShellExecute.ShowFileProperties(Path);
            };

            Menu.Items.Add(filePropertiesItem);
        }

        public void UpdatePath(string path) {
            Data = new FileInfo(path);
            NotifyPropertyChanged("Name");
            NotifyPropertyChanged("Path");
        }
    }

    public interface IProjectItemViewModel {
        string Name { get; set; }
        string Path { get; }
        bool Edit { get; set; }
    }
}
