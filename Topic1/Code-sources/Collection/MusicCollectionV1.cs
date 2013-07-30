using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace REMPv2.Collection {

    public class MusicCollectionV1 {

        // contains your collection, so the recommend engine knows what songs you have
        private List<Media> _files = new List<Media>();
        private List<DirectoryInfo> _musicDirectories = new List<DirectoryInfo>();

        public MusicCollectionV1() {
            _musicDirectories = new List<DirectoryInfo>();
            _files = new List<Media>();
        }

        public MusicCollectionV1(string loc) {
            AddLocation(loc);
        }

        public MusicCollectionV1(List<DirectoryInfo> dirLoactions) {

            // scan all given locations for files
            foreach (DirectoryInfo dir in dirLoactions) {
                AddSongsFromDir(dir);
            }
        }

        public void FullDirList(string loc) {
            AddSongsFromDir(new DirectoryInfo(loc));
        }

        public void AddSongsFromDir(DirectoryInfo dir) {
            Media record;
            // Scans folder, with subfolders and add all music files to list
            foreach (FileInfo file in dir.GetFiles("*.*", SearchOption.AllDirectories).Where(file => file.Extension.Equals(".mp3") || file.Extension.Equals(".wav"))) {
                record = new Media(file.FullName);
                if (_files.Contains(record) == false && record.FailedToCreate == false)
                    _files.Add(record);
            }
        }

        public void AddLocation(DirectoryInfo dir) {
            if (_musicDirectories.Contains(dir) == false) {
                _musicDirectories.Add(dir);
                AddSongsFromDir(dir);
            }
        }

        public void AddLocation(string loc) {
            AddLocation(new DirectoryInfo(loc));
        }

        public void AddLocation(List<DirectoryInfo> dirLoactions) {
            foreach (DirectoryInfo dir in dirLoactions) {
                AddSongsFromDir(dir);
            }
        }

        public void Dump() {
            foreach (Media record in _files) {
                Console.WriteLine(record.ToString());
            }
        }

        public bool ContainsArtist(string artist) {
            foreach (Media record in _files) {
                if (record.Artist == artist && songExists(record))
                    return true;
            }
            return false;
        }

        public List<Media> getSongs(string artist) {
            List<Media> songs = new List<Media>();
            foreach (Media record in _files) {
                if (record.Artist == artist)
                    if (songExists(record))
                        songs.Add(record);
            }
            return songs;
        }

        private bool songExists(Media song) {
            return File.Exists(song.Location);
        }

        public List<Media> FilesFromFolder(string Location) {
            List<Media> files = new List<Media>();
            foreach (Media record in _files)
                if (record.Location.Contains(Location))
                    files.Add(record);

            return files;

        }

        public Media getRecord(string artist) {
            return Collection.Find(file => file.Artist.ToLower() == artist.ToLower());
        }

        public List<Media> Collection {
            get { return _files; }
        }

        public List<DirectoryInfo> MusicDirectories {
            get { return _musicDirectories; }
            set { _musicDirectories = value; }
        }
    }
}