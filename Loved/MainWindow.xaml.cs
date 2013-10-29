using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using Loved.Controls;
using IO = System.IO;

namespace Loved {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private FindReplaceDialog findFilesDialog;
        private Process debuggingProcess;

        public Settings SettingsInstance { get { return Settings.Instance; } }

        public SolutionViewModel Solution {
            get { return (SolutionViewModel)GetValue(SolutionProperty); }
            set { SetValue(SolutionProperty, value); }
        }

        public static readonly DependencyProperty SolutionProperty =
            DependencyProperty.Register("Solution", typeof(SolutionViewModel), typeof(MainWindow));

        public ProjectViewModel Project {
            get { return Solution != null ? Solution.Children[0] : null; }
            set {
                if (Solution != null) {
                    Solution.Children[0] = value;
                }
                else {
                    Solution = new SolutionViewModel(value);
                }
            }
        }

        public static readonly DependencyProperty ProjectProperty =
            DependencyProperty.Register("Project", typeof(ProjectViewModel), typeof(MainWindow));

        public ObservableCollection<CompileError> CompileErrors { get; set; }
        public ObservableCollection<SearchResult> SearchResults { get; set; }

        public RuntimeError RuntimeError {
            get { return (RuntimeError)GetValue(RuntimeErrorProperty); }
            set { SetValue(RuntimeErrorProperty, value); }
        }

        public static readonly DependencyProperty RuntimeErrorProperty =
            DependencyProperty.Register("RuntimeError", typeof(RuntimeError), typeof(MainWindow));

        public CompileError SelectedCompileError {
            get { return (CompileError)GetValue(SelectedCompileErrorProperty); }
            set { SetValue(SelectedCompileErrorProperty, value); }
        }

        public static readonly DependencyProperty SelectedCompileErrorProperty =
            DependencyProperty.Register("SelectedCompileError", typeof(CompileError), typeof(MainWindow));

        public ProjectInfoItemViewModel SelectedProjectItem {
            get { return (ProjectInfoItemViewModel)GetValue(SelectedProjectItemProperty); }
            set { SetValue(SelectedProjectItemProperty, value); }
        }

        public RuntimeError SelectedRuntimeError {
            get { return (RuntimeError)GetValue(SelectedRuntimeErrorProperty); }
            set { SetValue(SelectedRuntimeErrorProperty, value); }
        }

        public static readonly DependencyProperty SelectedRuntimeErrorProperty =
            DependencyProperty.Register("SelectedRuntimeError", typeof(RuntimeError), typeof(MainWindow));

        public static readonly DependencyProperty SelectedProjectItemProperty =
            DependencyProperty.Register("SelectedProjectItem", typeof(ProjectInfoItemViewModel), typeof(MainWindow));

        public bool IsDebugging {
            get { return (bool)GetValue(IsDebuggingProperty); }
            set { SetValue(IsDebuggingProperty, value); }
        }

        public static readonly DependencyProperty IsDebuggingProperty =
            DependencyProperty.Register("IsDebugging", typeof(bool), typeof(MainWindow), new PropertyMetadata(false, OnIsDebuggingPropertyChanged));

