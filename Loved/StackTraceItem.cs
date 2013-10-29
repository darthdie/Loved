using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    public class StackTraceItem {
        public string File { get; set; }
        public int Line { get; set; }
        public string Description { get; set; }

        public static StackTraceItem ParseLine(string stringLine) {
            stringLine = stringLine.Trim();

            var file = "";
            var fileLine = 0;
            var desc = "";

            if (stringLine.StartsWith("[C]")) {
                //[C]: in function 'assert'
                file = "[C]";
                desc = stringLine.Substring(3);
            }
            else if (stringLine.StartsWith("[")) {
                var split = stringLine.Split(new[] { ':' }, 3);
                file = split[0];
                fileLine = Convert.ToInt32(split[1]);
                desc = split[2];
            }
            else {
                //zoetrope/core/class.lua:34: in function 'extend'
                var split = stringLine.Split(new[] { ':' }, 3);
                file = Path.GetFullPath(Path.Combine(ProjectPath.Path, split[0]));
                fileLine = Convert.ToInt32(split[1]);
                desc = split[2];
            }
            
            return new StackTraceItem {
                File = file,
                Line = fileLine,
                Description = desc
            };
        }
    }
}
