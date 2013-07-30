using System;

namespace REMPv2.Collection.Music {

    public class Song {
        private TagLib.Tag _tags;
        private String _location;
        private bool _playing;
        private bool _failedCreate;

        public Song(string location) {
            _location = location;
            _failedCreate = false;

            try {
                _tags = TagLib.File.Create(_location).Tag;
            } catch {
                _failedCreate = true;
            }
        }

        public TagLib.Tag Tags {
            get { return _tags; }
        }

        public string Location {
            get { return _location; }
        }

        public bool Playing {
            get { return _playing; }
            set { _playing = value; }
        }

        public override string ToString() {
            return _playing ? "> " + _tags.Performers[0] + " - " + _tags.Title : "    " + _tags.Performers[0] + " - " + _tags.Title;
        }

        public string ToPlayerString() {
            return _tags.Performers[0] + " - " + _tags.Title;
        }

        public bool FailedToCreate {
            get { return _failedCreate; }
        }
    }
}