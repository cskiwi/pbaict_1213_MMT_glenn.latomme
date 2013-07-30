using System.Collections.Generic;
using System.IO;
using System.Linq;
using REMPv2.Collection.Music;

namespace REMPv2.Collection {

    public class MusicCollection {
        private List<Artist> _artists;
        private List<DirectoryInfo> _musicDir;

        public MusicCollection() {
            _artists = new List<Artist>();
            _musicDir = new List<DirectoryInfo>();
        }

        public void AddLocation(string location) {
            DirectoryInfo dir = new DirectoryInfo(location);
            if (_musicDir.Contains(dir) == false) {
                _musicDir.Add(dir);
                AddSongsFromDir(dir);
            }
        }

        public void AddSongsFromDir(DirectoryInfo dir) {
            Song record;

            // Scans folder, with subfolders and add all music files to list
            foreach (FileInfo file in dir.GetFiles("*.*", SearchOption.AllDirectories).Where(file => file.Extension.Equals(".mp3") || file.Extension.Equals(".wav"))) {
                record = new Song(file.FullName);

                int arid = _artists.FindIndex(a => a.Name == (record.Tags.Performers[0] == null ? "UnknownArtist" : record.Tags.Performers[0]));
                if (arid == -1) {
                    Artist ar = new Artist((record.Tags.Performers[0] == null ? "UnknownArtist" : record.Tags.Performers[0]));
                    ar.AddSong(record);
                    _artists.Add(ar);
                } else {
                    _artists[arid].AddSong(record);
                }
            }
        }

        public List<Song> SongsFromFolder(DirectoryInfo dir) {
            List<Song> songsInFolder = new List<Song>();
            foreach (Artist artist in _artists)
                foreach (Song song in artist.Songs)
                    if (song.Location.Contains(dir.FullName))
                        songsInFolder.Add(song);

            return songsInFolder;
        }

        public Artist getArtist(Song song) {
            return _artists.Find(artist => artist.Songs.Contains(song));
        }

        public List<Artist> Artists {
            get { return _artists; }
        }

        public List<DirectoryInfo> MusicDirectories {
            get { return _musicDir; }
            set { _musicDir = value; }
        }
    }
}