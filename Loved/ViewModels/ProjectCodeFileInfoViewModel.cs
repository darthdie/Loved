using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = System.IO;

namespace Loved {
    public class ProjectCodeFileInfoViewModel : ProjectInfoFileItemViewModel {
        public ProjectCodeFileInfoViewModel(IProjectParentViewModel parent, FileInfo data) : base(parent, data) {
        }
    }
}
