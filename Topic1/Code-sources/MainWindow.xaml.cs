using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using REMPv2.Collection.Music;
using WinInterop = System.Windows.Interop;

namespace REMPv2 {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Manager _musicManager;
        private AddFilesToPlaylist _addFilesToplaylist;
        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        private double _totalseconds;
        private double _currentseconds;
        private bool _fetchingResults;

        public MainWindow() {
            InitializeComponent();

            this.SourceInitialized += new EventHandler(win_SourceInitialized);
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(250);
            timer.Start();

            // when playlst box is loaded, link it with the items of my playlist
            PlaylistBox.Loaded += new RoutedEventHandler(playlistbox_Loaded);

            _musicManager = new Manager();

            TextBox info = new TextBox();
            info.Text = "hi,\n\nyou're probably wondering how this program works,\nnow let me tell you:\n\nBefore we start you will have to have a collection of songs\nwith extra tags (my database isn't large enough yet), or you can\nuse the included folder with music (skip to 'Preparing recommends').\n\n---=[Adding tags]=---\nso there fore you'll have to run the AddTags program, but configure first in\nprogram.cs the line: where it says : 'set here your location'\n\n let that program run untill it's done\n\n---=[Preparing recommends]=---\nNow you add a folder to your collection, on the right press 'add file'\nthen click on 'add folder', select your folder with music.\n\nthen on the right of that screen you'll see the songs that are in that folder showing up.\nselect some songs you want to add to your initial playlist (this won't effect the results)\nyou can add multiple by doing CTRL + clicking them, and then pressing add songs\nor by dubbel clicking on 1 song.\n\nwhen your happy with the playlist close the add song screen.\n(when you dubbel click an song in your playlist it will start playing)\n\n---=[getting recommends]=---\n when the button 'get recommends' becomes active, you can click it.\nthis wil freeze the program (sorry about that) for a while, because it's fetching your\nrecommendations\n\nthen you can click on an artist and the songs of the artist will be shown,\ndubbel clicking one, will add it to your playlist.\n\n---=[final]=---\nhappy listening\n\n";
            recommendationList.Children.Add(info);
        }

