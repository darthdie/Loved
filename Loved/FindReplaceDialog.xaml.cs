using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Loved {
    /// <summary>
    /// Interaction logic for FindReplaceDialog.xaml
    /// </summary>
    public partial class FindReplaceDialog : Window {
        //public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event SearchSubmittedEventHandler SearchSubmitted;
        public event ReplaceSubmittedEventHandler ReplaceSubmitted;

        public bool IsSearch {
            get { return (bool)GetValue(IsSearchProperty); }
            set { SetValue(IsSearchProperty, value); }
        }

        public static readonly DependencyProperty IsSearchProperty =
            DependencyProperty.Register("IsSearch", typeof(bool), typeof(FindReplaceDialog),new PropertyMetadata(true, OnIsSearchChanged));

        private static void OnIsSearchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            /*var dlg = d as FindReplaceDialog;
            if (((bool)e.NewValue) == true) {
                dlg.MainTabControl.SelectedItem = dlg.FindTabItem;
            }
            else {
                dlg.MainTabControl.SelectedItem = dlg.ReplaceTabItem;
            }*/
        }

        public FindSources SelectedFindSource {
            get { return (FindSources)GetValue(SelectedFindSourceProperty); }
            set { SetValue(SelectedFindSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedFindSourceProperty =
            DependencyProperty.Register("SelectedFindSource", typeof(FindSources), typeof(FindReplaceDialog), new PropertyMetadata(FindSources.Project));

        public string SearchText {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(FindReplaceDialog));

        public string ReplaceText {
            get { return (string)GetValue(ReplaceTextProperty); }
            set { SetValue(ReplaceTextProperty, value); }
        }

        public static readonly DependencyProperty ReplaceTextProperty =
            DependencyProperty.Register("ReplaceText", typeof(string), typeof(FindReplaceDialog));

        public FindReplaceDialog() {
            InitializeComponent();

            IsVisibleChanged += delegate {
                /*if (IsVisible) {
                    if (IsSearch) {
                        SearchTextbox.Focus();
                    }
                    else {
                        SearchReplaceTextbox.Focus();
                    }
                }*/
            };
        }

        public void ShowSearch() {
            MainTabControl.SelectedItem = FindTabItem;
            if (!IsVisible) {
                Show();
            }

            SearchTextbox.Focus();

            BringIntoView();
            Focus();
        }

        public void ShowReplace() {
            MainTabControl.SelectedItem = ReplaceTabItem;
            if (!IsVisible) {
                Show();
            }

            SearchReplaceTextbox.Focus();

            BringIntoView();
            Focus();
        }

        private void OnFindAllButtonClicked(object sender, RoutedEventArgs e) {
            if (SearchSubmitted != null) {
                SearchSubmitted(this, new SearchSubmitEventArgs(SearchText, SelectedFindSource));
            }

            Hide();
        }

        private void OnDialogClosing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        private void OnReplaceAllButtonClicked(object sender, RoutedEventArgs e) {
            if (ReplaceSubmitted != null) {
                ReplaceSubmitted(this, new ReplaceSubmitEventArgs(SearchText, ReplaceText, SelectedFindSource));
            }

            Hide();
        }
    }

    public class SearchSubmitEventArgs : EventArgs {
        public string Search { get; set; }
        public FindSources FindSource { get; set; }
        public bool IgnoreCase { get; set; }

        public SearchSubmitEventArgs(string search, FindSources source, bool ignoreCase = true) {
            Search = search;
            FindSource = source;
            IgnoreCase = ignoreCase;
        }
    }

    public delegate void SearchSubmittedEventHandler(object sender, SearchSubmitEventArgs e);

    public class ReplaceSubmitEventArgs : EventArgs {
        public string Search { get; set; }
        public string Replace { get; set; }
        public FindSources FindSource { get; set; }
        public bool IgnoreCase { get; set; }

        public ReplaceSubmitEventArgs(string search, string replace, FindSources source, bool ignoreCase = true) {
            Search = search;
            Replace = replace;
            FindSource = source;
            IgnoreCase = ignoreCase;
        }
    }

    public delegate void ReplaceSubmittedEventHandler(object sender, ReplaceSubmitEventArgs e);

    [TypeConverter(typeof(EnumTypeConverter))]
    public enum FindSources {
        [EnumDisplayName("Project")]
        Project,
        [EnumDisplayName("Current File")]
        CurrentFile
    }
}
