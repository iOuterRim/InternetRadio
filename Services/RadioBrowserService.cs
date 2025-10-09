using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using InternetRadio.Models;
using DnsHttpCheckerLib;

namespace InternetRadio.Services
{
    public class RadioBrowserService
    {
        private readonly string _defaultApi = "https://de1.api.radio-browser.info";
        private readonly string _stationsPath = "/json/stations/topclick/10";

        // fallback list if API fails
        private readonly List<RadioStation> _fallbackStations = new()
        {
            new RadioStation { Name = "BBC World Service", Url = "http://stream.live.vc.bbcmedia.co.uk/bbc_world_service" },
            new RadioStation { Name = "NPR News", Url = "https://npr-ice.streamguys1.com/live.mp3" },
            new RadioStation { Name = "Classic FM (UK)", Url = "https://media-ice.musicradio.com/ClassicFMMP3" },
            new RadioStation { Name = "1.FM - Adore Jazz Radio (CH)", Url = "http://strm112.1.fm/ajazz_mobile_mp3" },
            new RadioStation { Name = "1.FM - Chillout Lounge Radio (CH)", Url = "http://strm112.1.fm/chilloutlounge_mobile_mp3" },
            new RadioStation { Name = "1.FM - Radio Gaia (CH)", Url = "http://strm112.1.fm/radiogaia_mobile_mp3" }, 
            new RadioStation { Name = "Radio Paradise Mellow Mix 320k AAC", Url = "http://stream.radioparadise.com/mellow-320" },
            new RadioStation { Name = "Deutschlandfunk", Url = "https://st01.sslstream.dlf.de/dlf/01/128/mp3/stream.mp3" },
            new RadioStation { Name = "Soma FM - Left Coast 70s", Url = "https://ice6.somafm.com/seventies-320-mp3" },
            new RadioStation { Name = "Klassik Radio - Live", Url = "https://live.streams.klassikradio.de/klassikradio-deutschland/stream/mp3" },
        };

        public async Task<List<RadioStation>> GetStationsAsync()
        {
            var stations = new List<RadioStation>();

            try
            {
                string stationsUrl = _defaultApi + _stationsPath;

                // try DNS-based server discovery
                var checker = new DnsHttpChecker("all.api.radio-browser.info");
                var results = await checker.CheckAllAsync();
                foreach (var result in results)
                {
                    if (result.StatusCode == "200" && !string.IsNullOrEmpty(result.Url))
                    {
                        stationsUrl = result.Url + _stationsPath;
                        break;
                    }
                }

                using var client = new HttpClient();
                var response = await client.GetStringAsync(stationsUrl);
                var requested_stations = JsonSerializer.Deserialize<List<RadioStation>>(response);

                if (requested_stations != null && requested_stations.Count > 0)
                {
                    foreach (var station in requested_stations)
                    {
                        // basic validation
                        if (!string.IsNullOrEmpty(station.Name) && !string.IsNullOrEmpty(station.Url))
                        {
                            stations.Add(station);
                        }
                    }
                }
            }
            catch
            {
                // swallow and fallback
            }

            // handle the _fallbackStations as fixed additional stations here
            foreach (var station in _fallbackStations)
                stations.Add(station);

            return stations;
        }
    }
}
