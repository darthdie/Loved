using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    public class CompileError {
        public static CompileError FromLuaException(NLua.Exceptions.LuaScriptException ex, string filePath) {
            try {
                var error = new CompileError {
                    File = System.IO.Path.GetFileName(filePath),
                    Path = filePath
                };

                string[] info;
                if (!ex.Message.StartsWith("...")) {
                    info = ex.Message.Split(new[] { ':' }, 4);
                    error.Line = Convert.ToInt32(info[2]);
                    error.Description = info[3].Trim();
                }
                else {
                    info = ex.Message.Split(new[] { ':' }, 3);
                    error.Line = Convert.ToInt32(info[1]);
                    error.Description = info[2].Trim();
                }

                return error;
            }
            catch (Exception) {
                throw;
            }
        }

        public string File { get; set; }
        public string Path { get; set; }
        public int Line { get; set; }
        public string Description { get; set; }
    }
}
