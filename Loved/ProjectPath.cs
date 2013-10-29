using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    static class ProjectPath {
        public static string Path { get; set; }

        public static string ExpandProjectPath(string path) {
            if (System.IO.Path.GetPathRoot(path) == "") {
                return System.IO.Path.Combine(Path, path);
            }

            return path;
        }
    }
}
