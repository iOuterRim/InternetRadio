//using ABI.System;
using InternetRadio.Models;
using InternetRadio.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace InternetRadio
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<RadioStation> Stations { get; set; } = new();

        private RadioStation? _currentStation;
        private string _lastMetadata = string.Empty;
        private DateTime _lastMetadataTime = DateTime.MinValue;
        private DispatcherTimer? _metadataTimer;

        private readonly IcyMetadataService _icyService = new();

        public MainWindow()
        {
            InitializeComponent();

            // use the Dark theme initially
            if (Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Dark;
            }

            // Set custom window icon
            //WindowIconHelper.SetWindowIcon(this, "Assets\\AppIcon.ico");
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            StationsList.ItemsSource = Stations;
            _ = LoadStationsAsync();
        }

        private async Task LoadStationsAsync()
        {
            if (StationNameRun != null)
            {
                StationNameRun.Text = "Loading stations...";
                HyphenRun.Text = "";
                NowPlayingRun.Text = "";
            }

            var service = new Services.RadioBrowserService();
            var stations = await service.GetStationsAsync();

            Stations.Clear();
            foreach (var station in stations)
                Stations.Add(station);

            if (StationNameRun != null)
                StationNameRun.Text = "";
               
        }

        private async Task UpdateNowPlayingAsync()
        {
            if (_currentStation == null)
                return;

            try
            {
                string? meta = await _icyService.GetNowPlayingAsync(_currentStation.Url);

                if (!string.IsNullOrEmpty(meta))
                {
                    _lastMetadata = meta;
                    _lastMetadataTime = DateTime.Now;
                }
                else if ((DateTime.Now - _lastMetadataTime).TotalMinutes > 2)
                {
                    _lastMetadata = string.Empty; // expire stale data
                }

                if (!string.IsNullOrEmpty(_lastMetadata) && !string.IsNullOrEmpty(_currentStation.Name))
                {
                    StationNameRun.Text = $"{_currentStation.Name}";
                    HyphenRun.Text = " - ";
                    NowPlayingRun.Text = $"{_lastMetadata}";
                }
                else
                {
                    if (!string.IsNullOrEmpty(_currentStation.Name))
                    {
                        StationNameRun.Text = $"{_currentStation.Name}";
                        HyphenRun.Text = "";
                        NowPlayingRun.Text = "";
                    }
                }
            }
            catch
            {
                // Silently ignore errors, keep last known text
            }
        }


        private void MediaPlayer_CurrentStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                switch (sender.CurrentState)
                {
                    case Windows.Media.Playback.MediaPlayerState.Playing:
                        // Start or restart the metadata timer
                        _metadataTimer?.Stop();
                        _metadataTimer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(5)
                        };
                        _metadataTimer.Tick += async (s, e) => await UpdateNowPlayingAsync();
                        _metadataTimer.Start();
                        break;

                    case Windows.Media.Playback.MediaPlayerState.Paused:
                    case Windows.Media.Playback.MediaPlayerState.Stopped:
                        // Stop metadata updates when paused or stopped
                        _metadataTimer?.Stop();
                        break;
                }
            });
        }


        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RadioStation station)
            {
                _currentStation = station; // store selected station
                Player.Source = MediaSource.CreateFromUri(new Uri(station.Url));
                Player.MediaPlayer.Play();

                // subscribe to state changes to update metadata when playback starts/pauses/stops
                Player.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;

                // on play button click, the last metadata should be cleared
                _lastMetadata = string.Empty;
                _metadataTimer?.Stop();
                _metadataTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                _metadataTimer.Tick += async (s, ev) => await UpdateNowPlayingAsync();
                _metadataTimer.Start();

                await UpdateNowPlayingAsync(); // first refresh immediately
            }
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (Content is FrameworkElement rootElement)
            {
                // Check current theme and toggle
                if (rootElement.RequestedTheme == ElementTheme.Light)
                {
                    rootElement.RequestedTheme = ElementTheme.Dark;
                }
                else
                {
                    rootElement.RequestedTheme = ElementTheme.Light;
                }
            }
        }
    }
}