        private static void OnIsDebuggingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            UpdateWindowTitle(((MainWindow)d));
        }

        public bool IsCompiling {
            get { return (bool)GetValue(IsCompilingProperty); }
            set { SetValue(IsCompilingProperty, value); }
        }

        public static readonly DependencyProperty IsCompilingProperty =
            DependencyProperty.Register("IsCompiling", typeof(bool), typeof(MainWindow), new PropertyMetadata(false, OnIsCompilingPropertyChanged));

        private static void OnIsCompilingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            UpdateWindowTitle(((MainWindow)d));
        }

        private const string DefaultWindowTitle = "LÖVED - IDE for LÖVE";
        public string WindowTitle {
            get { return (string)GetValue(WindowTitleProperty); }
            set { SetValue(WindowTitleProperty, value); }
        }

        public static readonly DependencyProperty WindowTitleProperty =
            DependencyProperty.Register("WindowTitle", typeof(string), typeof(MainWindow), new PropertyMetadata(DefaultWindowTitle));

        public SearchResult SelectedSearchResult {
            get { return (SearchResult)GetValue(SelectedSearchResultProperty); }
            set { SetValue(SelectedSearchResultProperty, value); }
        }

        public static readonly DependencyProperty SelectedSearchResultProperty =
            DependencyProperty.Register("SelectedSearchResult", typeof(SearchResult), typeof(MainWindow));

        private static void UpdateWindowTitle(MainWindow window) {
            if (window.IsDebugging) {
                window.WindowTitle = string.Format("{0} - Debugging", DefaultWindowTitle);
            }
            else if (window.IsCompiling) {
                window.WindowTitle = string.Format("{0} - Compiling", DefaultWindowTitle);
            }
            else {
                window.WindowTitle = DefaultWindowTitle;
            }
        }

        public MainWindow() {
            CompileErrors = new ObservableCollection<CompileError>();
            SearchResults = new ObservableCollection<SearchResult>();

            LoadLuaSyntaxHighlighting();
            InitializeComponent();

            findFilesDialog = new FindReplaceDialog();
            findFilesDialog.SearchSubmitted += OnFindFilesDialogSearchSubmitted;
            findFilesDialog.ReplaceSubmitted += OnFindFilesDialogReplaceSubmitted;
            
            Loaded += delegate {
                findFilesDialog.Owner = this;
            };

            Closing += delegate {
                SettingsFile.Save(Settings.Instance, SettingsFile.Path); 
            };
        }

        void OnFindFilesDialogReplaceSubmitted(object sender, ReplaceSubmitEventArgs e) {
            ExtrasTabControl.SelectedItem = FindResultsTabItem;

            if (string.IsNullOrEmpty(e.Search) || string.IsNullOrEmpty(e.Replace)) {
                return;
            }

            SearchResults.Clear();

            var comparisonSettings = e.IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

            if (e.FindSource == FindSources.CurrentFile) {
                if (MainTabbedEditor.SelectedCodeFile == null) {
                    return;
                }

                var filePath = MainTabbedEditor.SelectedCodeFile.Path;
                var fileType = FileTypes.GetFileTypeFor(filePath);
                if (fileType == FileType.Code) {
                    var editorText = IO.File.ReadAllText(filePath);
                    if (editorText.Length == 0 || !editorText.Contains(e.Search)) {
                        return;
                    }

                    var vm = Project.GetFile(filePath);
                    if (vm == null) {
                        return;
                    }

                    MainTabbedEditor.AddFile(vm);
                    var codeFile = MainTabbedEditor.CodeFiles.FirstOrDefault(c => c.Path == filePath) as CodeFile;
                    if (codeFile == null) {
                        return;
                    }

                    codeFile.Editor.Text = codeFile.Editor.Text.Replace(e.Search, e.Replace);
                }
            }
            else if (e.FindSource == FindSources.Project) {
                var fileList = IO.Directory.GetFiles(ProjectPath.Path, "*.lua");

                foreach (var file in fileList) {
                    var editorText = IO.File.ReadAllText(file);
                    if (editorText.Length == 0 || !editorText.Contains(e.Search)) {
                        continue;
                    }

                    var vm = Project.GetFile(file);
                    if (vm == null) {
                        continue;
                    }

                    MainTabbedEditor.AddFile(vm);
                    var codeFile = MainTabbedEditor.CodeFiles.FirstOrDefault(c => c.Path == file) as CodeFile;
                    if (codeFile == null) {
                        continue;
                    }

                    codeFile.Editor.Text = codeFile.Editor.Text.Replace(e.Search, e.Replace);
                }   
            }
        }

        void OnFindFilesDialogSearchSubmitted(object sender, SearchSubmitEventArgs e) {
            ExtrasTabControl.SelectedItem = FindResultsTabItem;

            if (string.IsNullOrEmpty(e.Search)) {
                return;
            }

            var comparisonSettings = e.IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

            if (e.FindSource == FindSources.CurrentFile) {
                var file = MainTabbedEditor.SelectedCodeFile as CodeFile;
                if (file != null) {
                    var editorText = file.Editor.Document.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (editorText.Length == 0) {
                        return;
                    }

                    var lineNumber = 0;
                    foreach (var line in editorText) {
                        lineNumber++;
                        
                        if (line.IndexOf(e.Search, comparisonSettings) != -1) {
                            SearchResults.Add(new SearchResult {
                                File = file.Path,
                                Line = lineNumber,
                                Result = line
                            });
                        }
                    }
                }
            }
            else if (e.FindSource == FindSources.Project) {
                SearchResults.Clear();

                var fileList = IO.Directory.GetFiles(ProjectPath.Path, "*.lua");

                foreach (var file in fileList) {
                    var editorText = IO.File.ReadAllLines(file);
                    if (editorText.Length == 0) {
                        continue;
                    }

                    var lineNumber = 0;
                    foreach (var line in editorText) {
                        lineNumber++;

                        if (line.IndexOf(e.Search, comparisonSettings) != -1) {
                            SearchResults.Add(new SearchResult {
                                File = file,
                                Line = lineNumber,
                                Result = line
                            });
                        }
                    }
                }
            }
        }

        private void LoadLuaSyntaxHighlighting() {
            IHighlightingDefinition customHighlighting;
            using (var s = typeof(MainWindow).Assembly.GetManifestResourceStream("Loved.LuaHighlighting.xshd")) {
                if (s == null) {
                    throw new InvalidOperationException("Could not find embedded resource");
                }

                using (XmlReader reader = new XmlTextReader(s)) {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            HighlightingManager.Instance.RegisterHighlighting("Lua", new string[] { ".lua" }, customHighlighting);
        }

        private void OnMenuButtonNewProjectClicked(object sender, RoutedEventArgs e) {

        }

        private void OnMenuButtonProjectFromExistingFilesClicked(object sender, RoutedEventArgs e) {
            ImportProject();
        }

        private bool ImportProject() {
            var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog {
                Filter = "main.lua|main.lua"
            };

            if (dlg.ShowDialog() != true) {
                return false;
            }
            
            Project = new ProjectViewModel(new DirectoryInfo(IO.Path.GetDirectoryName(dlg.FileName)));

            return true;
        }

        private void OnMenuButtonExitClicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void RunDispatcher(Action action) {
            if (!Dispatcher.CheckAccess()) {
                Dispatcher.Invoke(action);
            }
            else {
                action();
            }
        }

        private void ValidateSyntax(Action<CompileFinishedEventArgs> action) {
            IsCompiling = true;
            ExtrasTabControl.SelectedItem = OutputTabItem;

            var compiler = new LuaCompiler(Project);
            compiler.OnProgressChanged += (sender, e) => {
                RunDispatcher(() => { AddOutputText(e.Message); });
            };

            compiler.OnCompileFinished += (sender, e) => {
                RunDispatcher(() => {
                    IsCompiling = false;

                    if (!e.Success) {
                        ExtrasTabControl.SelectedItem = CompileErrorsTabItem;
                    }

                    if (action != null) {
                        action(e);
                    }
                });
            };

            compiler.StartCompile();
        }

        private void AddOutputText(string message) {
            OutputTextbox.AppendText(string.Format("[{0}]: {1}\r\n", DateTime.Now.ToString("h:mm:ss"), message));

            OutputTextbox.Focus();
            OutputTextbox.CaretIndex = OutputTextbox.Text.Length;
            OutputTextbox.ScrollToEnd();
        }


        private void SaveProjectCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MainTabbedEditor != null && MainTabbedEditor.CodeFiles.FirstOrDefault(c => c.IsModified) != null && !IsCompiling && !IsDebugging;
        }

        private void SaveProjectCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            SaveProject();
        }

        private void SaveProject() {
            foreach (var cmd in MainTabbedEditor.CodeFiles.Where(c => c.IsModified)) {
                cmd.Save();
            }
        }

        private void SaveCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MainTabbedEditor != null && MainTabbedEditor.SelectedCodeFile != null && MainTabbedEditor.SelectedCodeFile.IsModified && !IsCompiling && !IsDebugging;
        }

        private void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            MainTabbedEditor.SelectedCodeFile.Save();
        }

        private void RunCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project != null && Project.IsLoaded && !IsDebugging && !IsCompiling;
        }

        private void RunCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            RunProject();
        }

        private void RunProject() {
            CompileProject((e) => {
                if (!e.Success && MessageBox.Show("There are syntax errors. Would you like to run anyway?", "Syntax Errors", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
                    return;
                }

                IsDebugging = true;
                RuntimeError = null;

                debuggingProcess = Process.Start(new ProcessStartInfo(Settings.Instance.LoveExecutablePath, string.Format("\"{0}\"", ProjectPath.Path)) { UseShellExecute = false, RedirectStandardOutput = true });
                debuggingProcess.EnableRaisingEvents = true;

                debuggingProcess.Exited += delegate {
                    Dispatcher.Invoke(new Action(() => {
                        ResolveDebugOutput(debuggingProcess.StandardOutput.ReadToEnd());

                        IsDebugging = false;
                        if (RuntimeError != null) {
                            ExtrasTabControl.SelectedItem = RuntimeErrorsTabItem;
                        }
                        else {
                            ExtrasTabControl.SelectedItem = OutputTabItem;
                        }

                        CommandManager.InvalidateRequerySuggested();
                    }));
                };
            });
        }

        private void ResolveDebugOutput(string text) {
            if (text.Length == 0) {
                return;
            }

            AddOutputText(text);
            var output = DebugOutput.CreateFrom(text);
            if (output.Errors.Count > 0) {
                RuntimeError = output.Errors[0];
            }
        }

        private void StopCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project != null && Project.IsLoaded && IsDebugging && debuggingProcess != null;
        }

        private void StopCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            debuggingProcess.Kill();
            debuggingProcess = null;
        }

        private void CompileCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project != null && Project.IsLoaded && !IsCompiling && !IsDebugging;
        }

        private void CompileCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            CompileProject();
        }

        private void CompileProject(Action<CompileFinishedEventArgs> action = null) {
            SaveProject();
            ValidateSyntax(action);
        }

        void OnSearchResultDoubleClick(object sender, MouseButtonEventArgs e) {
            if (SelectedSearchResult == null) {
                return;
            }

            var file = Project.GetFile(SelectedSearchResult.File);
            if (file != null) {
                MainTabbedEditor.AddFile(file, SelectedSearchResult.Line);
            }
        }

        void OnCompileErrorDoubleClick(object sender, MouseButtonEventArgs e) {
            if (SelectedCompileError == null) {
                return;
            }

            var file = Project.GetFile(SelectedCompileError.Path);
            if (file != null) {
                MainTabbedEditor.AddFile(file, SelectedCompileError.Line);
            }
        }

        void OnRuntimeErrorDoubleClick(object sender, MouseButtonEventArgs e) {
            if (SelectedRuntimeError == null) {
                return;
            }

            var file = Project.GetFile(SelectedRuntimeError.Path);
            if (file != null) {
                MainTabbedEditor.AddFile(file, SelectedRuntimeError.Line);
            }
        }

        private void OnProjectViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var file = e.NewValue as ProjectInfoItemViewModel;
            if (file != null) {
                file.IsSelected = true;
            }
        }

        private void OnPreviewRightMouseButtonDown(object sender, MouseButtonEventArgs e) {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null) {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source) {
            while (source != null && !(source is TreeViewItem)) {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        private void SearchFilesCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            findFilesDialog.ShowSearch();
        }

        private void SearchFilesCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project != null && Project.IsLoaded;
        }

        private void ReplaceFilesCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            findFilesDialog.ShowReplace();
        }

        private void ReplaceFilesCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project != null && Project.IsLoaded;
        }

        private void GotoLineCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var dlg = new GotoLineDialog();
            if (dlg.ShowDialog() == true) {
                if (MainTabbedEditor.SelectedCodeFile is CodeFile) {
                    var file = MainTabbedEditor.SelectedCodeFile as CodeFile;
                    if (dlg.LineNumber < file.Editor.LineCount) {
                        file.Editor.ScrollToLine(dlg.LineNumber);
                        file.Editor.CaretOffset = file.Editor.Document.GetOffset(dlg.LineNumber, 1);
                    }
                }
                else if (MainTabbedEditor.SelectedCodeFile is TextEditorFile) {
                    var file = MainTabbedEditor.SelectedCodeFile as TextEditorFile;
                    if (dlg.LineNumber < file.Editor.LineCount) {
                        file.Editor.ScrollToLine(dlg.LineNumber);
                        file.Editor.CaretOffset = file.Editor.Document.GetOffset(dlg.LineNumber, 1);
                    }
                }
            }
        }

        private void GotoLineCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = CanEditorFileCommandExecute();
        }

        private void OpenPreferencesCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            OpenPreferences();
        }

        private void OpenPreferences() {
            var dlg = new SettingsDialog(false, Settings.Instance) {
                Owner = this
            };

            dlg.ShowDialog();
        }

        private void OpenPreferencesCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private bool CanEditorFileCommandExecute() {
            return MainTabbedEditor != null && MainTabbedEditor.SelectedCodeFile != null && (MainTabbedEditor.SelectedCodeFile is CodeFile || MainTabbedEditor.SelectedCodeFile is TextEditorFile);
        }

        private void OpenFileCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project != null && Project.IsLoaded;
        }

        private void OpenFileCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var file = e.Parameter as ProjectInfoItemViewModel;
            if (file != null) {
                if (MainTabbedEditor.SelectedCodeFile == null || MainTabbedEditor.SelectedCodeFile.Path != file.Path) {
                    MainTabbedEditor.AddFile(file);
                } 
            }
        }

        private void AddFolderCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project != null && Project.IsLoaded && !IsCompiling && !IsDebugging;
        }

        private void AddFolderCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var directoryView = e.Parameter as ProjectDirectoryInfoViewModel;
            string basePath;
            if (directoryView != null) {
                basePath = directoryView.Path;
            }
            else {
                basePath = ProjectPath.Path;
            }

            var dlg = new AddFileDialog(basePath, "You know the drill, enter in the desired directory name:") { Owner = this, FileName = "theendisnigh" };
            if (dlg.ShowDialog() == true) {
                var dirPath = System.IO.Path.Combine(basePath, dlg.FileName);
                var dirInfo = System.IO.Directory.CreateDirectory(dirPath);

                if (directoryView != null) {
                    directoryView.AddChild(dirInfo);
                }
                else {
                    Project.AddChild(dirInfo);
                }
            }
        }

        private void AddFileCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project != null && Project.IsLoaded && !IsCompiling && !IsDebugging;
        }

        private void AddFileCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var directoryView = e.Parameter as ProjectDirectoryInfoViewModel;
            string basePath;
            if (directoryView != null) {
                basePath = directoryView.Path;
            }
            else {
                basePath = ProjectPath.Path;
            }

            var dlg = new AddFileDialog(basePath, "You know the drill, enter in the desired file name:") { Owner = this, FileName = "boringfile.lua" };
            if (dlg.ShowDialog() == true) {
                var filePath = System.IO.Path.Combine(basePath, dlg.FileName);
                System.IO.File.Create(filePath).Close();

                if (directoryView != null) {
                    directoryView.AddChild(new FileInfo(filePath));
                }
                else {
                    Project.AddChild(new FileInfo(filePath));
                }
            }
        }

        private void DeleteFileCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project != null && Project.IsLoaded && Project.SelectedItem != null && !IsCompiling && !IsDebugging;
        }

        private void DeleteFileCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var isDirectory = System.IO.Path.GetExtension(Project.SelectedItem.Path) == "";
            if (MessageBox.Show(string.Format("Are you sure you wish to delete this {0} permanently?", isDirectory ? "directory" : "file"), "Delete Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                Project.SelectedItem.Delete();
            }
        }

        private void OpenStackTraceFileCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = RuntimeError != null && RuntimeError.StackTrace.Count > 0;
        }

        private void OpenStackTraceFileCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var item = e.Parameter as StackTraceItem;
            if (item != null) {
                var file = Project.GetFile(ProjectPath.ExpandProjectPath(item.File));
                if (file != null) {
                    MainTabbedEditor.AddFile(file, item.Line);
                }
            }
        }

        private void OnCanProjectItemEnterEditMode(object sender, CanEnterEditModeEventArgs e) {
            if (IsCompiling || IsDebugging) {
                e.CanExecute = false;
                return;
            }

            var file = ((EditableTextBlock)sender).DataContext as ProjectInfoItemViewModel;
            if (file != null) {
                if (file is ProjectDirectoryInfoViewModel) {
                    e.CanExecute = true;
                }
                else {
                    e.CanExecute = file.IsCurrent;
                }
            }
        }
    }
}
