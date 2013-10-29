using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using Loved.Controls;
using WpfAnimatedGif;

namespace Loved {
    /// <summary>
    /// Interaction logic for TabbedLuaEditor.xaml
    /// </summary>
    public partial class TabbedLuaEditor : UserControl {
        public ObservableCollection<EditorInfo> CodeFiles { get; set; }

        public EditorInfo SelectedCodeFile {
            get { return (EditorInfo)GetValue(SelectedCodeFileProperty); }
            set { SetValue(SelectedCodeFileProperty, value); }
        }

        public static readonly DependencyProperty SelectedCodeFileProperty =
            DependencyProperty.Register("SelectedCodeFile", typeof(EditorInfo), typeof(TabbedLuaEditor));

        public EditorInfo CurrentCodeFile {
            get { return (EditorInfo)GetValue(CurrentCodeFileProperty); }
            set { SetValue(CurrentCodeFileProperty, value); }
        }

        public static readonly DependencyProperty CurrentCodeFileProperty =
            DependencyProperty.Register("CurrentCodeFile", typeof(EditorInfo), typeof(TabbedLuaEditor), new PropertyMetadata(null, OnCurrentCodeFileChanged));

        private static void OnCurrentCodeFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var editor = (TabbedLuaEditor)d;
            foreach (var c in editor.CodeFiles) {
                c.Model.IsCurrent = false;
            }

            ((EditorInfo)e.NewValue).Model.IsCurrent = true;
          //Shape sh = (Shape) d;
          //sh.Fill = new ImageBrush(new BitmapImage((Uri)e.NewValue));
        }

        public TabbedLuaEditor() {
            CodeFiles = new ObservableCollection<EditorInfo>();

            InitializeComponent();
        }

        public void AddFile(ProjectInfoItemViewModel model, int scrollTo = -1) {
            if (ContainsFile(model.Path)) {
                SwitchFile(model.Path, scrollTo);
                return;
            }

            if (ContainsFile(model.Path)) {
                SwitchFile(model.Path, scrollTo);
                return;
            }

            switch (FileTypes.GetFileTypeFor(model.Path)) {
                case FileType.Code:
                    CodeFiles.Add(new CodeFile(model));
                    break;
                case FileType.Image:
                    if (System.IO.Path.GetExtension(model.Path) == ".gif") {
                        CodeFiles.Add(new GifEditorFile(model));
                    }
                    else {
                        CodeFiles.Add(new ImageEditorFile(model));
                    }
                    break;
                case FileType.Audio:
                    CodeFiles.Add(new AudioEditorFile(model));
                    break;
                case FileType.Text:
                    CodeFiles.Add(new TextEditorFile(model));
                    break;
            }

            model.IsOpen = true;
            
            SwitchFile(model.Path, scrollTo);
        }

        public bool ContainsFile(string path) {
            return CodeFiles.Count(c => c.Path == path) > 0;
        }

