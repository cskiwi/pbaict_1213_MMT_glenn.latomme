using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using REMPv2.Collection;
using REMPv2.Collection.Music;
using REMPv2.Recommendation;

namespace REMPv2 {

    internal class Manager {
        private BindingList<Song> _playList;
        private Player _player;
        private int _currentSong;
        private RecommendEngine _recommendEngine;
        private MusicCollection _musicCollection;
        private bool _isScanning;

        public Manager() {
            _player = new Player();
            _playList = new BindingList<Song>();
            _currentSong = 0;
            _recommendEngine = new RecommendEngine();
            _musicCollection = new MusicCollection();
            _isScanning = false;
        }

        public void OpenFile() {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Them music files(*.wav;*.mp3)|*.wav;*.mp3";
            bool? result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value) {
                Song song = new Song(openFileDialog.FileName);
                AddSong(song);
            }
        }

        public void Play() {
            bool HasFile = false;
            Song playsong = null;

            if (_playList.Count > 0) {
                playsong = (Song)_playList[_currentSong];
                if (playsong != null) {
                    HasFile = true;
                }
            }

            if (HasFile) {
                if (_player.songLoaded() == false) {
                    Load(playsong.Location);
                    _playList[_currentSong].Playing = true;
                }
                _player.Play();
            } else {
                OpenFile();
                Play();
            }
        }

        public String GetSongInfo() {
            return _playList[_currentSong].ToPlayerString();
        }

        public String GetSongInfo(int songNumber) {
            return _playList[songNumber].ToString();
        }

        public void Stop() {
            _player.Stop();
        }

        public void Pause() {
            _player.Pause();
        }

        public void nextSong() {
            _player.Dispose();
            _playList[_currentSong].Playing = false;

            if (_playList.Count > 1) {
                if (_currentSong + 1 == _playList.Count)
                    _currentSong = 0;
                else
                    _currentSong++;
            }
            Load(_playList[_currentSong].Location);
            _playList[_currentSong].Playing = true;
        }

        public void previousSong() {
            _player.Dispose();
            _playList[_currentSong].Playing = false;
            if (_playList.Count > 1) {
                if (_currentSong == 0)
                    _currentSong = _playList.Count - 1;
                else
                    _currentSong--;
            }
            Load(_playList[_currentSong].Location);
            _playList[_currentSong].Playing = true;
        }

        public void Dispose() {
            _player.Dispose();
        }

        public void Load(String loc) {
            _player.Load(loc);
        }

        public void AddSong(String loc) {
            Song song = new Song(loc);
            AddSong(song);
        }

        public void AddSong(Song Song) {
            _playList.Add(Song);
        }

        public String getPlayTime() {
            return _player.getPlayTime();
        }

        public String getTotalPlayTime() {
            return _player.getTotalPlayTime();
        }

        public void load(int id) {
            if (id >= 0 && id < _playList.Count) {
                _player.Dispose();
                _playList[_currentSong].Playing = false;

                _currentSong = id;

                Load(_playList[_currentSong].Location);
                _playList[_currentSong].Playing = true;
            }
        }

        public void AddMusicFolder() {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.Description = "Open your folder with music thingys";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                    _musicCollection.AddLocation(dialog.SelectedPath);
            }
            Thread scanThread = new Thread(ScanFiles);
            if (scanThread.ThreadState == ThreadState.Unstarted || scanThread.ThreadState == ThreadState.Stopped)
                scanThread.Start();
            // ScanFiles();
        }

        private void ScanFiles() {
            _isScanning = true;
            for(int i = 0; i < _musicCollection.Artists.Count; i ++) {
                _recommendEngine.CheckArtist(_musicCollection.Artists[i]);
            };
            _isScanning = false;
            /*
            foreach (Artist ar in _musicCollection.Artists) {
                _recommendEngine.CheckArtist(ar);
            }
             */
        }

        public List<Artist> GetRecommends(Artist artist, int limit) {
            return _recommendEngine.GetRecommends(artist, limit, _musicCollection);
        }

        public BindingList<Song> PlayList {
            get { return _playList; }
        }

        public MusicCollection MusicCollection {
            get { return _musicCollection; }
            set { _musicCollection = value; }
        }

        public int currentSeconds() {
            return _player.currentSeconds();
        }

        public int totalSeconds() {
            return _player.totalSeconds();
        }

        public void setPlayTime(TimeSpan t) {
            _player.setPlayTime(t);
        }

        public int CurrentSongNumber {
            get { return _currentSong; }
        }

        public Song CurrentSong {
            get { return _playList.Count > 0 ? _playList[_currentSong] : null; }
        }
        public bool IsScanning {
            get { return _isScanning; }
        }
    }
}