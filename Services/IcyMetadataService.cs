using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace InternetRadio.Services
{
    public class IcyMetadataService
    {
        private readonly HttpClient _client;

        public IcyMetadataService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Icy-MetaData", "1");
        }

        public async Task<string?> GetNowPlayingAsync(string streamUrl)
        {
            try
            {
                using var response = await _client.GetAsync(streamUrl, HttpCompletionOption.ResponseHeadersRead);
                if (!response.Headers.TryGetValues("icy-metaint", out var values))
                    return null;

                if (!int.TryParse(values.FirstOrDefault(), out int metaInt) || metaInt <= 0)
                    return null;

                using var stream = await response.Content.ReadAsStreamAsync();

                // skip audio data until metadata block
                byte[] buffer = new byte[metaInt];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead < metaInt)
                    return null;

                // next byte is metadata length (in blocks of 16 bytes)
                int metadataLength = stream.ReadByte() * 16;
                if (metadataLength <= 0) return null;

                buffer = new byte[metadataLength];
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead < metadataLength)
                    return null;

                string metadata = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                // e.g. "StreamTitle='Artist - Song';"
                return ParseStreamTitle(metadata);
            }
            catch
            {
                return null;
            }
        }

        private string? ParseStreamTitle(string metadata)
        {
            // look for "StreamTitle='...';"
            var start = metadata.IndexOf("StreamTitle='", StringComparison.OrdinalIgnoreCase);
            if (start >= 0)
            {
                start += "StreamTitle='".Length;
                var end = metadata.IndexOf("';", start);
                if (end > start)
                {
                    return metadata.Substring(start, end - start);
                }
            }
            return null;
        }
    }
}
