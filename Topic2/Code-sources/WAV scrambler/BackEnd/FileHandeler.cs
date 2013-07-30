using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using WAV_scrambler.BackEnd;

namespace WAV_scrambler {

    internal class FileHandeler {
        private string _location;
        private float[] _floats;
        private wavInfo _wavInfo;

        // temp save header info, for not having to save evry thing in wavInfo that I don't need
        private byte[] _headerInfo;

        public FileHandeler() {

            // setup things for null reference can be checked for
            _location = "";
            _wavInfo = new wavInfo();
            _floats = null;
        }

        public void ReadFile(string location) {
            _location = location;
            ReadFile();
        }

        public void ReadFile() {

            // read them bytes
            byte[] wav = File.ReadAllBytes(_location);

            if (wav.Length < 16000000) {
                // check format
                byte[] formatBytes = { wav[12], wav[13], wav[14] };

                string format = System.Text.Encoding.UTF8.GetString(formatBytes);

                // Set info based on format
                switch (format) {
                    case "fmt":
                        _wavInfo.Format = "fmt";
                        _wavInfo.NumChannnels = wav[22];
                        _wavInfo.SampleRate = bytesToInt(wav[24], wav[25]);
                        _wavInfo.BitsPerSample = bytesToInt(wav[34], wav[35]);
                        setDataBlock(wav);

                        break;

                    default:
                        MessageBox.Show("Not supported wav format");
                        break;
                }
            } else {
                MessageBox.Show("File is to large");
                wav = null;
            }
        }

        /// <summary>
        /// Copy's header info of a wav file
        /// </summary>
        /// <param name="wav">Wav file with header</param>
        /// <returns></returns>
        private byte[] GetHeaderInfo(byte[] wav) {
            byte[] header = new byte[_wavInfo.StartDataBlock];
            int startDataBlock = findDataBlock(wav);
            if (startDataBlock != -1) {
                for (int i = 0; i < startDataBlock; i++) {
                    header[i] = wav[i];
                }
                return header;
            } else
                return null;
        }

        /// <summary>
        /// Finds the data chunck based on format.
        /// Or if that doesn't work, checks first 200 bytes if it can find it
        /// </summary>
        /// <param name="wav">byteArray of the wav file</param>
        /// <returns></returns>
        private int findDataBlock(byte[] wav) {
            byte[] DataString = new byte[4];
            int checkLocation = 0;

            //fast check for known formats
            switch (_wavInfo.Format) {
                case "fmt":
                    DataString = new byte[] { wav[36], wav[37], wav[38], wav[39] };
                    checkLocation = 36;
                    break;
            }

            // if fast check was set, check if there is an data block there
            if (System.Text.Encoding.UTF8.GetString(DataString) != "data") {

                // reset check location
                checkLocation = 0;

                // check if location was correct, else find data block in first 200 bytes
                while (System.Text.Encoding.UTF8.GetString(DataString) != "data" && checkLocation < 200) {

                    // set a new data block
                    DataString = new byte[4] { wav[checkLocation], wav[checkLocation + 1], wav[checkLocation + 2], wav[checkLocation + 3] };

                    // increase location
                    checkLocation++;
                }
            } else {

                // small fix because in while block I increase at the end, this is just the easiest way,
                // to fix if it was preset, without setting not correct numbers in fast check
                checkLocation++;
            }

            // skip the data words && subchunk size (block is 8 long, but while makes the checkLoc +1 so here -1)
            checkLocation += 7;

            // if checkLocation == 207 that means, the location was not found return -1 in that case
            return checkLocation == 207 ? -1 : checkLocation;
        }

