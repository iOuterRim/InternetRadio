using FFMpegCore;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace InternetRadio.Services
{
    public class IcyMetadataResult
    {
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
    }

    public static class IcyMetadataReader
    {
        public static async Task<IcyMetadataResult?> TryGetIcyMetadataAsync(string streamUrl, int timeoutMs = 4000)
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromMilliseconds(timeoutMs)
                };
                client.DefaultRequestHeaders.Add("Icy-MetaData", "1");

                using var response = await client.GetAsync(streamUrl, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                    return null;

                if (!response.Headers.TryGetValues("icy-metaint", out var values))
                    return null;

                if (!int.TryParse(values.FirstOrDefault(), out int metaInt) || metaInt <= 0)
                    return null;

                using var stream = await response.Content.ReadAsStreamAsync();
                var buffer = new byte[metaInt];
                int read = await stream.ReadAsync(buffer.AsMemory(0, metaInt));

                // some streams send metadata earlier than expected, so don’t fail here
                if (read <= 0)
                    return null;

                int metaLen = stream.ReadByte();
                if (metaLen < 0)
                    return null; // end of stream
                if (metaLen == 0)
                    return new IcyMetadataResult(); // no metadata block this time

                var metaBuffer = new byte[metaLen * 16];
                read = await stream.ReadAsync(metaBuffer.AsMemory(0, metaBuffer.Length));

                var metaString = Encoding.UTF8.GetString(metaBuffer);
                
                var result = new IcyMetadataResult();

                // Parse StreamTitle
                var titleStart = metaString.IndexOf("StreamTitle='", StringComparison.OrdinalIgnoreCase);
                if (titleStart >= 0)
                {
                    titleStart += 13;
                    var end = metaString.IndexOf("';", titleStart);
                    if (end > titleStart)
                        result.Title = metaString[titleStart..end].Trim();
                }

                // Parse StreamUrl
                var urlStart = metaString.IndexOf("StreamUrl='", StringComparison.OrdinalIgnoreCase);
                if (urlStart >= 0)
                {
                    urlStart += 11;
                    var end = metaString.IndexOf("';", urlStart);
                    if (end > urlStart)
                        result.ImageUrl = metaString[urlStart..end].Trim();
                }

                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
