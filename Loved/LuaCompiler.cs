using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loved {
    class LuaCompiler {
        private ProjectViewModel project;
        private BackgroundWorker compileWorker;

        public delegate void CompileFinishedEventHandler(object sender, CompileFinishedEventArgs e);
        public event CompileFinishedEventHandler OnCompileFinished;

        public delegate void CompileProgressEventHandler(object sender, CompileProgressEventArgs e);
        public event CompileProgressEventHandler OnProgressChanged;

        public LuaCompiler(ProjectViewModel projectModel) {
            compileWorker = new BackgroundWorker();
            compileWorker.DoWork += OnCompileWorkerDoWork;
            compileWorker.RunWorkerCompleted += OnCompileWorkerRunWorkerCompleted;
            
            project = projectModel;
        }

        void OnCompileWorkerDoWork(object sender, DoWorkEventArgs e) {
            NotifyProgress("Starting build...");

            var errors = new List<CompileError>();
            var lua = new NLua.Lua();
            var files = GetCodeFiles();

            foreach (var file in files) {
                try {
                    lua.LoadFile(file.Path);

                    NotifyProgress(string.Format("'{0}' is ok...", file.Name));
                }
                catch (NLua.Exceptions.LuaScriptException ex) {
                    var error = CompileError.FromLuaException(ex, file.Path);
                    errors.Add(error);
                    NotifyProgress(string.Format("{0}({1}): {2}", error.File, error.Line, error.Description));
                }
            }

            if (errors.Count > 0) {
                NotifyProgress("Build failed.");
            }
            else {
                NotifyProgress("Build succeeded.");
            }

            e.Result = new CompileFinishedEventArgs {
                Success = errors.Count == 0,
                Errors = errors
            };
        }

        private void NotifyProgress(string message) {
            if (OnProgressChanged != null) {
                OnProgressChanged(this, new CompileProgressEventArgs { Message = message });
            }
        }

        private List<ProjectCodeFileInfoViewModel> GetCodeFiles() {
            var fileList = new List<ProjectCodeFileInfoViewModel>();
            foreach (var item in project.Children) {
                var file = item as ProjectCodeFileInfoViewModel;
                if (file != null) {
                    fileList.Add(file);
                }
                else {
                    var dir = item as ProjectDirectoryInfoViewModel;
                    if (dir != null) {
                        fileList.AddRange(GetCodeFilesFor(dir));
                    }
                }
            }

            return fileList;
        }

        private List<ProjectCodeFileInfoViewModel> GetCodeFilesFor(ProjectDirectoryInfoViewModel dir) {
            var items = new List<ProjectCodeFileInfoViewModel>();
            foreach (var item in dir.Children) {
                var file = item as ProjectCodeFileInfoViewModel;
                if (file != null) {
                    items.Add(file);
                }
                else {
                    var subDir = item as ProjectDirectoryInfoViewModel;
                    if (subDir != null) {
                        items.AddRange(GetCodeFilesFor(subDir));
                    }
                }
            }

            return items;
        }

        void OnCompileWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (OnCompileFinished != null) {
                OnCompileFinished(this, e.Result as CompileFinishedEventArgs);
            }
        }

        public void StartCompile() {
            if (!compileWorker.IsBusy) {
                compileWorker.RunWorkerAsync();
            }
        }
    }

    public class CompileFinishedEventArgs : EventArgs {
        public bool Success { get; set; }
        public List<CompileError> Errors { get; set; }
    }

    public class CompileProgressEventArgs : EventArgs {
        public string Message { get; set; }
    }
}
