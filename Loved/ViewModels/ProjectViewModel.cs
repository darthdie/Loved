using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using ThomasJaworski.ComponentModel;

namespace Loved{
    public class ProjectViewModel : ProjectInfoItemViewModel, INotifyPropertyChanged, IProjectParentViewModel {
        private ChangeListener changeListener;
        private FileSystemWatcher fileWatcher;

        public override string Name { get; set; }
        public override int SortOrder { get { return -1; } }

        public override bool CanAcceptChildren {
            get { return true; }
        }

        public DirectoryInfo Directory {
            get { return Data as DirectoryInfo; }
        }

        private ProjectInfoItemViewModel _selectedItem;
        public ProjectInfoItemViewModel SelectedItem {
            get { return _selectedItem; }
            set {
                if (_selectedItem != value) {
                    _selectedItem = value;
                    UpdateSelectedItems(value);
                    NotifyPropertyChanged("SelectedItem");
                }
            }
        }

        public bool IsLoaded { get { return Children.Count > 0; } }

        private void UpdateSelectedItems(ProjectInfoItemViewModel newItem) {
            foreach (var item in Children.Where(i => i.IsSelected)) {
                if (item != newItem) {
                    item.IsSelected = false;
                }
            }

            SelectedItem = newItem;
        }

        public ProjectViewModel(DirectoryInfo data) : base(null, data) {
            fileWatcher = new FileSystemWatcher(data.FullName) {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            fileWatcher.Changed += OnFileWatcherChanged;
            fileWatcher.Created += OnFileWatcherChanged;
            fileWatcher.Deleted += OnFileWatcherChanged;
            fileWatcher.Renamed += OnFileWatcherRenamed;

            Name = data.Name;
            ProjectPath.Path = data.FullName;
            IsExpanded = true;

            Children = new ObservableCollection<ProjectInfoItemViewModel>();
            var cvs = CollectionViewSource.GetDefaultView(Children);
            cvs.SortDescriptions.Add(new SortDescription("SortOrder", ListSortDirection.Ascending));
            cvs.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            foreach (var directory in Directory.GetDirectories()) {
                Children.Add(new ProjectDirectoryInfoViewModel(this, directory));
            }

            foreach (var file in Directory.GetFiles()) {
                AddChild(file);
            }

            changeListener = ChangeListener.Create(Children);
            changeListener.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                var vm = sender as ProjectInfoItemViewModel;
                if (vm != null) {
                    UpdateSelectedItems(vm);
                }
            };
        }

        private void OnFileWatcherChanged(object sender, FileSystemEventArgs e) {
            switch (e.ChangeType) {
                case WatcherChangeTypes.Changed:
                    break;
                case WatcherChangeTypes.Created:
                    HandleFileCreation(e.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    HandleFileDeletion(e.FullPath);
                    break;
            }
        }

        private void HandleFileDeletion(string filePath) {
            var code = GetFile(filePath);
            if (code == null) {
                return;
            }

            if (code.IsOpen) {
                if (MessageBox.Show(string.Format("The file '{0}' was deleted, would you like to keep it open?", System.IO.Path.GetFileName(filePath)),"Keep it open?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    code.IsModified = true;
                }
                else {
                    code.Parent.Children.Remove(code);
                    code.Parent = null;
                }
            }
            else {
                MessageBox.Show(string.Format("The file '{0}' was deleted externally", System.IO.Path.GetFileName(filePath)));
                code.Parent.Children.Remove(code);
                code.Parent = null;
            }
        }

        private void HandleFileCreation(string filePath) {
            var dir = GetFile(System.IO.Path.GetDirectoryName(filePath)) as IProjectParentViewModel;
            if (dir != null) {
                dir.AddChild(new FileInfo(filePath));
            }
        }

        private void OnFileWatcherRenamed(object sender, RenamedEventArgs e) {
        }

        public void AddChild(FileSystemInfo info) {
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess()) {
                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    AddChild(info);
                });

                return;
            }

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

        public ProjectInfoItemViewModel GetFile(string searchPath) {
            if (searchPath == Path) {
                return this;
            }

            foreach (var child in Children) {
                if (child.Path == searchPath) {
                    return child;
                }

                if (child is IProjectParentViewModel) {
                    var file = GetFile(searchPath, (IProjectParentViewModel)child);
                    if (file != null) {
                        return file;
                    }
                }
            }

            return null;
        }

        private ProjectInfoItemViewModel GetFile(string searchPath, IProjectParentViewModel root) {
            foreach (var child in root.Children) {
                if (child.Path == searchPath) {
                    return child;
                }

                if (child is IProjectParentViewModel) {
                    var file = GetFile(searchPath, (IProjectParentViewModel)child);
                    if (file != null) {
                        return file;
                    }
                }
            }

            return null;
        }
    }
}
