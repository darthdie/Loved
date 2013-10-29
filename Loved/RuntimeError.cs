using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    public class RuntimeError {
        public string File { get; set; }
        public string Path { get; set; }
        public int Line { get; set; }
        public string Description { get; set; }
        public List<StackTraceItem> StackTrace { get; set; }

        public RuntimeError() {
            StackTrace = new List<StackTraceItem>();
        }
    }
}
