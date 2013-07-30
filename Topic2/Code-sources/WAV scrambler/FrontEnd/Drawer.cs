using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WAV_scrambler.FrontEnd {

    internal class Drawer {
        private double _wavXcale;
        private double _wavYcale;
        private double _freqXcale;
        private double _freqYcale;

        public Drawer() {
            ResetScaling();
        }
        /// <summary>
        /// Creates an wav image
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Heighte of the image</param>
        /// <param name="data">Your wav data</param>
        /// <returns>Bitmap with wav in getekend</returns>
        public Bitmap makeWavGraph(int width, int height, float[] data, int channels, int length) {
            Bitmap bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);

            _wavXcale = length / width / channels;

            // splitting the channels
            List<float> leftChannel = new List<float>();

            // if 2 channels skip every one value, because It should be the same, and it saves some computing power
            if (channels == 2)
                for (int i = 0; i < length; i += 2)
                    leftChannel.Add(data[i]);
            else
                for (int i = 0; i < length; i++)
                    leftChannel.Add(data[i]);

            // splitting the values in positive and negative taking
            List<float> fixedSizeDataPos = new List<float>();
            List<float> fixedSizeDataNeg = new List<float>();

            float[] dataSizeForFixign = new float[(int)_wavXcale * (width + 1)];
            Array.Copy(leftChannel.ToArray(), dataSizeForFixign, leftChannel.Count);

            for (int i = 0; i < width; i++) {
                float[] takeavarage = new float[(int)_wavXcale];
                Array.Copy(dataSizeForFixign, i * (int)_wavXcale, takeavarage, 0, (int)_wavXcale);

                if (takeavarage.Count(x => x >= 0) > 0)
                    fixedSizeDataPos.Add(takeavarage.Where(x => x >= 0).Average());
                else
                    fixedSizeDataPos.Add(0);
                if (takeavarage.Count(x => x < 0) > 0)
                    fixedSizeDataNeg.Add(takeavarage.Where(x => x < 0).Average());
                else
                    fixedSizeDataNeg.Add(0);
            }

            // save var, so no need to search whole list again;
            double min = fixedSizeDataNeg.Min();
            double max = fixedSizeDataPos.Max();

            // find scale values if not set yet
            if (_wavYcale == 0)
                _wavYcale = (height - 50) / 2 / max;

            g.Clear(Color.White);
            using (var pen = new Pen(Color.DarkBlue)) {
                for (int i = 0; i < fixedSizeDataPos.Count; i++) {
                    double val1 = fixedSizeDataPos[i] * _wavYcale;
                    double val2 = height / 2 - val1;
                    g.DrawLine(pen, i, height / 2, i, (int)val2);
                }
                for (int i = 0; i < fixedSizeDataNeg.Count; i++) {
                    double val1 = fixedSizeDataNeg[i] * _wavYcale;
                    double val2 = height / 2 - val1;
                    g.DrawLine(pen, i, height / 2, i, (int)val2);
                }
            }

            // box it
            g.DrawRectangle(new Pen(Color.Black), 0, 0, width - 1, height - 1);

            return bitmap;
        }

        /// <summary>
        /// Creates an frequentie image
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Heighte of the image</param>
        /// <param name="data">Your FFT data</param>
        /// <returns>Bitmap with freq in getekend</returns>
        public Bitmap makeFreqGraph(int width, int height, float[] data) {
            List<double> modulus = new List<double>();
            Bitmap bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);

            for (int i = 0; i < data.Length / 2; i += 2) {
                Complex c = new Complex(data[i], data[i + 1]);
                modulus.Add(c.GetModulus());
            }

            _freqXcale = modulus.Count / width;

            // make an new array, with an size that can be handeld
            double[] modulusFix = new double[(int)_freqXcale * (width + 1)];
            Array.Copy(modulus.ToArray(), modulusFix, modulus.Count);
            List<double> limitedModuls = new List<double>();
            for (int i = 0; i < width; i++) {
                double[] takeavarage = new double[(int)_freqXcale];
                Array.Copy(modulusFix, i * (int)_freqXcale, takeavarage, 0, (int)_freqXcale);
                limitedModuls.Add(takeavarage.Average());
            }

            // save var, so no need to search whole list again;
            double min = limitedModuls.Min();
            double max = limitedModuls.Max();

            // Console.WriteLine("max: " + max + ", min: " + min);

            // find scale values
            if (_freqYcale == 0)
                _freqYcale = height / max;
            g.Clear(Color.White);
            using (var pen = new Pen(Color.DarkBlue)) {
                for (int i = 0; i < limitedModuls.Count; i++) {
                    double val1 = limitedModuls[i] * _freqYcale;
                    double val2 = height - val1;
                    g.DrawLine(pen, i, height, i, (int)val2);
                }
            }

            // add a grid
            g.DrawImage(drawGridLine(width, height, 5), 0, 0);

            // box it
            g.DrawRectangle(new Pen(Color.Black), 0, 0, width - 1, height - 1);

            return bitmap;
        }

        /// <summary>
        /// Draws a grid
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Heighte of the image</param>
        /// <param name="amountofBoxes">Amount of boxes in both axes</param>
        /// <returns>Image</returns>
        private Bitmap drawGridLine(int width, int height, int amountofBoxes) {
            Bitmap bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);

            float scaleLinesHorz = height / amountofBoxes;
            float scaleLinesVert = width / amountofBoxes;

            using (var pen = new Pen(Color.Black)) {
                for (int i = 1; i < amountofBoxes; i++) {
                    g.DrawLine(pen, 0, i * scaleLinesHorz, width, i * scaleLinesHorz);
                }
                for (int i = 1; i < amountofBoxes; i++) {
                    g.DrawLine(pen, i * scaleLinesVert, 0, i * scaleLinesVert, height);
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Resets the scaling
        /// </summary>
        public void ResetScaling() {
            _wavXcale = 0;
            _wavYcale = 0;
            _freqXcale = 0;
            _freqYcale = 0;
        }
    }
}