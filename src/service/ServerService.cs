using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Wallcat.service
{
    public class Channel
    {
        public string id { get; set; }
        public string title { get; set; }
        public bool isDefault { get; set; }
    }

    public class Wallpaper
    {
        public string id { get; set; }
        public string title { get; set; }

        public WallpaperPartner partner { get; set; }
        public WallpaperChannel channel { get; set; }
        public WallpaperUrls url { get; set; }

        public string sourceUrl { get; set; }
    }

    public class WallpaperPartner
    {
        public string id { get; set; }
        public string first { get; set; }
        public string last { get; set; }

        public string name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(last))
                    return first;

                return $"{first} {last}";
            }
        }
    }

    public class WallpaperChannel
    {
        public string id { get; set; }
    }

    public class WallpaperUrls
    {
        public string o { get; set; }
    }
    
    public class Server
    {
        private const string ApiHost = @"http://beta.wall.cat/api/v1";

        private static readonly HttpClient Client = new HttpClient();

        public static Channel[] GetDefaultChannels()
        {
            const string jsonstr = "{\"success\":true,\"payload\":[{\"id\":\"RX1BtSlCrq\",\"title\":\"Structure\"," +
                                   "\"description\":\"\",\"url\":\"\",\"isDefault\":true},{\"id\":\"ea4KhWHUGE\"," +
                                   "\"title\":\"Fresh Air\",\"description\":\"\",\"url\":\"\",\"isDefault\":false}," +
                                   "{\"id\":\"ygmTdEW7aF\",\"title\":\"Gradients\",\"description\":\"\",\"url\":\"\"," +
                                   "\"isDefault\":false},{\"id\":\"tdA5aG8zpa\",\"title\":\"Northern Perspective\"," +
                                   "\"description\":\"\",\"url\":\"\",\"isDefault\":false}],\"apiVersion\":1," +
                                   "\"location\":\"http://beta.wall.cat/api/v1/channels\"}";
            return new JavaScriptSerializer().Deserialize<ChannelResponse>(jsonstr).payload;
        }

        public async Task<Channel[]> GetChannels()
        {
            var response = await Client.GetAsync($"{ApiHost}/channels");
            response.EnsureSuccessStatusCode();
            return new JavaScriptSerializer().Deserialize<ChannelResponse>(await response.Content.ReadAsStringAsync()).payload;
        }

        public async Task<Wallpaper> GetWallpaper(string channelId)
        {
            var response = await Client.GetAsync($"{ApiHost}/channels/{channelId}/image/{DateTime.Now:yyyy-MM-dd}T00:00:00.000Z");
            response.EnsureSuccessStatusCode();
            return new JavaScriptSerializer().Deserialize<WallpaperResponse>(await response.Content.ReadAsStringAsync()).payload.image;
        }
        
        public void WallpaperSourceWebpage(Wallpaper wallpaper)
        {
            const string campaign = "?utm_source=windows&utm_medium=menuItem&utm_campaign=wallcat";
            Process.Start(wallpaper.sourceUrl + campaign);
        }
    }

    internal class ChannelResponse
    {
        public bool success { get; set; }
        public Channel[] payload { get; set; }
    }

    internal class WallpaperResponse
    {
        public bool success { get; set; }
        public WallpaperImageResponse payload { get; set; }
    }

    internal class WallpaperImageResponse
    {
        public Wallpaper image { get; set; }
    }
}