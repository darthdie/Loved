using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    public static class FileSystemInfoExtensions {
        public static bool IsDirectory(this FileSystemInfo info) {
            return (info.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}
