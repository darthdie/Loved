using System;
using System.Collections.Generic;
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

namespace Loved {
    /// <summary>
    /// Interaction logic for LuaTextEditor.xaml
    /// </summary>
    public partial class LuaTextEditor : UserControl {
        public LuaTextEditor() {
            InitializeComponent();
        }

        public LuaTextEditor(string path) {
            InitializeComponent();

            MainRichTextBox.AppendText(System.IO.File.ReadAllText(path));
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e) {
            var editor = sender as RichTextBox;
            if (editor == null || editor.Document == null) {
                return;
            }

            var documentRange = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
            documentRange.ClearAllProperties();

            var navigator = editor.Document.ContentStart;

            var tags = new List<Tag>();
            while (navigator.CompareTo(editor.Document.ContentEnd) < 0) {
                var context = navigator.GetPointerContext(LogicalDirection.Backward);
                var run = navigator.Parent as Run;
                if (context == TextPointerContext.ElementStart && run != null) {//&& navigator.Parent is Run
                    tags.AddRange(GetTagsForRun(run));
                }

                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
            }

            if (tags.Count > 0) {
                editor.TextChanged -= OnEditorTextChanged;
                FormatEditor(editor, tags);
                editor.TextChanged += OnEditorTextChanged;
            }
        }

        private void FormatEditor(RichTextBox editor, List<Tag> formatTags) {
            foreach (var tag in formatTags) {
                var range = new TextRange(tag.StartPosition, tag.EndPosition);

                switch (tag.Type) {
                    case SyntaxType.Keyword:
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Blue));
                        range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        break;

                    case SyntaxType.Keyword2:
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.LightSkyBlue));
                        range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        break;

