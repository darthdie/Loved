using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    public interface IProjectParentViewModel : IProjectItemViewModel {
        ObservableCollection<ProjectInfoItemViewModel> Children { get; }
        void AddChild(FileSystemInfo info);
    }
}