        /// <summary>
        /// Does the calculations of the data block
        /// </summary>
        /// <param name="wav">Supported sound file</param>
        private void setDataBlock(byte[] wav) {
            _wavInfo.StartDataBlock = findDataBlock(wav);
            if (_wavInfo.StartDataBlock != -1) {
                _headerInfo = GetHeaderInfo(wav);

                if (_headerInfo != null && wav != null) {

                    // make the byte array as an float
                    float[] _WavAsFloat = wav.Select(b => (float)Convert.ToDouble(b)).ToArray();


                    _floats = new float[wav.Length / 2];
                    List<float> sampleValue = new List<float>();

                    for (int i = _wavInfo.StartDataBlock; i < wav.Length; i += 2) {
                        byte[] value = new byte[2];
                        Array.Copy(wav, i, value, 0, 2);                                     // kopieer  b0 & b1
                        Int16 SampleValueAsInteger = BitConverter.ToInt16(value, 0);         // combineer tot een Int16
                        float SampleValueAsFloat = ((float)SampleValueAsInteger) / 32768f;   // converter en normaliseer naar het bereik -1..+1
                        sampleValue.Add(SampleValueAsFloat);
                    }

                    _floats = sampleValue.ToArray();
                    _wavInfo.LengthDataBlock = _floats.Length - _wavInfo.StartDataBlock;
                } else {
                    Console.WriteLine("header block could not be fetched");
                }
            } else {
                Console.WriteLine("No Data Block was found");
            }
        }

        /// <summary>
        /// Save a wav file
        /// </summary>
        /// <param name="location">Destination Location</param>
        /// <param name="headerInfo">Header info</param>
        /// <param name="soundData">SoundData</param>
        public void WriteFile(string location, byte[] headerInfo, float[] soundData) {
            byte[] FullwavFile = new byte[soundData.Length * 2 + headerInfo.Length];

            // copy header
            Array.Copy(headerInfo, FullwavFile, headerInfo.Length);

            // Add Data
            List<byte> exportData = new List<byte>();
            for (int i = 0; i < soundData.Length; i++) {
                Int16 SampleValueAsInteger = (Int16)(soundData[i] * 32768f);                // convert to int16
                byte[] SampleValueAsBytes = BitConverter.GetBytes(SampleValueAsInteger);    // make bytes
                exportData.Add(SampleValueAsBytes[0]);                                      // add bytes to list
                exportData.Add(SampleValueAsBytes[1]);
            }
            // copy list to final array
            Array.Copy(exportData.ToArray(), 0, FullwavFile, _wavInfo.StartDataBlock, exportData.Count); 

            // save
            File.WriteAllBytes(location, FullwavFile);
        }

        /// <summary>
        /// Saves current loaded wav file
        /// </summary>
        /// <param name="location">Destination Location</param>
        public void WriteFile(string location) {
            WriteFile(location, _headerInfo, _floats);
        }

        /// <summary>
        /// Saves current custom soundddata with same format as loaded wav file
        /// </summary>
        /// <param name="location">Destination Location</param>
        /// <param name="soundData">SoundData</param>
        public void WriteFile(string location, float[] soundData) {
            WriteFile(location, _headerInfo, soundData);
        }

        /// <summary>
        /// Saves current data, with different headerinfo
        /// </summary>
        /// <param name="location">Destination Location</param>
        /// <param name="headerInfo">Header info</param>
        public void WriteFile(string location, byte[] headerInfo) {
            WriteFile(location, headerInfo, _floats);
        }

        /// <summary>
        /// Converts 2 Bytes to an int
        /// </summary>
        /// <param name="firstByte"></param>
        /// <param name="secondByte"></param>
        /// <returns></returns>
        private int bytesToInt(byte firstByte, byte secondByte) {
            return (secondByte << 8) | firstByte;
        }


        /// <summary>
        /// Gets data floats, without the header info
        /// </summary>
        /// <returns>Float array of data</returns>
        public float[] Floats {
            get { return _floats; }
            set { _floats = value; }
        }

        /// <summary>
        /// Gets data floats, without the header info
        /// </summary>
        /// <returns>Float array of data</returns>
        public wavInfo GetInfo {
            get { return _wavInfo; }
        }

        public byte[] HeaderInfo {
            get { return _headerInfo; }
        }
    }
}