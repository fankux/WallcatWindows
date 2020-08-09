using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wallcat.Properties;
using Wallcat.service;
using Wallcat.Util;

namespace Wallcat.fe
{
    public class Entry : ApplicationContext
    {
        private static readonly ContextMenu Cmenu = new ContextMenu();

        private NotifyIcon _icon = new NotifyIcon
        {
            Icon = Resources.AppIcon,
            ContextMenu = Cmenu,
            Visible = true
        };

        private MenuItem _currentText = new MenuItem("Current") {Enabled = false};
        private MenuItem _current = new MenuItem("Loading...");

        private MenuItem _channelText = new MenuItem("Channels") {Enabled = false};
        private List<KeyValuePair<string, MenuItem>> _channelItems = new List<KeyValuePair<string, MenuItem>>();

        private List<MenuItem> _options = null;

        private SystemService _system = new SystemService();
        private Server _server = new Server();
        private ChannelService _channel = new ChannelService();
        private Settings _state = Settings.Default;

        public Entry()
        {
            Application.ApplicationExit += OnApplicationExit;

            if (InitUi() && _state.LastChecked != DateTime.Now)
            {
                SelectChannel(_state.CurrentChannel);
            }

            _system.MidnightUpdate(x => { SelectChannel(_state.CurrentChannel); });
        }

        private bool InitUi()
        {
            bool ret;
            using (new IconAnimation(ref _icon))
            {
                ret = InitChannelItems();
                InitOptionItems();
                LoadState();
                RefreshMenu();
            }

            return ret;
        }

        private bool InitChannelItems()
        {
            _channelItems.Clear();

            var serverChannels = _channel.InitChannelsOnce();
            if (serverChannels == null)
            {
                const string menuKey = "Failed to fetch Channels, retrying...";
                _channelItems.Add(new KeyValuePair<string, MenuItem>("failed",
                    new MenuItem(menuKey) {Enabled = false, Name = menuKey}));

                // async task to update 
                Task.Run(() =>
                {
                    if (!InitChannelItems())
                    {
                        return;
                    }

                    RefreshMenu();
                    SelectChannel(_state.CurrentChannel);
                });
                return false;
            }

            if (serverChannels.Length == 0)
            {
                _channelItems.Add(new KeyValuePair<string, MenuItem>("NoChannel", new MenuItem("No Channel")));
                return true;
            }

            foreach (var channel in serverChannels)
            {
                _channelItems.Add(new KeyValuePair<string, MenuItem>(channel.id,
                    new MenuItem(channel.title, (sender, e) => { SelectChannel((Channel) ((MenuItem) sender).Tag); })
                        {Tag = channel, Checked = false}));
            }

            if (_state.CurrentChannel != null)
            {
                return true;
            }

            // set default channel. first use
            _state.CurrentChannel = serverChannels.First();
            _icon.ShowBalloonTip(10 * 1000, "Welcome to Wallcat",
                $"Enjoy the {_state.CurrentChannel.title} channel!",
                ToolTipIcon.Info);
            return true;
        }

        private void InitOptionItems()
        {
            if (string.IsNullOrEmpty(_state.SavePath))
            {
                _state.SavePath = Path.GetDirectoryName(Path.GetTempFileName());
            }
            
            _options = new List<MenuItem>();
            _options.AddRange(new[]
            {
                new MenuItem("Start at login", this.OnSelectStartup)
                    {Checked = SystemService.IsEnabledAtStartup()},
                new MenuItem(_state.SavePath, OnSelectSavePath),
                new MenuItem("-") {Enabled = false},
                new MenuItem("Quit Wallcat", (sender, args) => Application.Exit())
            });
        }

        private void LoadState()
        {
            if (_state.CurrentChannel != null && _channelItems != null)
            {
                foreach (var channel in _channelItems)
                {
                    channel.Value.Checked = channel.Key == _state.CurrentChannel.id;
                }
            }

            if (_state.CurrentWallpaper != null)
            {
                _current = new MenuItem(_state.CurrentWallpaper.title,
                    (sender, args) => _server.WallpaperSourceWebpage(_state.CurrentWallpaper));
            }
        }

        private void RefreshMenu()
        {
            Cmenu.MenuItems.Clear();

            // current
            Cmenu.MenuItems.Add(_currentText);
            Cmenu.MenuItems.Add(_current);

            // channels
            Cmenu.MenuItems.Add(new MenuItem("-") {Enabled = false});
            Cmenu.MenuItems.Add(_channelText);
            if (_channelItems != null)
            {
                Cmenu.MenuItems.AddRange(_channelItems.Select(channel => channel.Value).ToArray());
            }

            // options
            Cmenu.MenuItems.Add(new MenuItem("-") {Enabled = false});
            if (_options != null)
            {
                Cmenu.MenuItems.AddRange(_options.ToArray());
            }
        }

        private async void SelectChannel(Channel channel)
        {
            using (new IconAnimation(ref _icon))
            {
                var wallpaper = await _server.GetWallpaper(channel.id);
                if (wallpaper.id == _state.CurrentWallpaper?.id)
                {
                    return;
                }

                var filePath = await DownloadFile.Get(wallpaper.url.o, _state.SavePath + "/" + wallpaper.title);
                SystemService.UpdateSystemWallpaper(filePath);

                // Update Settings
                Settings.Default.CurrentChannel = channel;
                Settings.Default.CurrentWallpaper = wallpaper;
                Settings.Default.LastChecked = DateTime.Now.Date;
                Settings.Default.Save();
                _state = Settings.Default;

                LoadState();
                RefreshMenu();
            }
        }

        private void OnSelectSavePath(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog {Description = @"Choose Directory"};
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Settings.Default.SavePath = dialog.SelectedPath;
            Settings.Default.Save();
            _state = Settings.Default;

            ((MenuItem) sender).Text = _state.SavePath;
        }

        private void OnSelectStartup(object sender, EventArgs e)
        {
            SystemService.CreateStartupShortcut();
            ((MenuItem) sender).Checked = SystemService.IsEnabledAtStartup();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _icon.Visible = false;
        }
    }
}