using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Wallcat.Services
{
    public class WallcatService : IWallcatService
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