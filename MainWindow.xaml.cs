using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Media.Core;
using InternetRadio.Models;


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
            if (StatusPane != null)
                StatusPane.Text = "Loading stations...";

            var service = new Services.RadioBrowserService();
            var stations = await service.GetStationsAsync();

            Stations.Clear();
            foreach (var station in stations)
                Stations.Add(station);

            if (StatusPane != null)
                StatusPane.Text = "";
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

            // update SearchBox with the station name
            if (button.DataContext is RadioStation station)
            {
                StatusPane.Text = $"Now Playing: {station.Name}";
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
