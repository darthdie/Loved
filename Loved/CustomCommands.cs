using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Loved {
    public static class CustomCommands {
        public static readonly RoutedUICommand CompileCommand = new RoutedUICommand("Compile the project", "Compile", typeof(CustomCommands), new InputGestureCollection { new KeyGesture(Key.F6) });
        public static readonly RoutedUICommand RunCommand = new RoutedUICommand("Run the project", "Run", typeof(CustomCommands), new InputGestureCollection { new KeyGesture(Key.F5) });
        public static readonly RoutedUICommand SaveProject = new RoutedUICommand("Save the project", "Save Project", typeof(CustomCommands), new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift) });
        public static readonly RoutedUICommand StopCommand = new RoutedUICommand("Stop the project", "Stop", typeof(CustomCommands), new InputGestureCollection { new KeyGesture(Key.F5, ModifierKeys.Shift) });
        public static readonly RoutedUICommand SearchFilesCommand = new RoutedUICommand("Search Files", "Search", typeof(CustomCommands), new InputGestureCollection { new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Shift) });
        public static readonly RoutedUICommand ReplaceFilesCommand = new RoutedUICommand("Replace Files", "Replace", typeof(CustomCommands), new InputGestureCollection { new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift) });
        public static readonly RoutedUICommand GotoLineCommand = new RoutedUICommand("Goto line", "Goto", typeof(CustomCommands), new InputGestureCollection { new KeyGesture(Key.G, ModifierKeys.Control) });
        public static readonly RoutedUICommand OpenPreferencesCommand = new RoutedUICommand("Open Preferences", "Preferences", typeof(CustomCommands));
        public static readonly RoutedUICommand OpenFileCommand = new RoutedUICommand("Open File", "OpenFile", typeof(CustomCommands));
        public static readonly RoutedUICommand AddFileCommand = new RoutedUICommand("Add File", "AddFile", typeof(CustomCommands));
        public static readonly RoutedUICommand DeleteFileCommand = new RoutedUICommand("Delete File", "DeleteFile", typeof(CustomCommands));
        public static readonly RoutedUICommand OpenStackTraceFileCommand = new RoutedUICommand("Open Stack Trace Item", "OpenStackTrace", typeof(CustomCommands));
        public static readonly RoutedUICommand AddFolderCommand = new RoutedUICommand("Add Folder", "AddFolder", typeof(CustomCommands));
    }
}
