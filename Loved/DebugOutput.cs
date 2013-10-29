using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    class DebugOutput {
        public static DebugOutput CreateFrom(string debugOutput) {
            var output = new DebugOutput();

            var lines = new Queue<string>(debugOutput.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            while (lines.Count > 0) {
                var line = lines.Dequeue();

                if (line.StartsWith("Error:")) {
                    //Error: zoetrope/core/class.lua:34: must extend a table, received a number
                    var lineInfo = line.Split(new[] { ':' });
                    var error = new RuntimeError {
                        Path = ProjectPath.ExpandProjectPath(lineInfo[1].Trim()),
                        File = System.IO.Path.GetFileName(lineInfo[1].Trim()),
                        Line = Convert.ToInt32(lineInfo[2]),
                        Description = lineInfo[3].Trim()
                    };

                    if (lines.Peek() == "stack traceback:") {
                        lines.Dequeue();

                        while (lines.Count > 0) {
                            error.StackTrace.Add(StackTraceItem.ParseLine(lines.Dequeue()));
                           // error.StackTrace += lines.Dequeue() + "\r\n";
                        }
                    }

                    output.Errors.Add(error);
                }
            }

            return output;
        }

        public List<RuntimeError> Errors { get; set; }

        public DebugOutput() {
            Errors = new List<RuntimeError>();
        }
    }
}
