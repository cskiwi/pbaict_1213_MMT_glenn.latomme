using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WAV_scrambler.BackEnd;
using WAV_scrambler.FrontEnd;

namespace WAV_scrambler {

    public partial class Form1 : Form {
        private FileHandeler _filehandeler;
        private Player _soundPlayer;
        private float[] _fourierData;
        private float[] _originals;
        private bool _isFFT;
        private Drawer _draw;

        public Form1() {
            InitializeComponent();
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer5.FixedPanel = FixedPanel.Panel1;
            splitContainer6.FixedPanel = FixedPanel.Panel1;
            splitContainer7.FixedPanel = FixedPanel.Panel1;
            splitContainer8.FixedPanel = FixedPanel.Panel1;
            _isFFT = false;

            _draw = new Drawer();
            _soundPlayer = new Player();
            resetImages();
        }

        #region Toolstrip things
        /// <summary>
        /// Open a wav with FileDialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {

            // openfiledialog aanmaken
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Opties instellen, filters toepassen
            openFileDialog.Filter = "wav files (*.wav)|*.wav";

            // dialog tonen en testen of ingeladen kan worden
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                string path = openFileDialog.FileName;
                try {
                    _filehandeler = new FileHandeler();
                    _draw.ResetScaling();
                    resetImages();
                    _filehandeler.ReadFile(path);

                    if (_filehandeler.Floats != null) {
                        wavInfo info = _filehandeler.GetInfo;

                        Channels.Text = info.NumChannnels.ToString();
                        SamplePerSecond.Text = info.SampleRate.ToString();
                        BitsPerSample.Text = info.BitsPerSample.ToString();
                        FileName.Text = openFileDialog.SafeFileName;
                        Path.Text = path.Replace(FileName.Text, "");

                        _soundPlayer = new Player();
                        _soundPlayer.addLocation(path);


                        openActions();
                    } else {
                        _filehandeler = null;
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Error: Could not read file from disk.\nOriginal error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Save the wav with FileDialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {

            // savefiledialog aanmaken
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // opties instellen, filters toepassen
            saveFileDialog.Filter = "wav files (*.wav)|*.wav";

            // tonen en testen of de save werkt
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                string file = saveFileDialog.FileName;
                try {
                    if (_isFFT) {
                        Fourier.RFFT(_fourierData, _fourierData.Length, FourierDirection.Backward);
                        float factor = _fourierData.Length / 2;
                        for (int i = 0; i < _fourierData.Length; i++)
                            _fourierData[i] /= factor;
                    }

                    _filehandeler.WriteFile(file, _fourierData);
                } catch (Exception ex) {
                    MessageBox.Show("Error: Could not write file to disk.\nOriginal error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Exit thingy in menu thng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Environment.Exit(0);
        }

        #endregion

        #region Fourier
        /// <summary>
        /// Calculates an RFFT based on the values in the sliders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e) {
            if (_fourierData != null) {
                if (_isFFT == false) {
                    Array.Copy(_originals, _fourierData, _originals.Length);
                    Fourier.RFFT(_fourierData, _fourierData.Length, FourierDirection.Forward);
                }

                // Console.WriteLine("Hz/Number: " + HzPerValue);

                double persentage;
                persentage = ((double)(trackBar0_5.Value - trackBar0_5.Minimum) / trackBar0_5.Maximum) * 100;
                if (persentage != 0)
                    modifiyFloats(0, 5000, persentage);
                persentage = ((double)(trackBar5_10.Value - trackBar5_10.Minimum) / trackBar5_10.Maximum) * 100;
                if (persentage != 0)
                    modifiyFloats(5000, 10000, persentage);
                persentage = ((double)(trackBar10_15.Value - trackBar15_20.Minimum) / trackBar15_20.Maximum) * 100;
                if (persentage != 0)
                    modifiyFloats(10000, 15000, persentage);
                persentage = ((double)(trackBar15_20.Value - trackBar15_20.Minimum) / trackBar15_20.Maximum) * 100;
                if (persentage != 0)
                    modifiyFloats(15000, 20000, persentage);

                FreqOut.Image = _draw.makeFreqGraph(FreqOut.Width, FreqOut.Height, _fourierData); ;

                // revert Forier
                Fourier.RFFT(_fourierData, _fourierData.Length, FourierDirection.Backward);
                _isFFT = false;

                // fix the RFFT that get's bigger by the factor
                float factor = _fourierData.Length / 2;
                for (int i = 0; i < _fourierData.Length; i++) {
                    _fourierData[i] /= factor;
                }

                WavOut.Image = _draw.makeWavGraph(WavIn.Width, WavIn.Height, _fourierData, _filehandeler.GetInfo.NumChannnels, _filehandeler.GetInfo.LengthDataBlock);

                buttonFilterPlay.Enabled = true;
                buttonFilterStop.Enabled = true;
            } else {
                MessageBox.Show("No data loaded");
            }
        }

        /// <summary>
        /// Changes the values from _fourierData
        /// </summary>
        /// <param name="min">Min Hz</param>
        /// <param name="max">Max  Hz</param>
        /// <param name="persentage">% that it's gonna be devided with</param>
        /// <param name="HzPerValue">How many Hz per value there is, to know how much values need to be changed</param>
        private void modifiyFloats(int min, int max, double persentage) {
            double HzPerValue = (double)_filehandeler.GetInfo.SampleRate / (_fourierData.Length);

            int from = (int)((min)/ HzPerValue);
            int to =  (int)(max/ HzPerValue);

            // check if value is even, because values work in pairs (complex)
            if ((to - from) % 2 != 0)
                to++;

            for (int i = from; i < to; i++) {
                _fourierData[i] -= (float)(_fourierData[i] * (persentage / 100));
            }
        }

        #endregion

        /// <summary>
        /// A set of functions and calculations that needs to be set when a new file is opend
        /// </summary>
        private void openActions() {

            // get data without header
            float[] data = _filehandeler.Floats;

            // find suitable power of 2
            int sizeArray = 2;
            while (sizeArray < (data.Length - _filehandeler.GetInfo.StartDataBlock))
                sizeArray *= 2;

            // create an array with that data, rest insert 0
            _fourierData = new float[sizeArray];
            _originals = new float[sizeArray];
            for (int v = 0; v < sizeArray; v++)
                _fourierData[v] = 0;

            Array.Copy(data, _fourierData, data.Length);
            Array.Copy(data, _originals, data.Length);

            WavIn.Image = _draw.makeWavGraph(WavIn.Width, WavIn.Height, _fourierData, _filehandeler.GetInfo.NumChannnels, _filehandeler.GetInfo.LengthDataBlock);

            // calculate Forier
            Fourier.RFFT(_fourierData, _fourierData.Length, FourierDirection.Forward);
            _isFFT = true;
            FreqIn.Image = _draw.makeFreqGraph(FreqIn.Width, FreqIn.Height, _fourierData);
        }

        #region restting things
        /// <summary>
        /// Creates a white image,  with a border
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Bitmap generateEmptyImage(int width, int height) {
            Bitmap blank = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(blank);

            g.Clear(Color.White);
            g.DrawRectangle(new Pen(Color.Black), 0, 0, width - 1, height - 1);

            return blank;
        }

        /// <summary>
        /// Makes all the images in the program white with a black border
        /// </summary>
        private void resetImages() {
            WavIn.Image = generateEmptyImage(WavIn.Width, WavIn.Height);
            WavOut.Image = generateEmptyImage(WavOut.Width, WavOut.Height);

            FreqIn.Image = generateEmptyImage(FreqIn.Width, FreqIn.Height);
            FreqOut.Image = generateEmptyImage(FreqOut.Width, FreqOut.Height);
        }

        /// <summary>
        /// Resets the sliders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReset_Click(object sender, EventArgs e) {
            trackBar0_5.Value = 0;
            trackBar5_10.Value = 0;
            trackBar10_15.Value = 0;
            trackBar15_20.Value = 0;
        }
        #endregion

        #region SaveImages

        /// <summary>
        /// Saves an image, with an openFileLocation
        /// </summary>
        /// <param name="image">Image that needs to be saved</param>
        private void saveImage(Image image) {

            // savefiledialog aanmaken
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // opties instellen, filters toepassen
            saveFileDialog.InitialDirectory = @"C:\";
            saveFileDialog.Filter = "jpg files (*.jpg)|*.jpg";

            // tonen en testen of de save werkt
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                string file = saveFileDialog.FileName;
                try {
                    image.Save(file);
                } catch (Exception ex) {
                    MessageBox.Show("Error: Could not write file to disk.\nOriginal error: " + ex.Message);
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e) {

            // input wav
            if (WavOut.Image != null)
                saveImage(WavOut.Image);
            else
                MessageBox.Show("Image was not generated yet");
        }

        private void saveFreqToolStripMenuItem_Click(object sender, EventArgs e) {

            // input freq
            if (FreqIn.Image != null)
                saveImage(FreqIn.Image);
            else
                MessageBox.Show("Image was not generated yet");
        }

        private void saveWavToolStripMenuItem_Click(object sender, EventArgs e) {

            // output wav
            if (WavOut.Image != null)
                saveImage(WavOut.Image);
            else
                MessageBox.Show("Image was not generated yet");
        }

        private void saveFreqToolStripMenuItem1_Click(object sender, EventArgs e) {

            // output freq
            if (FreqOut.Image != null)
                saveImage(FreqOut.Image);
            else
                MessageBox.Show("Image was not generated yet");
        }

        private void saveBothToolStripMenuItem_Click(object sender, EventArgs e) {
            if (WavIn.Image != null && FreqIn != null) {
                Bitmap both = new Bitmap(WavIn.Width + FreqIn.Width + 30, WavIn.Height + 20);
                Graphics g = Graphics.FromImage(both);

                // white background
                g.Clear(Color.White);

                // set images
                g.DrawImage(WavIn.Image, new Point(10, 10));
                g.DrawImage(FreqIn.Image, new Point(WavIn.Width + 20, 10));

                saveImage(both);
            } else
                MessageBox.Show("Not all images were generated");
        }

        private void saveBothToolStripMenuItem1_Click(object sender, EventArgs e) {
            if (WavOut.Image != null && FreqOut != null) {
                Bitmap both = new Bitmap(WavOut.Width + FreqOut.Width + 30, WavOut.Height + 20);
                Graphics g = Graphics.FromImage(both);

                // white background
                g.Clear(Color.White);

                // set images
                g.DrawImage(WavOut.Image, new Point(10, 10));
                g.DrawImage(FreqOut.Image, new Point(WavOut.Width + 20, 10));

                saveImage(both);
            } else
                MessageBox.Show("Not all images were generated");
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e) {
            if (WavIn.Image != null && FreqIn != null && WavOut.Image != null && FreqOut != null) {
                Bitmap all = new Bitmap(WavOut.Width + FreqOut.Width + 30, WavOut.Height + WavIn.Height + 30);
                Graphics g = Graphics.FromImage(all);

                // white background
                g.Clear(Color.White);

                // set images
                g.DrawImage(WavIn.Image, new Point(10, 10));
                g.DrawImage(FreqIn.Image, new Point(WavIn.Width + 20, 10));

                g.DrawImage(WavOut.Image, new Point(10, WavIn.Height + 20));
                g.DrawImage(FreqOut.Image, new Point(WavOut.Width + 20, WavIn.Height + 20));

                saveImage(all);
            } else
                MessageBox.Show("Not all images were generated");
        }

        #endregion

        #region Play/load sound
        private void button2_Click(object sender, EventArgs e) {
            if (_soundPlayer.AmountOfLocations() > 0) {
                _soundPlayer.playFile(0);
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            _soundPlayer.stopSoud();
        }

        private void button5_Click(object sender, EventArgs e) {
            _soundPlayer.stopSoud();

            if (File.Exists("..\\temp.wav"))
                File.Delete("..\\temp.wav");

            _filehandeler.WriteFile("..\\temp.wav", _fourierData);
            if (_soundPlayer.AmountOfLocations() < 2)
                _soundPlayer.addLocation("..\\temp.wav");
            _soundPlayer.playFile(1);
        }

        private void button4_Click(object sender, EventArgs e) {
            _soundPlayer.stopSoud();
        }

        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (_soundPlayer.AmountOfLocations() == 2)
                File.Delete("..\\temp.wav");
        }
    }
}