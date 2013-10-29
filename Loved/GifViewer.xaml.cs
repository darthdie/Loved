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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Loved {
    /// <summary>
    /// Interaction logic for GifViewer.xaml
    /// </summary>
    public partial class GifViewer : UserControl {
        private ImageAnimationController imageController;

        public string SourcePath {
            get { return (string)GetValue(SourcePathProperty); }
            set { SetValue(SourcePathProperty, value); }
        }

        public static readonly DependencyProperty SourcePathProperty =
            DependencyProperty.Register("SourcePath", typeof(string), typeof(GifViewer));

        public bool IsRepeating {
            get { return (bool)GetValue(IsRepeatingProperty); }
            set { SetValue(IsRepeatingProperty, value); }
        }

        public static readonly DependencyProperty IsRepeatingProperty =
            DependencyProperty.Register("IsRepeating", typeof(bool), typeof(GifViewer), new PropertyMetadata(false, IsRepeatingOnChanged));

        private static void IsRepeatingOnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var viewer = obj as GifViewer;
            if (viewer != null && viewer.imageController != null) {
                if ((bool)e.NewValue == true) {
                    ImageBehavior.SetRepeatBehavior(viewer.MainImage, RepeatBehavior.Forever);
                }
                else {
                    var frame = viewer.imageController.CurrentFrame;
                    ImageBehavior.SetRepeatBehavior(viewer.MainImage, new RepeatBehavior(1));
                    if (frame != -1) {
                        viewer.imageController.GotoFrame(frame);
                        viewer.imageController.Play();
                    }
                }
            }
        }

        public GifViewer(string source) {
            SourcePath = source;

            InitializeComponent();

            imageController = ImageBehavior.GetAnimationController(MainImage);
        }

        private void OnPlayButtonClicked(object sender, RoutedEventArgs e) {
            imageController = ImageBehavior.GetAnimationController(MainImage);
            if (imageController != null) {
                imageController.Play();
            }
        }

        private void OnPauseButtonClicked(object sender, RoutedEventArgs e) {
            imageController = ImageBehavior.GetAnimationController(MainImage);
            if (imageController != null) {
                imageController.Pause();
            }
        }

        private void OnStopButtonClicked(object sender, RoutedEventArgs e) {
            imageController = ImageBehavior.GetAnimationController(MainImage);
            if (imageController != null) {
                imageController.GotoFrame(imageController.FrameCount - 1);
            }
        }

        private void OnRepeatButtonClicked(object sender, RoutedEventArgs e) {
            IsRepeating = !IsRepeating;
        }
    }
}
