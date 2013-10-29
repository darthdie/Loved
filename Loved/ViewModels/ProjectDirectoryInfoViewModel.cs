using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using IO = System.IO;

namespace Loved {
    public class ProjectDirectoryInfoViewModel : ProjectInfoItemViewModel, IProjectParentViewModel {
        public override int SortOrder {
            get { return 0; }
        }

        public override bool CanAcceptChildren {
            get { return true; }
        }

        public DirectoryInfo Directory {
            get { return Data as DirectoryInfo; }
        }

        public override string Name {
            get { return Data.Name; }
            set {
                if (Data.Name != value && !string.IsNullOrEmpty(value)) {
                    if (!string.IsNullOrEmpty(Data.Name)) {
                        var newPath = System.IO.Path.Combine(Parent.Path, value);

                        if (System.IO.Directory.Exists(newPath)) {
                            return;
                        }

                        try {
                            Directory.MoveTo(newPath);
                        }
                        catch (System.IO.IOException ex) {
                            MessageBox.Show(string.Format("Error renaming directory: {0}", ex.Message));
                            return;
                        }
                    }

                    NotifyPropertyChanged("Name");
                    NotifyPropertyChanged("Path");
                }
            }
        }

        public ProjectDirectoryInfoViewModel(IProjectParentViewModel parent, DirectoryInfo data) : base(parent, data) {
            Children = new ObservableCollection<ProjectInfoItemViewModel>();
            var cvs = CollectionViewSource.GetDefaultView(Children);
            cvs.SortDescriptions.Add(new SortDescription("SortOrder", ListSortDirection.Ascending));
            cvs.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            
            CreateMenu();
            LoadChildren();
        }

        private void CreateMenu() {
            var openItem = new MenuItem {
                Header = "Open in Explorer"
            };

            openItem.Click += delegate {
                System.Diagnostics.Process.Start("explorer.exe", string.Format("\"{0}\"", Path));
            };

            Menu.Items.Add(openItem);

            var addFileItem = new MenuItem {
                Header = "Add File"
            };

            addFileItem.Click += delegate {
                CustomCommands.AddFileCommand.Execute(this, null);
            };

            Menu.Items.Add(addFileItem);
        }

        private void LoadChildren() {
            foreach (var directory in Directory.GetDirectories()) {
                Children.Add(new ProjectDirectoryInfoViewModel(this, directory));
            }

            foreach (var file in Directory.GetFiles()) {
                AddChild(file);
            }
        }

        public void AddChild(FileSystemInfo info) {
            if (info.IsDirectory()) {
                Children.Add(new ProjectDirectoryInfoViewModel(this, (DirectoryInfo)info));
            }
            else {
                var file = info as FileInfo;
                switch (FileTypes.GetFileTypeFor(file.FullName)) {
                    case FileType.Code:
                        Children.Add(new ProjectCodeFileInfoViewModel(this, file));
                        break;
                    case FileType.Audio:
                        Children.Add(new ProjectAudioFileInfoViewModel(this, file));
                        break;
                    case FileType.Image:
                        Children.Add(new ProjectImageFileInfoViewModel(this, file));
                        break;
                    case FileType.Text:
                        Children.Add(new ProjectTextFileInfoViewModel(this, file));
                        break;
                }
            }
        }

        public ObservableCollection<ProjectInfoItemViewModel> Children {
            get;
            private set;
        }
    }
}
