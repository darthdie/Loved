using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    static class FileTypes {
        public static FileType GetFileTypeFor(string file) {
            string[] ImageTypes = new string[] { ".bmp", ".tga", ".png", ".jpg", ".gif" };
            string[] AudioTypes = new string[] { ".mp3", ".ogg", ".wav", ".mod", ".s3m", ".xm", ".it", ".669", ".amf", ".ams", ".dbm", ".dmf", ".dsm", ".far", ".mdl", ".med", ".mtm", ".okt", ".ptm", ".stm", ".ult", ".umx", ".mt2", ".psm", ".mid", ".abc" };

            var ext = System.IO.Path.GetExtension(file).ToLower();

            if (ext == ".lua") {
                return FileType.Code;
            }

            if (ImageTypes.Contains(ext)) {
                return FileType.Image;
            }

            if (AudioTypes.Contains(ext)) {
                return FileType.Audio;
            }

            if (ext == "") {
                return FileType.Folder;
            }

            return FileType.Text;
        }
    }

    public enum FileType {
        Text,
        Code,
        Image,
        Audio,
        Folder
    }
}