        /// <summary>
        /// Makes the screen draggable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dragging(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                DragMove();
            }
        }

        /// <summary>
        /// Close the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close(object sender, MouseButtonEventArgs e) {
            _musicManager.Dispose();
            System.Environment.Exit(0);
        }

        /// <summary>
        /// Toggles between fullscreen & normal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Maximize(object sender, MouseButtonEventArgs e) {

            // set window to maximized window (fixed for taskbar)
            this.WindowState = (this.WindowState == System.Windows.WindowState.Maximized) ? System.Windows.WindowState.Normal : this.WindowState = System.Windows.WindowState.Maximized;
        }

        /// <summary>
        /// Minimalizes the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minimize(object sender, MouseButtonEventArgs e) {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void LoginClick(object sender, RoutedEventArgs e) {

            //TODO: replace button by loading image
            Login(LoginUsername.Text, LoginPassword.Password);
        }

        /// <summary>
        /// Log in,
        ///
        /// still no DB though
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        private void Login(String username, String password) {
            if (username != "" && password != "") {
                UsernameLabel.Content = username;
                Uri uriImage;

                // check if user has an online version, else use default
                if (ImageExcists("http://srv.latomme-g.be:8080/REMP/ProfilePics/" + username + ".png")) {
                    uriImage = new Uri("http://srv.latomme-g.be:8080/REMP/ProfilePics/" + username + ".png");
                } else {
                    uriImage = new Uri(@"\Images\NoProfilePic.png", UriKind.Relative);
                }

                // create image
                UserPicture.Source = new BitmapImage(uriImage);

                // Hide login form
                LoginUsername.Visibility = Visibility.Hidden;
                LoginPassword.Visibility = Visibility.Hidden;
                LoginUsernameLabel.Visibility = Visibility.Hidden;
                LoginPasswordLabel.Visibility = Visibility.Hidden;
                LoginButton.Visibility = Visibility.Hidden;

                // Show logged in stuff
                UsernameLabel.Visibility = Visibility.Visible;
                UserPicture.Visibility = Visibility.Visible;
            } else {
                LoginUsernameLabel.Foreground = (username == "") ? Brushes.Red : Brushes.Black;
                LoginPasswordLabel.Foreground = (password == "") ? Brushes.Red : Brushes.Black;
            }
        }

        /// <summary>
        /// Check if an image excits by checkinf if there is a response on the header of the page
        /// </summary>
        /// <param name="url">The http:// location of the image</param>
        /// <returns>True = excitsts, false = unavailible</returns>
        private bool ImageExcists(String url) {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "HEAD";
            try {
                request.GetResponse();
                return true;
            } catch {
                return false;
            }
        }

        private void StopSong(Object sender, RoutedEventArgs e) {
            _musicManager.Stop();
        }

        private void PauseSong(Object sender, RoutedEventArgs e) {
            _musicManager.Pause();
            PlayButton.Visibility = Visibility.Visible;
            PauseButton.Visibility = Visibility.Hidden;
        }

        private void PlaySong(Object sender, RoutedEventArgs e) {
            _musicManager.Play();

            updateSongThings();
        }

        private void NextSong(Object sender, RoutedEventArgs e) {
            _musicManager.Stop();
            _musicManager.nextSong();
            _musicManager.Play();

            updateSongThings();
        }

        private void PreviousSong(Object sender, RoutedEventArgs e) {
            _musicManager.Stop();
            _musicManager.previousSong();
            _musicManager.Play();

            updateSongThings();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            LabelCurrentTime.Content = _musicManager.getPlayTime();
            _totalseconds = _musicManager.totalSeconds();
            _currentseconds = _musicManager.currentSeconds();
            if (_totalseconds > 0 && _currentseconds > 0) {
                Trackbar.Value = (_currentseconds / _totalseconds) * (Trackbar.Maximum - Trackbar.Minimum) + Trackbar.Minimum;
            }
            if (_currentseconds != -1 && _totalseconds != -1) {
                if (_currentseconds == _totalseconds) {
                    _musicManager.Stop();
                    _musicManager.nextSong();
                    _musicManager.Play();

                    updateSongThings();
                }
            }
            if (_fetchingResults == false) {
                if (_musicManager.IsScanning) {
                    RecommendationButton.IsEnabled = false;
                    RecommendationButton.Content = "Scanning ...";
                } else {
                    RecommendationButton.IsEnabled = true;
                    RecommendationButton.Content = "Get recommends";

                }
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e) {
            var mainborder = sender as Border;
            double x = e.GetPosition(mainborder).X;
            double val = 1 - (mainborder.ActualWidth - x) / mainborder.ActualWidth;
            Trackbar.Value = val * (Trackbar.Maximum - Trackbar.Minimum) + Trackbar.Minimum;
            set_playTimer(val);
        }

        private void set_playTimer(double val) {
            double seconds = val * _totalseconds;
            _musicManager.setPlayTime(TimeSpan.FromSeconds(seconds));
        }

        private void playlistbox_Loaded(object sender, RoutedEventArgs e) {
            PlaylistBox.ItemsSource = _musicManager.PlayList;
        }

        private void DoubleClickitem(object sender, MouseButtonEventArgs e) {
            _musicManager.load(PlaylistBox.SelectedIndex);
            _musicManager.Play();
            updateSongThings();
        }

        private void updateSongThings() {

            // reste the trackbar
            Trackbar.Value = 0;

            // refresh the playlist ( a ">" is in front of the current playing song)
            PlaylistBox.Items.Refresh();

            // total time setting
            LabelTotalTime.Content = _musicManager.getTotalPlayTime();

            // title of the song on top of the trackar
            TrackInfo.Content = _musicManager.GetSongInfo();

            // play / pause buttons
            togglePlayPauseButtons(false);
        }

        private void togglePlayPauseButtons(bool showPlay) {

            // togglle visibility of the buttons
            PlayButton.Visibility = showPlay ? Visibility.Visible : Visibility.Hidden;
            PauseButton.Visibility = showPlay ? Visibility.Hidden : Visibility.Visible;
        }


        private void GetRecommends(object sender, RoutedEventArgs e) {
            
            // make sure there is an song to get the recommends from
            if (_musicManager.CurrentSong != null) {
                List<Expander> expanders = new List<Expander>();
                _fetchingResults = true;
                RecommendationButton.IsEnabled = false;
                RecommendationButton.Content = "Fetching results";

                List<Artist> recommends = _musicManager.GetRecommends(_musicManager.MusicCollection.getArtist(_musicManager.CurrentSong), 10);


                // reset the list that is shown
                recommendationList.Children.Clear();
                for (int i = 0; i < recommends.Count; i++) {

                    // create an expander and set it properties
                    Expander artistExpander = new Expander();
                    artistExpander.Header = recommends[i].Name;
                    artistExpander.HorizontalAlignment = HorizontalAlignment.Left;
                    artistExpander.IsExpanded = false;
                    artistExpander.Padding = new Thickness(0, 0, 0, 10);
                    artistExpander.Width = recommendationList.Width;

                    // populate each expander with the songs of that artist
                    ListView songs = new ListView();
                    songs.BorderThickness = new Thickness(0);
                    songs.Width = recommendationList.Width;
                    songs.ItemsSource = recommends[i].Songs;
                    songs.MouseDoubleClick += addToSongToPlaylist;

                    // give the expander the list
                    artistExpander.Content = songs;

                    // add the expander to the stackpanel in mainview
                    recommendationList.Children.Add(artistExpander);
                }
                _fetchingResults = false;
            }
        }

        private void addToSongToPlaylist(object sender, RoutedEventArgs e) {
            foreach (Song song in ((ListView)sender).SelectedItems)
                _musicManager.AddSong(song);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            _addFilesToplaylist = new AddFilesToPlaylist(this, _musicManager.MusicCollection);

            _addFilesToplaylist.Show();
        }

        public void addFileToPlaylist(Song record) {
            _musicManager.AddSong(record);
        }

        public void addFolder() {
            _musicManager.AddMusicFolder();
            LoadingScreen.Visibility = Visibility.Visible;

            _addFilesToplaylist.MusicCollection = _musicManager.MusicCollection;

            LoadingScreen.Visibility = Visibility.Hidden;
        }

        // ************************
        // Win32 Code for fixing maximize screen 'n stuff
        //
        // Source: http://blogs.msdn.com/b/llobo/archive/2006/08/01/maximizing-window-_2800_with-windowstyle_3d00_none_2900_-considering-taskbar.aspx
        // ************************

        #region win 32 code

        private void win_SourceInitialized(object sender, EventArgs e) {
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        }

        private static System.IntPtr WindowProc(
              System.IntPtr hwnd,
              int msg,
              System.IntPtr wParam,
              System.IntPtr lParam,
              ref bool handled) {
            switch (msg) {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam) {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero) {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {

            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;

            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO {

            /// <summary>
            /// </summary>
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>
            public int dwFlags = 0;
        }

        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT {

            /// <summary> Win32 </summary>
            public int left;

            /// <summary> Win32 </summary>
            public int top;

            /// <summary> Win32 </summary>
            public int right;

            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public int Width {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }

            /// <summary> Win32 </summary>
            public int Height {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom) {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc) {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty {
                get {

                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }

            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString() {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj) {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }

            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) {
                return !(rect1 == rect2);
            }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        ///
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        #endregion win 32 code
    }
}