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
using System.Windows.Threading;

namespace Loved {
    /// <summary>
    /// Interaction logic for MediaPlayer.xaml
    /// </summary>
    public partial class MediaPlayer : UserControl {
        private bool isDragging;
        private DispatcherTimer seekTimer;

        public bool IsPlaying {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(MediaPlayer), new PropertyMetadata(false));

        public string MediaPath {
            get { return (string)GetValue(MediaPathProperty); }
            set { SetValue(MediaPathProperty, value); }
        }

        public static readonly DependencyProperty MediaPathProperty =
            DependencyProperty.Register("MediaPath", typeof(string), typeof(MediaPlayer), new PropertyMetadata(""));

        public MediaPlayer(string mediaPath) {
            isDragging = false;

            seekTimer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            seekTimer.Tick += delegate {
                if (!isDragging) {
                    UpdateSeekPosition();
                }
            };

            MediaPath = mediaPath;

            InitializeComponent();
        }

        private void UpdateSeekPosition() {
            MainPositionSlider.Value = MainMediaElement.Position.TotalSeconds;
        }

        private void OnPlayPauseButtonClicked(object sender, RoutedEventArgs e) {
            if (IsPlaying) {
                PauseMedia();
            }
            else {
                PlayMedia();
            }
        }

        private void PauseMedia() {
            MainMediaElement.Pause();
            IsPlaying = false;
            seekTimer.Stop();
        }

        private void PlayMedia() {
            MainMediaElement.Play();
            IsPlaying = true;
            seekTimer.Start();
        }

        private void OnStopButtonClicked(object sender, RoutedEventArgs e) {
            if (IsPlaying) {
                StopMedia();
            }
        }

        private void StopMedia() {
            MainMediaElement.Stop();
            IsPlaying = false;
            seekTimer.Stop();
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e) {
            StopMedia();

            MainPositionSlider.Value = 0;
        }

        private void OnMediaOpened(object sender, RoutedEventArgs e) {
            if (MainMediaElement.NaturalDuration.HasTimeSpan) {
                var timeSpan = MainMediaElement.NaturalDuration.TimeSpan;
                MainPositionSlider.Maximum = timeSpan.TotalSeconds;
                if (timeSpan.TotalSeconds > 5) {
                    MainPositionSlider.SmallChange = 1;
                    MainPositionSlider.LargeChange = Math.Min(10, timeSpan.TotalSeconds / 10);
                }
                else {
                    MainPositionSlider.SmallChange = 0.1;
                    MainPositionSlider.LargeChange = 0.5;
                }
            }
        }

        private void OnSliderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            isDragging = true;
        }

        private void OnSliderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            isDragging = false;
        }

        private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            MainMediaElement.Position = TimeSpan.FromSeconds(MainPositionSlider.Value);
        }
    }
}
