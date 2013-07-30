using System.Collections.Generic;
using System.Windows;
using REMPv2.Collection;
using REMPv2.Collection.Music;

namespace REMPv2 {

    /// <summary>
    /// Interaction logic for AddFilesToPlaylist.xaml
    /// </summary>
    public partial class AddFilesToPlaylist : Window {
        private MainWindow _parent;
        private MusicCollection _musicCollection;
        private int _selectedFolder;

        public AddFilesToPlaylist(MainWindow window, MusicCollection collection) {
            InitializeComponent();

            _parent = window;
            _musicCollection = collection;
            _selectedFolder = -1;
            updateLists();
        }

        private void AddSongToPlaylist(object sender, RoutedEventArgs e) {
            foreach (var item in SongSelection.SelectedItems)
                _parent.addFileToPlaylist((Song)item);
        }

        private void AddFolder(object sender, RoutedEventArgs e) {
            _parent.addFolder();
            _selectedFolder = _musicCollection.MusicDirectories.Count - 1;
            updateLists();
        }

        private void updateLists() {

            // only the folder name now shows instead of full directory
            List<string> items = new List<string>();
            foreach (var item in _musicCollection.MusicDirectories)
                items.Add(item.Name);

            FolderSecltion.ItemsSource = items;

            // get items in selected folder
            if (_selectedFolder != -1)
                SongSelection.ItemsSource = _musicCollection.SongsFromFolder(_musicCollection.MusicDirectories[_selectedFolder]);
            if (SongSelection.Items.Count > 0)
                if (SongSelection.SelectedItems.Count == 0)
                    SongSelection.ScrollIntoView(SongSelection.Items[0]);
                else
                    SongSelection.ScrollIntoView(SongSelection.SelectedItems[0]);
        }

        public MusicCollection MusicCollection {
            set { _musicCollection = value; }
        }

        private void SongSelection_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (SongSelection.SelectedItem != null)
                if (SongSelection.SelectedItem.ToString().Length != 0)
                    _parent.addFileToPlaylist((Song)SongSelection.SelectedItem);
        }

        private void FolderSecltion_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            _selectedFolder = FolderSecltion.SelectedIndex;
            updateLists();
        }
    }
}