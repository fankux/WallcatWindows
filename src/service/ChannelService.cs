using System;
using System.Threading;
using System.Windows.Forms;

namespace Wallcat.service
{
    public class ChannelService
    {
        public Channel[] InitChannelsOnce()
        {
            try
            {
                var channels = Server.GetDefaultChannels();
                return channels;
            }
            catch (Exception e)
            {
#if DEBUG
                MessageBox.Show(e.ToString(), @"Failed to init Channels!");
#endif
            }

            return null;
        }

        public Channel[] InitChannels()
        {
            do
            {
                var channels = InitChannelsOnce();
                if (channels != null)
                {
                    return channels;
                }

                Thread.Sleep(1000);
            } while (true);
        }
    }
}