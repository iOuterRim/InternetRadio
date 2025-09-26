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
            using var client = new HttpClient();
            var response = await client.GetStringAsync("https://de1.api.radio-browser.info/json/stations/topclick/10");
            var stations = JsonSerializer.Deserialize<RadioStation[]>(response);
            if (stations != null)
            {
                foreach (var station in stations)
                    Stations.Add(station);
            }
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
