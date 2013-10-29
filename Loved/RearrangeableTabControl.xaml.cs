using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Loved {
    /// <summary>
    /// Interaction logic for RearrangeableTabControl.xaml
    /// </summary>
    public partial class RearrangeableTabControl : TabControl {
        public RearrangeableTabControl() {
            InitializeComponent();
        }

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
        }
    }
}