                    case SyntaxType.Comment:
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.LightGreen));
                        range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                        break;

                    case SyntaxType.BuiltinFunction:
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.DarkBlue));
                        range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                        range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        break;

                    case SyntaxType.None:
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
                        range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        break;
                }
            }
        }

        //(--[^\n\r]*)|
        //private const string LuaRegex = @"(function.*\(.*\))|([ \r\n\t]elseif)|[\n\r\t ](if)|([ \n\r\t]and)|([ \n\r\t]else)|([ \n\r\t]end)|([ \n\r\t]then)|([ \n\r\t]or)";
        //private const string LuaRegex = @"(--[^\n\r]*)|([ \a\r\n\t]if)";
        private const string LuaRegex = @"(--[^\n\r]*)|and|break|do|else|elseif|end|false|for|function|if|in|local|nil|not|or|repeat|return|then|true|until|while|_VERSION|assert|collectgarbage|dofile|error|gcinfo|loadfile|loadstring|print|tonumber|tostring|type|unpack|_ALERT|_ERRORMESSAGE|_INPUT|_PROMPT|_OUTPUT|_STDERR|_STDIN|_STDOUT|call|dostring|foreach|foreachi|getn|globals|newtype|rawget|rawset|require|sort|tinsert|tremove|_G|getfenv|getmetatable|ipairs|loadlib|next|pairs|pcall|rawegal|rawget|rawset|require|setfenv|setmetatable|xpcall|string|table|math|coroutine|io|os|debug|abs|acos|asin|atan|atan2|ceil|cos|deg|exp|floor|format|frexp|gsub|ldexp|log|log10|max|min|mod|rad|random|randomseed|sin|sqrt|strbyte|strchar|strfind|strlen|strlower|strrep|strsub|strupper|tan|string.byte|string.char|string.dump|string.find|string.len|string.lower|string.rep|string.sub|string.upper|string.format|string.gfind|string.gsub|table.concat|table.foreach|table.foreachi|table.getn|table.sort|table.insert|table.remove|table.setn|math.abs|math.acos|math.asin|math.atan|math.atan2|math.ceil|math.cos|math.deg|math.exp|math.floor|math.frexp|math.ldexp|math.log|math.log10|math.max|math.min|math.mod|math.pi|math.rad|math.random|math.randomseed|math.sin|math.sqrt|math.tan|openfile|closefile|readfrom|writeto|appendto|remove|rename|flush|seek|tmpfile|tmpname|read|write|clock|date|difftime|execute|exit|getenv|setlocale|time|coroutine.create|coroutine.resume|coroutine.status|coroutine.wrap|coroutine.yield|io.close|io.flush|io.input|io.lines|io.open|io.output|io.read|io.tmpfile|io.type|io.write|io.stdin|io.stdout|io.stderr|os.clock|os.date|os.difftime|os.execute|os.exit|os.getenv|os.remove|os.rename|os.setlocale|os.time|os.tmpname";

        private List<Tag> GetTagsForRun(Run run) {
            var tags = new List<Tag>();

            foreach (Match match in Regex.Matches(run.Text, LuaRegex)) {
                tags.Add(CreateTagFrom(match, run));
            }

            return tags;
        }

        private Tag CreateTagFrom(Capture capture, Run run) {
            return new Tag {
                StartPosition = run.ContentStart.GetPositionAtOffset(capture.Index, LogicalDirection.Forward),
                EndPosition = run.ContentStart.GetPositionAtOffset(capture.Index + capture.Length, LogicalDirection.Backward),
                Word = capture.Value,
                Type = EvaluateSyntaxType(capture.Value)
            };
        }

        private static string[] KeywordList = { "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "if", "in", "local", "nil", "not", "or", "repeat", "return", "then", "true", "until", "while" };
        private static string[] SecondaryKeywordList = { "_VERSION", "assert", "collectgarbage", "dofile", "error", "gcinfo", "loadfile", "loadstring", "print", "tonumber", "tostring", "type", "unpack", "_ALERT", "_ERRORMESSAGE", "_INPUT", "_PROMPT", "_OUTPUT", "_STDERR", "_STDIN", "_STDOUT", "call", "dostring", "foreach", "foreachi", "getn", "globals", "newtype", "rawget", "rawset", "require", "sort", "tinsert", "tremove", "_G", "getfenv", "getmetatable", "ipairs", "loadlib", "next", "pairs", "pcall", "rawegal", "rawget", "rawset", "require", "setfenv", "setmetatable", "xpcall", "string", "table", "math", "coroutine", "io", "os", "debug" };
        private static string[] BuiltinFunctionList = { "abs", "acos", "asin", "atan", "atan2", "ceil", "cos", "deg", "exp", "floor", "format", "frexp", "gsub", "ldexp", "log", "log10", "max", "min", "mod", "rad", "random", "randomseed", "sin", "sqrt", "strbyte", "strchar", "strfind", "strlen", "strlower", "strrep", "strsub", "strupper", "tan", "string.byte", "string.char", "string.dump", "string.find", "string.len", "string.lower", "string.rep", "string.sub", "string.upper", "string.format", "string.gfind", "string.gsub", "table.concat", "table.foreach", "table.foreachi", "table.getn", "table.sort", "table.insert", "table.remove", "table.setn", "math.abs", "math.acos", "math.asin", "math.atan", "math.atan2", "math.ceil", "math.cos", "math.deg", "math.exp", "math.floor", "math.frexp", "math.ldexp", "math.log", "math.log10", "math.max", "math.min", "math.mod", "math.pi", "math.rad", "math.random", "math.randomseed", "math.sin", "math.sqrt", "math.tan", "openfile", "closefile", "readfrom", "writeto", "appendto", "remove", "rename", "flush", "seek", "tmpfile", "tmpname", "read", "write", "clock", "date", "difftime", "execute", "exit", "getenv", "setlocale", "time", "coroutine.create", "coroutine.resume", "coroutine.status", "coroutine.wrap", "coroutine.yield", "io.close", "io.flush", "io.input", "io.lines", "io.open", "io.output", "io.read", "io.tmpfile", "io.type", "io.write", "io.stdin", "io.stdout", "io.stderr", "os.clock", "os.date", "os.difftime", "os.execute", "os.exit", "os.getenv", "os.remove", "os.rename", "os.setlocale", "os.time", "os.tmpname" };

        private static SyntaxType EvaluateSyntaxType(string word) {
            if (word.StartsWith("--")) {
                return SyntaxType.Comment;
            }

            if (KeywordList.Contains(word)) {
                return SyntaxType.Keyword;
            }

            if(SecondaryKeywordList.Contains(word)){
                return SyntaxType.Keyword2;
            }

            if(BuiltinFunctionList.Contains(word)) {
                return SyntaxType.BuiltinFunction;
            }

            return SyntaxType.None;
        }
    }

    class Tag {
        public TextPointer StartPosition { get; set; }
        public TextPointer EndPosition { get; set; }
        public string Word { get; set; }
        public SyntaxType Type { get; set; }
    }

    enum SyntaxType {
        None,
        Keyword,
        Keyword2,
        BuiltinFunction,
        Comment
    }
}
