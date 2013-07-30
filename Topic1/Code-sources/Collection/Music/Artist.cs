using System.Collections.Generic;

namespace REMPv2.Collection.Music {

    public class Artist {
        private string _name;
        private List<Song> _songs;
        private List<string> _tags;
        private bool _scanned;

        public Artist(string name) {
            _name = name;
            _songs = new List<Song>();
            _tags = new List<string>();
            _scanned = false;
        }

        private void scanForTags(Song song) {
            if (song.Tags != null)
                foreach (string tag in song.Tags.Genres)
                    if (_tags.Contains(tag) == false)
                        _tags.Add(tag);
        }

        public void AddSong(Song song) {
            if (_songs.Contains(song) == false) {
                scanForTags(song);
                _songs.Add(song);
            }
        }

        public string Name {
            get { return _name; }
        }

        public List<Song> Songs {
            get { return _songs; }
        }

        public List<string> Tags {
            get { return _tags; }
        }

        public bool Scanned {
            get { return _scanned; }
            set { _scanned = value; }
        }
    }
}