        public void SwitchFile(string path, int scrollTo = -1) {
            var file = CodeFiles.FirstOrDefault(c => c.Path == path);
            if (file != null) {
                
                System.Threading.ThreadPool.QueueUserWorkItem(
                  (a) => {
                      System.Threading.Thread.Sleep(100);
                      Dispatcher.Invoke(
                      new Action(() => {
                          SelectedCodeFile = file;
                          CurrentCodeFile = file;
                          SelectedCodeFile.Content.Focus();

                          if (scrollTo != -1) {
                              if (SelectedCodeFile is CodeFile) {
                                  var editor = ((CodeFile)SelectedCodeFile).Editor;
                                  editor.ScrollToLine(scrollTo);

                                  editor.CaretOffset = editor.Document.GetOffset(scrollTo, 1);

                                  var line = editor.Document.GetLineByOffset(editor.CaretOffset);
                                  editor.Select(line.Offset, line.Length);
                              }
                          }
                      }));
                  });
            }
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e) {
            var file = ((FrameworkElement)sender).Tag as EditorInfo;

            var codeFile = file as CodeFile;

            if (codeFile != null) {
                if (codeFile.Editor.IsModified) {
                    var result = MessageBox.Show("There are unsaved changes. Would you like to save before closing?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Cancel) {
                        return;
                    }
                    else if (result == MessageBoxResult.Yes) {
                        codeFile.Editor.Save(codeFile.Path);
                    }
                }
            }

            var textFile = file as TextEditorFile;
            if (textFile != null && textFile.Editor.IsModified) {
                var result = MessageBox.Show("There are unsaved changes. Would you like to save before closing?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel) {
                    return;
                }
                else if (result == MessageBoxResult.Yes) {
                    textFile.Editor.Save(textFile.Path); 
                }
            }

            file.Model.IsOpen = false;
            CodeFiles.Remove(file);
        }
        /*
        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e) {
            var tabItem = e.Source as TabItem;

            if (tabItem == null) {
                return;
            }

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed) {
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
            }
        }


        private void TabItem_Drop(object sender, DragEventArgs e) {
            var tabItemTarget = e.Source as TabItem;
            var tabItemSource = e.Data.GetData(typeof(TabItem)) as TabItem;

            if (!tabItemTarget.Equals(tabItemSource)) {
                var tabControl = tabItemTarget.Parent as TabControl;
                int sourceIndex = tabControl.Items.IndexOf(tabItemSource);
                int targetIndex = tabControl.Items.IndexOf(tabItemTarget);

                tabControl.Items.Remove(tabItemSource);
                tabControl.Items.Insert(targetIndex, tabItemSource);

                tabControl.Items.Remove(tabItemTarget);
                tabControl.Items.Insert(sourceIndex, tabItemTarget);
            }
        }*/

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e) {
            var tabItem = e.Source as TabItem;

            if (tabItem == null) {
                return;
            }

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed) {
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.Move);
            }
        }


        private void TabItem_Drop(object sender, DragEventArgs e) {
            var tabItemTargetTab = e.Source as TabItem;
            if (tabItemTargetTab == null) {
                return;
            }

            var tabItemTarget = tabItemTargetTab.DataContext as EditorInfo;

            var tabItemSourceTab = e.Data.GetData(typeof(TabItem)) as TabItem;
            var tabItemSource = tabItemSourceTab.DataContext as EditorInfo;

            if (!tabItemTarget.Equals(tabItemSource)) {
                var sourceIndex = CodeFiles.IndexOf(tabItemSource);
                var targetIndex = CodeFiles.IndexOf(tabItemTarget);

                CodeFiles.Remove(tabItemSource);
                CodeFiles.Insert(targetIndex, tabItemSource);

                CodeFiles.Remove(tabItemTarget);
                CodeFiles.Insert(sourceIndex, tabItemTarget);
            }

            SelectedCodeFile = tabItemSource;
        }
    }

    public class FileCompletionData : ICompletionData {
        public FileCompletionData(FileSystemInfo info) {
            Text = info.Name;

            if (info.IsDirectory()) {
                Image = new BitmapImage(new Uri("pack://application:,,,/Icons/folder.png"));
            }
            else {
                Image = new BitmapImage(new Uri("pack://application:,,,/Icons/script.png"));
            }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs) {
            textArea.Document.Replace(completionSegment, Text);
        }

        public object Content {
            get { return Text; }
        }

        public object Description {
            get;
            private set;
        }

        public ImageSource Image {
            get;
            private set;
        }

        public double Priority {
            get { return 1; }
        }

        public string Text {
            get;
            private set;
        }
    }

    public class MyCompletionData : ICompletionData {
        public MyCompletionData(NamespaceInfo info) {
            Text = info.Name;
            Description = info.Description;

            switch (info.Type) {
                case NamespaceInfoType.Table:
                    //Uri uri = new Uri("pack://application:,,,/munch.png");
                    Image = new BitmapImage(new Uri("pack://application:,,,/Icons/tag.png"));
                    break;
                case NamespaceInfoType.Function:
                    Image = new BitmapImage(new Uri("pack://application:,,,/Icons/arrow_right.png"));
                    break;
            }
        }

        public System.Windows.Media.ImageSource Image {
            get;
            private set;
        }

        public string Text { get; private set; }

        public object Content {
            get { return this.Text; }
        }

        public object Description {
            get;
            private set;
        }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs) {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        public double Priority {
            get { return 1; }
        }
    }

    public abstract class EditorInfo : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _displayName;
        public string DisplayName {
            get { return _displayName; }
            set {
                if (_displayName != value) {
                    _displayName = value;
                    if (PropertyChanged != null) {
                        PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
                    }
                }
            }
        }

        public FrameworkElement Content { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public ProjectInfoItemViewModel Model { get; private set; }

        public virtual bool IsModified { get { return false; } }

        public virtual void Save() {
        }

        public EditorInfo(ProjectInfoItemViewModel model) {
            Path = model.Path;
            FileName = model.Name;
            DisplayName = FileName;
            Model = model;

            model.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Name") {
                    DisplayName = model.Name;
                    Path = model.Path;
                }
            };
        }
    }

    public class CodeFile : EditorInfo {
        private CompletionWindow completionWindow;
        private OverloadInsightWindow overloadWindow;
        private FoldingManager foldingManager;
        private LuaFoldingStrategy foldingStrategy;

        private static List<NamespaceInfo> namespaceInfo = NamespaceInfoFile.LoadLuaNamespace();

        public TextEditor Editor {
            get { return (TextEditor)Content; }
            set { Content = value; }
        }
        
        //public TextDocument Document { get; set; }

        public CodeFile(ProjectInfoItemViewModel model) : base(model) {
            Editor = new TextEditor {
                FontFamily = new FontFamily("Consolas"),
                SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("Lua"),
                ShowLineNumbers = true
            };

            Model.PropertyChanged += (s, o) => {
                if (o.PropertyName == "Name" || o.PropertyName == "Path") {
                    Editor.Load(model.Path);
                }
            };

            Editor.Load(Path);

            foldingManager = FoldingManager.Install(Editor.TextArea);
            foldingStrategy = new LuaFoldingStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, Editor.Document);  

            Editor.TextArea.TextEntering += (sender, e) => {
                if (e.Text.Length > 0 && completionWindow != null) {
                    if (!char.IsLetterOrDigit(e.Text[0])) {
                        completionWindow.CompletionList.RequestInsertion(e);
                    }
                }
            };

            Editor.Document.Changed += (sender, e) => {
                if (overloadWindow != null) {
                    if (e.RemovedText == ",") {
                        ((OverloadShower)overloadWindow.Provider).ParameterIndex--;
                    }
                }
            };

            Editor.TextArea.TextEntered += (sender, e) => {
                if (e.Text == ".") {
                    var completionList = BuildCompletionListFor(Editor.GetWordBeforeDot(true));
                    if (completionList != null && completionList.Count > 0) {
                        completionWindow = new CompletionWindow(Editor.TextArea);
                        completionList.ForEach(completionWindow.CompletionList.CompletionData.Add);

                        completionWindow.Show();
                        completionWindow.Closed += delegate {
                            completionWindow = null;
                        };
                    }
                }
                else if (e.Text == "(") {
                    var word = Editor.GetWordBeforeDot(true);

                    var prototypes = GetPrototypesFor(word);
                    if (prototypes != null) {
                        overloadWindow = new OverloadInsightWindow(Editor.TextArea);
                        overloadWindow.Provider = new OverloadShower(prototypes);

                        overloadWindow.Show();
                        overloadWindow.Closed += delegate {
                            overloadWindow = null;
                        };
                    }
                }
                else if (e.Text == ")") {
                    if (overloadWindow != null) {
                        overloadWindow.Close();
                    }
                }
                else if (e.Text == "'") {
                    var word = Editor.GetWordBeforeSpace();
                    if (word == "require") {
                        completionWindow = new CompletionWindow(Editor.TextArea);

                        foreach (var file in new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(Path)).GetFileSystemInfos()) {
                            if (file.IsDirectory() || System.IO.Path.GetExtension(file.Name) == ".lua") {
                                completionWindow.CompletionList.CompletionData.Add(new FileCompletionData(file));
                            }
                        }

                        completionWindow.Show();
                        completionWindow.Closed += delegate {
                            completionWindow = null;
                        };
                    }
                }
                else if (e.Text == "/") {
                    var word = Editor.GetWordBeforeSpace();
                    if (word != null) {
                    }
                }
                else if (e.Text == ",") {
                    if (overloadWindow != null) {
                        ((OverloadShower)overloadWindow.Provider).ParameterIndex++;
                    }
                }
            };

            var dpd = DependencyPropertyDescriptor.FromProperty(TextEditor.IsModifiedProperty, typeof(TextEditor));
            dpd.AddValueChanged(Editor, delegate {
                model.IsModified = Editor.IsModified;
                DisplayName = string.Format("{0}{1}", FileName, Editor.IsModified ? "*" : "");
                
                if (!Editor.IsModified && foldingStrategy != null) {
                    foldingStrategy.UpdateFoldings(foldingManager, Editor.Document);
                }
            });

            model.PropertyChanged += (o, e) => {
                if (e.PropertyName == "IsModified") {
                    Editor.IsModified = model.IsModified;
                }
            };
        }

        private List<MyCompletionData> BuildCompletionListFor(string word) {
            var words = word.Split(new[] { '.' });
            var currentNamespace = namespaceInfo.FirstOrDefault(c => c.Name == words[0]);
            if (currentNamespace == null) {
                return null;
            }
            
            for (int i = 1; i < words.Length; i++) {
                currentNamespace = currentNamespace.Children.FirstOrDefault(c => c.Name == words[i]);
                if (currentNamespace == null) {
                    break;
                }
            }

            if (currentNamespace != null) {
                return currentNamespace.Children.Select(c => new MyCompletionData(c)).ToList();
            }

            return null;
        }

        private List<NamespaceInfo> GetPrototypesFor(string word) {
            var words = word.Split(new[] { '.' });
            NamespaceInfo currentNamespace = namespaceInfo.FirstOrDefault(c => c.Name == words[0]);

            for (int i = 1; i < words.Length - 1; i++) {
                currentNamespace = currentNamespace.Children.FirstOrDefault(c => c.Name == words[i]);
                if (currentNamespace == null) {
                    break;
                }
            }

            if (currentNamespace != null) {
                return currentNamespace.Children.Where(c => c.Name == words[words.Length - 1]).ToList();
            }

            return null;
        }

        private static readonly List<string> LoveNamespaceItems = new List<string> {
            "audio",
            "event",
            "filesystem",
            "font",
            "graphics",
            "image",
            "joystick",
            "keyboard",
            "mouse",
            "physics",
            "sound",
            "thread",
            "timer"
        };

        private static readonly List<NamespaceInfo> NamespaceItems = new List<NamespaceInfo> {
            new NamespaceInfo("love", "root love namespace", new List<NamespaceInfo> {
                new NamespaceInfo("audio", "Provides an interface to output sound to the user's speakers.", null),

            })
        };

        public override void Save() {
            Editor.Save(Path);
        }

        public override bool IsModified {
            get {
                return Editor.IsModified;
            }
        }
    }

    public class OverloadShower : IOverloadProvider {
        private List<NamespaceInfo> prototypes;
        private TextBlock parameterTextBlock;
        private TextBlock descriptionTextBlock;
        private int index;

        public OverloadShower(List<NamespaceInfo> prototypeList) {
            parameterTextBlock = new TextBlock();
            descriptionTextBlock = new TextBlock();
            prototypes = prototypeList;
        }

        private int parameterIndex;
        public int ParameterIndex {
            get { return parameterIndex; }
            set {
                if (parameterIndex != value) {
                    parameterIndex = value;
                    NotifyPropertyChanged("ParameterIndex");
                    NotifyPropertyChanged("CurrentHeader");
                    NotifyPropertyChanged("CurrentContent");
                    NotifyPropertyChanged("CurrentIndexText");
                }
            }
        }

        public int Count {
            get { return prototypes.Count; }
        }

        public object CurrentContent {
            get {
                UpdateDescription();
                return descriptionTextBlock;
            }
        }

        public object CurrentHeader {
            get {
                UpdateHeader();
                return parameterTextBlock;
            }
        }

        public string CurrentIndexText {
            get { return (SelectedIndex + 1).ToString() + " of " + Count.ToString(); }
        }

        public int SelectedIndex {
            get {
                return index;
            }
            set {
                if (index != value) {
                    index = value;
                    NotifyPropertyChanged("SelectedIndex");
                    NotifyPropertyChanged("CurrentHeader");
                    NotifyPropertyChanged("CurrentContent");
                    NotifyPropertyChanged("CurrentIndexText");
                }
            }
        }

        private void UpdateHeader() {
            var prototype = prototypes[index].Prototype;
            var prototypeExpression = prototype;

            if (prototypeExpression.Contains(",")) {
                var split = prototypeExpression.Split(new[] { ',' });

                if (parameterIndex == 0) {
                    prototypeExpression = split[0].Replace("(", "(<bold>") + "</bold>";
                }
                else {
                    prototypeExpression = split[0];
                }

                for (int i = 1; i < split.Length; i++) {
                    prototypeExpression += ",";
                    if (i == parameterIndex) {
                        if (i == split.Length - 1) {
                            prototypeExpression += "<bold>" + split[i].Substring(0, split[i].IndexOf(")")) + "</bold>)";
                        }
                        else {
                            prototypeExpression += "<bold>" + split[i] + "</bold>";
                        }
                    }
                    else {
                        prototypeExpression += split[i];
                    }
                }
            }
            else if (prototypeExpression.IndexOf(")") - 1 > prototypeExpression.IndexOf("(")) {
                prototypeExpression = prototypeExpression.Replace("(", "(<bold>");
                prototypeExpression = prototypeExpression.Replace(")", "</bold>)");
            }

            InlineExpression.SetInlineExpression(parameterTextBlock, prototypeExpression);
        }

        private void UpdateDescription() {
            //prototypes[index].Description + "\r\n" + "..."; 
            var parameterExpression = prototypes[index].Description + "\r\n";

            parameterExpression += "<italic>" + "Example parameter info" + "</italic>";

            InlineExpression.SetInlineExpression(descriptionTextBlock, parameterExpression);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string prop) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        private void NotifyPropertyChanged(int index) { }
    }

    public class ImageEditorFile : EditorInfo {
        public Image View {
            get { return (Image)Content; }
            set { Content = value; }
        }

        public ImageEditorFile(ProjectInfoItemViewModel model)
            : base(model) {
                View = new Image {
                    Source = new BitmapImage(new Uri(Path, UriKind.Absolute)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Stretch = Stretch.None
                };
        }
    }

    public class GifEditorFile : EditorInfo {
        public GifViewer View {
            get { return (GifViewer)Content; }
            set { Content = value; }
        }

        public GifEditorFile(ProjectInfoItemViewModel model)
            : base(model) {
            /*View = new Image {
                //Source = new BitmapImage(new Uri(path, UriKind.Absolute)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.None
            };
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(Path, UriKind.Absolute);
            image.EndInit();

            ImageBehavior.SetAnimatedSource(View, image);*/
                View = new GifViewer(Path) {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    //Stretch = Stretch.None
                };
        }
    }

    public class AudioEditorFile : EditorInfo {
        public MediaPlayer Player {
            get { return (MediaPlayer)Content; }
            set { Content = value; }
        }

        public AudioEditorFile(ProjectInfoItemViewModel model)
            : base(model) {
                Player = new MediaPlayer(Path) {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
        }
    }

    public class TextEditorFile : EditorInfo {
        public TextEditor Editor {
            get { return (TextEditor)Content; }
            set { Content = value; }
        }

        //public TextDocument Document { get; set; }

        public TextEditorFile(ProjectInfoItemViewModel model)
            : base(model) {
            Editor = new TextEditor {
                FontFamily = new FontFamily("Consolas"),
                SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(Path)),
                ShowLineNumbers = true
            };

            var dpd = DependencyPropertyDescriptor.FromProperty(TextEditor.IsModifiedProperty, typeof(TextEditor));
            dpd.AddValueChanged(Editor, delegate {
                DisplayName = string.Format("{0}{1}", FileName, Editor.IsModified ? "*" : "");
                model.IsModified = Editor.IsModified;
            });

            model.PropertyChanged += (o, e) => {
                if (e.PropertyName == "IsModified") {
                    Dispatcher.CurrentDispatcher.Invoke(() => {
                        Editor.IsModified = model.IsModified;
                    });
                }
            };

            Editor.Load(Path);
        }

        public override void Save() {
            Editor.Save(Path);
        }

        public override bool IsModified {
            get {return Editor.IsModified; }
        }
    }

    public class LuaFoldingStrategy : AbstractFoldingStrategy {
        public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset) {
            firstErrorOffset = -1;
            return CreateNewFoldings(document);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document) {
            var newFoldings = new List<NewFolding>();

            var text = document.Text; //handelseif
            //@"(.*\{)|(.*\})|(\r\n)|(.*function)|(.*end)|(.*[^A-Za-z]if)"
            var matches = Regex.Matches(text, @"([\r\n])|(?<!--.*)({|}|[^A-Za-z]if|end|function|for|while)");//
            //var startOffsets = new Stack<FoldingOffset>();
            //var matches = Regex.Matches(text, @"(?:.*)(--[^\r\n]*)");
            var startOffsets = new Dictionary<OffsetType, Stack<int>> {
                {OffsetType.Bracket, new Stack<int>()},
                {OffsetType.Function, new Stack<int>()},
                {OffsetType.If, new Stack<int>()},
                {OffsetType.For, new Stack<int>()}
            };

            var lastNewLineOffset = 0;
            foreach (Match z in matches) {
                for (int i = 1; i < z.Groups.Count; i++) {
                    var m = z.Groups[i];
                    var value = m.Value;
                    if (value.Contains("--")) {
                        value = value.Remove(value.IndexOf("--"));
                    }

                    if (value.StartsWith("--")) {
                    }
                    else if (value.EndsWith("{") || m.Value.StartsWith("{")) {
                        //startOffsets.Push(new FoldingOffset(m.Index, OffsetType.Bracket));
                        startOffsets[OffsetType.Bracket].Push(m.Index);
                    }
                    else if (value.EndsWith("}") || m.Value.StartsWith("}")) {
                        var startOffset = startOffsets[OffsetType.Bracket].Pop();
                        // don't fold if opening and closing brace are on the same line
                        if (startOffset < lastNewLineOffset) {
                            newFoldings.Add(new NewFolding(startOffset, m.Index + m.Length));
                        }
                    }
                    else if (value.StartsWith("function") || m.Value.EndsWith("function")) {
                        startOffsets[OffsetType.Function].Push(m.Index + m.Length);
                    }
                    else if (value.StartsWith("if") || m.Value.EndsWith("if")) {
                        startOffsets[OffsetType.If].Push(m.Index + 3);
                    }
                    else if (value.StartsWith("end") || m.Value.EndsWith("end")) {
                        int functionOffset = -1;
                        int ifOffset = -1;
                        int forOffset = -1;

                        if (startOffsets[OffsetType.Function].Count > 0) {
                            functionOffset = startOffsets[OffsetType.Function].Peek();
                        }

                        if (startOffsets[OffsetType.If].Count > 0) {
                            ifOffset = startOffsets[OffsetType.If].Peek();
                        }

                        if (startOffsets[OffsetType.For].Count > 0) {
                            forOffset = startOffsets[OffsetType.For].Peek();
                        }

                        if (functionOffset > ifOffset && functionOffset > forOffset) {
                            var startOffset = startOffsets[OffsetType.Function].Pop();

                            if (startOffset < lastNewLineOffset) {
                                newFoldings.Add(new NewFolding(startOffset, m.Index + m.Length));
                            }
                        }
                        else if (ifOffset > forOffset) {
                            var startOffset = startOffsets[OffsetType.If].Pop();

                            if (startOffset < lastNewLineOffset) {
                                newFoldings.Add(new NewFolding(startOffset, m.Index + m.Length));
                            }
                        }
                        else if (forOffset != -1) {
                            var startOffset = startOffsets[OffsetType.For].Pop();
                        }
                    }
                    else if (value == "for" || value == "while") {
                        startOffsets[OffsetType.For].Push(m.Index);
                    }
                    else if (value == "\r\n" || value == "\n") {
                        lastNewLineOffset = m.Index + 1;
                    }
                }
            }

            /*var startOffsets = new Stack<int>();
            var lastNewLineOffset = 0;
            var openingBrace = '{';
            var closingBrace = '}';
            for (int i = 0; i < document.TextLength; i++) {
                var c = document.GetCharAt(i);
                if (c == openingBrace) {
                    startOffsets.Push(i);
                }
                else if (c == closingBrace && startOffsets.Count > 0) {
                    int startOffset = startOffsets.Pop();
                    // don't fold if opening and closing brace are on the same line
                    if (startOffset < lastNewLineOffset) {
                        newFoldings.Add(new NewFolding(startOffset, i + 1));
                    }
                }
                else if (c == '\n' || c == '\r') {
                    lastNewLineOffset = i + 1;
                }
            }*/

            //Split {}\r\n and --
            /*var tokenRegex = @"(--[^\n\r]*)|(\{)|(\})|(\r\n)";
            //var tokenRegex = @"(\r\n)";
            var tokenList = new Queue<string>(Regex.Split(document.Text, tokenRegex));
            var lastNewLineOffset = 0;
            var startOffsets = new Stack<int>();
            
            while (tokenList.Count > 0) {
                var token = tokenList.Dequeue();

                if (token == "{") {
                    
                }
                else if (token == "}") {
                }
                else if (token == "\r\n") {
                    lastNewLineOffset++;
                }
                else if (token.StartsWith("--")) {
                    lastNewLineOffset++;
                }
            }*/

            /*var lineList = document.Text.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var lastNewLineOffset = 0;
            var fileOffset = 0;
            var startOffsets = new Stack<int>();
            
            foreach (var line in lineList) {
                if (line.StartsWith("--")) {
                }
                else if (line.Contains('{')) {
                    var text = document.GetText(fileOffset, 15);
                    startOffsets.Push(fileOffset);
                    //startOffsets.Push(fileOffset + line.IndexOf('{'));
                }
                else if (line.Contains("function")) {
                    startOffsets.Push(fileOffset + line.IndexOf("function"));
                }
                else if (line.Contains("if")) {
                    startOffsets.Push(fileOffset + line.IndexOf("if"));
                }
                else if (line.Contains('}')) {
                    int startOffset = startOffsets.Pop();
                    // don't fold if opening and closing brace are on the same line
                    if (startOffset < fileOffset) {
                        newFoldings.Add(new NewFolding(startOffset, fileOffset + line.IndexOf('}')));
                    }
                }
                else if (line.Contains("end")) {
                    int startOffset = startOffsets.Pop();
                    // don't fold if opening and closing brace are on the same line
                    if (startOffset < fileOffset) {
                        newFoldings.Add(new NewFolding(startOffset, fileOffset + line.IndexOf("end")));
                    }
                }

                fileOffset += line.Length - 1;
                lastNewLineOffset++;
            }*/

            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }
    }

    public class FoldingOffset {
        public int Offset { get; set; }
        public OffsetType Type { get; set; }

        public FoldingOffset(int offset, OffsetType type) {
            Offset = offset;
            Type = type;
        }
    }

    public enum OffsetType {
        Bracket,
        Function,
        If,
        For
    }
}
