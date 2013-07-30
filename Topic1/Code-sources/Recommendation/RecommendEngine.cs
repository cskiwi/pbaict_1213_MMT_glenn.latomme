using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using REMPv2.Collection;
using REMPv2.Collection.Music;

namespace REMPv2.Recommendation {

    internal class RecommendEngine {
        private bool _connected;
        private DatabaseConnection _dBConnection;

        public RecommendEngine() {

            // setup database connection
            _dBConnection = new DatabaseConnection();
            _dBConnection.SetupDatabase("srv.latomme-g.be", "remp", "kiwi", "kiwi");
        }

        public void CheckArtist(Artist artist) {
            if (artist.Scanned == false) {
                _dBConnection.CheckConnection();
                if (_dBConnection.CanConnect) {
                    _dBConnection.OpenConnection();
                    List<string>[] result = _dBConnection.ExecuteSelect("SELECT id FROM artists WHERE Name = '" + artist.Name + "';");

                    if (result != null) {
                        // if there is a number in resutl parse, else -1
                        int artistID = result[0].Count == 0 ? -1 : int.Parse(result[0][0]);

                        // artist
                        if (artistID == -1) {
                            _dBConnection.ExecuteInsert("INSERT INTO Artists (Name)VALUES('" + artist.Name + "');");
                            artistID = int.Parse(_dBConnection.ExecuteSelect("SELECT id FROM artists WHERE Name = '" + artist.Name + "';")[0][0]);
                        }

                        // songs
                        result = _dBConnection.ExecuteSelect("SELECT Songs.Name FROM Songs INNER JOIN Artists on Artists.id = Songs.Artist_id WHERE Artists.Name = '" + artist.Name + "';");
                        List<string> songs = result == null ? null : result[0];

                        if (songs != null) {
                            if (songs.Count < artist.Songs.Count) {

                                // create list of current songs
                                List<string> currentSongs = new List<string>();
                                foreach (Song song in artist.Songs)
                                    currentSongs.Add(song.Tags.Title);

                                Parallel.ForEach(songs, song => {
                                    song = song.Replace("\'", "").ToLower();
                                });

                                // filter out those I have
                                List<string> tobeAdded = currentSongs.Except(songs).ToList();

                                // link artist to tag
                                foreach (string song in tobeAdded) {
                                    _dBConnection.ExecuteInsert("INSERT INTO Songs (Artist_id, Name)VALUES(" + artistID + ",  '" + song.Replace("\'", "").ToLower() + "');");
                                }
                            }

                            // tags
                            result = _dBConnection.ExecuteSelect("SELECT tag FROM artists_has_tags INNER JOIN Artists on Artists.id = artists_has_tags.Artist_id WHERE Artists.Name = '" + artist.Name + "';");

                            List<string> tags = result == null ? null : result[0];

                            if (tags != null) {
                                if (tags.Count < artist.Tags.Count) {
                                    Parallel.ForEach(artist.Tags, tag => {
                                        tag = tag.Replace("\'", "").ToLower();
                                    });
                                    List<string> tobeAdded = artist.Tags.Except(tags).ToList();
                                    foreach (string tag in tobeAdded) {
                                        string FilterTag = tag.Replace("\'", "").ToLower();

                                        // if tag doesn't excist add
                                        if (_dBConnection.ExecuteCount("SELECT COUNT(*) FROM tags WHERE Tag = '" + FilterTag + "';") == 0)
                                            _dBConnection.ExecuteInsert("INSERT INTO tags (Tag) VALUES ('" + FilterTag + "');");

                                        // link artist to tag
                                        if (_dBConnection.ExecuteCount("SELECT COUNT(*) FROM artists_has_tags WHERE Artist_id = " + artistID + " AND Tag = '" + FilterTag + "';") == 0)
                                            _dBConnection.ExecuteInsert("INSERT INTO artists_has_tags (Artist_id, Tag)VALUES(" + artistID + ",  '" + FilterTag + "');");
                                    }
                                }
                            }
                        }
                    }

                    _dBConnection.CloseConnection();
                } else {

                    // no need to check if excists in database if the link is offline
                }
                artist.Scanned = true;
            }
        }

        public List<Artist> GetRecommends(Artist artist, int limit, MusicCollection collection) {
            List<List<string>> SimmilarArtistFromTag = new List<List<string>>();
            List<string> oneTagInCommon;
            Dictionary<Artist, int> dictionary = new Dictionary<Artist, int>();

            _dBConnection.CheckConnection();
            if (_dBConnection.CanConnect) {
                _dBConnection.OpenConnection();

                // get all artists with 1 simmular tag
                foreach (string tag in artist.Tags)
                    SimmilarArtistFromTag.Add(_dBConnection.ExecuteSelect("SELECT Name FROM Artists INNER JOIN artists_has_tags on artists_has_tags.Artist_id = Artists.id WHERE artists_has_tags.tag = '" + tag.Replace("\'", "") + "';")[0]);

                List<string> temp = new List<string>();

                // add all the artists to a list
                foreach (List<string> artistsForTag in SimmilarArtistFromTag) {
                    foreach (string ar in artistsForTag)
                        temp.Add(ar);
                }

                // filter doubles
                oneTagInCommon = temp.Distinct().ToList();

                // remove original from compare list
                oneTagInCommon.Remove(artist.Name);

                // add own artist to result set, and make that it shows up first
                dictionary.Add(artist, int.MaxValue);

                // count the other tags in common
                foreach (string ar in oneTagInCommon) {
                    int artistIndex = collection.Artists.FindIndex(a => a.Name == ar);

                    // must excist in collection
                    if (artistIndex != -1) {
                        int common = 0;

                        // get them tags
                        List<string> tags = _dBConnection.ExecuteSelect("SELECT tag FROM artists_has_tags INNER JOIN Artists on Artists.id = artists_has_tags.Artist_id WHERE Artists.Name = '" + ar + "';")[0];

                        common = artist.Tags.Count - (tags.Count - tags.Except(artist.Tags).ToList().Count);

                        // add artist + count
                        dictionary.Add(collection.Artists[artistIndex], common);
                    }

                    // check if limit is reached
                    if (dictionary.Count == limit)
                        break;
                }
                _dBConnection.CloseConnection();
            } else {

                // offline search
                List<Artist> artists = new List<Artist>();
                List<string> tempArtists = new List<string>();

                int common = 0;
                foreach (Artist ar in collection.Artists) {
                    common = 0;
                    foreach (string tag in ar.Tags)
                        if (artist.Tags.Contains(tag))
                            common++;
                    dictionary.Add(ar, common);
                }
            }

            // return: sorted, listed, limited
            return dictionary.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList().GetRange(0, limit);
        }
    }
}