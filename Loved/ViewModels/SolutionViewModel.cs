using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GongSolutions.Wpf.DragDrop;

namespace Loved {
    public class SolutionViewModel : IDropTarget {
        public ObservableCollection<ProjectViewModel> Children { get; set; }

        public SolutionViewModel(ProjectViewModel project) {
            Children = new ObservableCollection<ProjectViewModel> { project };
        }

        public void DragOver(IDropInfo dropInfo) {
            var sourceItem = dropInfo.Data as ProjectInfoItemViewModel;
            var targetItem = dropInfo.TargetItem as IProjectParentViewModel;

            if (sourceItem != null && targetItem != null) {
                if (targetItem.Children.Contains(sourceItem)) {
                    return;
                }
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo) {
            var sourceItem = dropInfo.Data as ProjectInfoItemViewModel;
            var targetItem = dropInfo.TargetItem as IProjectParentViewModel;
            if (sourceItem != null && targetItem != null) {
                try {

                    var newDirectory = "";
                    if (targetItem is ProjectDirectoryInfoViewModel) {
                        newDirectory = ((ProjectDirectoryInfoViewModel)targetItem).Directory.FullName;
                    }
                    else {
                        newDirectory = ((ProjectViewModel)targetItem).Directory.FullName;
                    }

                    Directory.Move(sourceItem.Path, System.IO.Path.Combine(newDirectory, sourceItem.Name));
                }
                catch (IOException ex) {
                    MessageBox.Show("Error moving file:\r\n" + ex.Message);
                    return;
                }

                sourceItem.Parent.Children.Remove(sourceItem);
                targetItem.Children.Add(sourceItem);
                sourceItem.Parent = targetItem;
            }
        }
    }
}
