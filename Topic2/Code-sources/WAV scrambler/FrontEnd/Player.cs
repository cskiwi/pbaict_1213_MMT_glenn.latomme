/*
 * Soundplayer for custom sounds
 * 
 * Written by Glenn Latomme
 */ 
using System.Collections.Generic;
using System.IO;
using System.Media;

namespace WAV_scrambler.FrontEnd {

    internal class Player {
        private SoundPlayer _soundPlayer;
        private MemoryStream _memoryStream;
        private List<string> _locations;
        private bool _playing;

        public Player() {
            _locations = new List<string>();
        }

        /// <summary>
        /// Plays a file from the _locations list
        /// </summary>
        /// <param name="fileNumber">index number</param>
        public void playFile(int fileNumber) {
            if (_soundPlayer != null && _playing)
                _soundPlayer.Stop();

            _memoryStream = new MemoryStream(File.ReadAllBytes(_locations[fileNumber]), true);
            _soundPlayer = new SoundPlayer(_memoryStream);
            if (_soundPlayer.Stream != null)
                _soundPlayer.Stream.Seek(0, SeekOrigin.Begin);

            _soundPlayer.Play();
            _playing = true;
        }

        /// <summary>
        /// Stops the sound
        /// </summary>
        public void stopSoud() {
            if (_playing)
                _soundPlayer.Stop();
        }

        /// <summary>
        /// Add a location to the string
        /// </summary>
        /// <param name="location">Path to location</param>
        public void addLocation(string location) {
            _locations.Add(location);
        }

        /// <summary>
        /// Locations in list
        /// </summary>
        /// <returns>int length</returns>
        public int AmountOfLocations() {
            return _locations.Count;
        }
    }
}