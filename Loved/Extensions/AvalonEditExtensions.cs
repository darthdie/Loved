using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;

namespace Loved {
    public static class AvalonEditExtensions {
        public static string GetWordBeforeDot(this TextEditor textEditor, bool wholeCall = false) {
            var wordBeforeDot = string.Empty;
            var caretPosition = textEditor.CaretOffset - 2;
            var lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(caretPosition));
            string text = textEditor.Document.GetText(lineOffset, 1);

            while(true) {
                if (string.IsNullOrWhiteSpace(text) && (wholeCall || text.CompareTo(".") > 0)) {
                    break;
                }
                if (Regex.IsMatch(text, @".*[^A-Za-z\.]")) {
                    break;
                }

                wordBeforeDot = text + wordBeforeDot;

                if (caretPosition == 0) {
                    break;
                }

                lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(--caretPosition));

                text = textEditor.Document.GetText(lineOffset, 1);
            }

            return wordBeforeDot;
        }

        public static string GetWordBeforeSpace(this TextEditor textEditor) {
            var wordBeforeDot = string.Empty;
            var caretPosition = textEditor.CaretOffset - 2;
            var lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(caretPosition));
            string text = textEditor.Document.GetText(lineOffset, 1);

            while (true) {
                if (text == null && text.CompareTo(" ") > 0) {
                    break;
                }
                if (Regex.IsMatch(text, @".*[^A-Za-z\. ]")) {
                    break;
                }

                if (text != " ") {
                    wordBeforeDot = text + wordBeforeDot;
                }

                if (caretPosition == 0) {
                    break;
                }

                lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(--caretPosition));

                text = textEditor.Document.GetText(lineOffset, 1);
            }

            return wordBeforeDot;
        }
    }
}
