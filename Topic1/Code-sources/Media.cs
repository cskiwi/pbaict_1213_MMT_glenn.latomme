using System;
using System.Collections.Generic;

namespace REMPv2 {

    public class Media {
        private String _artist;
        private String _song;
        private String _location;
        private List<string> _genres;
        private bool _playing;
        private bool _failedCreate;

        public Media(String song, String artist, String location) {
            _location = location;
            _failedCreate = false;
            try {
                TagLib.File file = TagLib.File.Create(_location);
                _artist = artist;
                _song = song;

                // check if tags excists init, else default constructor
                _genres = file.Tag.Genres != null ? new List<string>() : null;

                if (_genres != null)
                    foreach (string tag in file.Tag.Genres)
                        _genres.Add(tag.Replace("&", "And"));
            } catch {
                _failedCreate = true;
            }
        }

        public Media(String location) {
            _location = location;
            _failedCreate = false;

            try {
                TagLib.File file = TagLib.File.Create(_location);

                // check if tags excists init, else default constructor
                _artist = file.Tag.Performers[0] != null ? file.Tag.Performers[0].Replace("&", "And") : "";
                _song = file.Tag.Title != null ? file.Tag.Title.Replace("\'", "").Replace("&", "And") : "";
                _genres = file.Tag.Genres != null ? new List<string>() : null;

                if (_genres != null)
                    foreach (string tag in file.Tag.Genres)
                        _genres.Add(tag);
            } catch {
                _failedCreate = true;
            }
        }

        public String Artist {
            get { return _artist; }
            set { _artist = value; }
        }

        public String Song {
            get { return _song; }
            set { _song = value; }
        }

        public String Location {
            get { return _location; }
            set { _location = value; }
        }

        public bool Playing {
            set { _playing = value; }
        }

        public List<string> Tags {
            get { return _genres; }
        }

        public override string ToString() {
            return _playing ? "> " + Artist + " - " + Song : "    " + Artist + " - " + Song;
        }

        public string ToPlayerString() {
            return Artist + " - " + Song;
        }

        public bool FailedToCreate {
            get { return _failedCreate; }
        }
    }
}