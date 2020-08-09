using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Wallcat.Properties;

namespace Wallcat.Util
{
    public static class DownloadFile
    {
        public static async Task<string> Get(string url, string filePath)
        {
#if DEBUG
            return url;
#endif
            await new WebClient().DownloadFileTaskAsync(new Uri(url), filePath);
            return filePath;
        }
    }
}