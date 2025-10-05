using DnsHttpCheckerLib;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using static System.Net.WebRequestMethods;

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

        private readonly List<RadioStation> FallbackStations = new()
        {
            new RadioStation { Name = "BBC World Service", Url = "http://stream.live.vc.bbcmedia.co.uk/bbc_world_service" },
            new RadioStation { Name = "NPR News", Url = "https://npr-ice.streamguys1.com/live.mp3" },
            new RadioStation { Name = "Classic FM (UK)", Url = "https://media-ice.musicradio.com/ClassicFMMP3" },
            new RadioStation { Name = "Jazz24 (Seattle)", Url = "https://live.wostreaming.net/direct/ppm-jazz24mp3-ibc1" },
            new RadioStation { Name = "Radio Paradise (Main)", Url = "http://stream.radioparadise.com/aac-320" },
            new RadioStation { Name = "Deutschlandfunk", Url = "https://st01.sslstream.dlf.de/dlf/01/128/mp3/stream.mp3" },
            new RadioStation { Name = "Soma FM - Left Coast 70s", Url = "https://ice6.somafm.com/seventies-320-mp3" },
        };


        public MainWindow()
        {
            InitializeComponent();

            // use the light theme initially
            if (Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Light;
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
            string stations_url_search_part = "/json/stations/topclick/10";
            // stations url with fallback to a specific server
            string stations_url = "https://de1.api.radio-browser.info" + stations_url_search_part;

            try
            {
                var checker = new DnsHttpChecker("all.api.radio-browser.info");
                var results = await checker.CheckAllAsync();

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        if (result.StatusCode == "200" && !string.IsNullOrEmpty(result.Url))
                        {
                            stations_url = result.Url + stations_url_search_part;
                            break;
                        }                           
                    }
                }
                

                using var client = new HttpClient();
                var response = await client.GetStringAsync(stations_url);
                var stations = JsonSerializer.Deserialize<RadioStation[]>(response);
                if (stations != null)
                {
                    foreach (var station in stations)
                        Stations.Add(station);
                }
            }

            catch
            {
                // Ignore and fall back
            }

            // Fallback if API fails
            foreach (var station in FallbackStations)
                Stations.Add(station);

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var streamUrl = button.Tag?.ToString();
            if (!string.IsNullOrEmpty(streamUrl))
            {
                var mediaSource = MediaSource.CreateFromUri(new Uri(streamUrl));
                Player.Source = mediaSource;
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


    public class RadioStation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url_resolved")]
        public string Url { get; set; } = string.Empty;
    }


}
