using System.Collections.Generic;
using System.Windows;
using MySql.Data.MySqlClient;

// ************************
// Database connection based on:
//
// http://www.codeproject.com/Articles/43438/Connect-C-to-MySQL
// ************************

namespace REMPv2.Recommendation {

    public class DatabaseConnection {
        private string _url;
        private string _database;
        private string _username;
        private string _password;
        private bool _canConnect;
        private MySqlConnection _connection;
        private MySqlCommand _command;

        public void SetupDatabase(string url, string database, string username, string password) {
            _url = url;
            _database = database;
            _username = username;
            _password = password;

            Initialize();
        }

        private void Initialize() {
            _connection = new MySqlConnection("SERVER=" + _url + ";" + "DATABASE=" + _database + ";" + "UID=" + _username + ";" + "PASSWORD=" + _password + ";");
        }

        public void CheckConnection() {
            OpenConnection();
            CloseConnection();
        }

        public void OpenConnection() {
            try {
                if (_connection.State == System.Data.ConnectionState.Closed)
                    _connection.Open();
                _canConnect = true;
            } catch (MySqlException e) {
                _canConnect = false;
            }
        }

        public void CloseConnection() {
            try {
                if (_connection.State != System.Data.ConnectionState.Closed) 
                    _connection.Close();
                
            } catch (MySqlException e) {
                MessageBox.Show("couldn't close the connection \n" + e.Message);
            }
        }

        public void ExecuteInsert(string querry) {

            // check if querry is set
            if (querry != "") {

                // check if querry has right statement
                if (querry.Contains("INSERT")) {

                    // check if the connection is open
                    if (_canConnect) {

                        // create command
                        _command = new MySqlCommand(querry, _connection);

                        // Console.WriteLine(querry);

                        // execute
                        _command.ExecuteNonQuery();
                    }
                } else {
                    MessageBox.Show("Not an insert querry, but insert has been called\nquerry = " + querry);
                }
            } else {
                MessageBox.Show("Querry not set");
            }
        }

        public void ExecuteUpdate(string querry) {

            // check if querry is set
            if (querry != "") {

                // check if querry has right statement
                if (querry.Contains("UPDATE")) {

                    // check if the connection is open
                    if (_canConnect) {

                        // create command
                        _command = new MySqlCommand(querry, _connection);

                        // execute
                        _command.ExecuteNonQuery();
                    }
                } else {
                    MessageBox.Show("Not an update querry, but update has been called\nquerry = " + querry);
                }
            } else {
                MessageBox.Show("Querry not set");
            }
        }

        public void ExecuteDelete(string querry) {

            // check if querry is set
            if (querry != "") {

                // check if querry has right statement
                if (querry.Contains("DELETE")) {

                    // check if the connection is open
                    if (_canConnect) {

                        // create command
                        _command = new MySqlCommand(querry, _connection);

                        // execute
                        _command.ExecuteNonQuery();
                    }
                } else {
                    MessageBox.Show("Not an delete querry, but delete has been called\nquerry = " + querry);
                }
            } else {
                MessageBox.Show("Querry not set");
            }
        }

        public List<string>[] ExecuteSelect(string querry) {
            List<string>[] resultSet = null;

            // check if querry is set
            if (querry != "") {

                // check if querry has right statement
                if (querry.Contains("SELECT")) {
                    if (_canConnect) {

                        // create command
                        _command = new MySqlCommand(querry, _connection);

                        // execute
                        MySqlDataReader reader = _command.ExecuteReader();

                        // fetch data
                        resultSet = new List<string>[reader.FieldCount];
                        List<string> columnames = new List<string>();

                        for (int i = 0; i < reader.FieldCount; i++) {

                            // storage the name of each collum
                            columnames.Add(reader.GetName(i));

                            // for each cullum there is an list for the values
                            resultSet[i] = new List<string>();
                        }

                        // get data for each collum
                        while (reader.Read()) {
                            for (int i = 0; i < columnames.Count; i++) {
                                string value = reader[columnames[i]].ToString();
                                resultSet[i].Add(value);
                            }
                        }

                        // close reader
                        reader.Close();
                    }
                } else {
                    MessageBox.Show("Not an select querry, but select has been called\nquerry = " + querry);
                }
            } else {
                MessageBox.Show("Querry not set");
            }

            // return list
            return resultSet;
        }

        public int ExecuteCount(string querry) {
            int count = -1;

            // check if querry is set
            if (querry != "") {

                // check if querry has right statement
                if (querry.Contains("COUNT")) {

                    // check if the connection is open
                    if (_canConnect) {

                        // create command
                        _command = new MySqlCommand(querry, _connection);

                        // execute
                        try {
                            count = int.Parse(_command.ExecuteScalar() + "");
                        } catch {
                            count = 0;
                        }
                    }
                } else {
                    MessageBox.Show("Not an count querry, but count has been called\nquerry = " + querry);
                }
            } else {
                MessageBox.Show("Querry not set");
            }
            return count;
        }

        public bool CanConnect {
            get { return _canConnect; }
        }
    }
}