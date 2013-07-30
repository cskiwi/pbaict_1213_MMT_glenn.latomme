namespace WAV_scrambler.BackEnd {

    public struct wavInfo {
        private string _format;
        private int _numChannels;
        private int _samplesRate;
        private int _bitsPerSample;
        private int _startDataBlock;
        private int _lengthDataBlock;

        public string Format {
            get { return _format; }
            set { _format = value; }
        }

        public int NumChannnels {
            get { return _numChannels; }
            set { _numChannels = value; }
        }

        public int SampleRate {
            get { return _samplesRate; }
            set { _samplesRate = value; }
        }

        public int BitsPerSample {
            get { return _bitsPerSample; }
            set { _bitsPerSample = value; }
        }

        public int StartDataBlock {
            get { return _startDataBlock; }
            set { _startDataBlock = value; }
        }

        public int LengthDataBlock {
            get { return _lengthDataBlock; }
            set { _lengthDataBlock = value; }
        }

        public override string ToString() {
            return
                "-----------------=[ " + _format + " ]=-----------------" + "\n" +
                " Number of channles:  " + _numChannels + "\n" +
                " Samples per second:  " + _samplesRate + "\n" +
                " Bits per sample:     " + _bitsPerSample + "\n" +
                " Start Data block:    " + _startDataBlock + "\n" +
                "-------------------------------------------"
                ;

            // return "# of channles: " + _numChannels + "\nSamples per sec: " + _samplesPerSec + "\nBit per sample: " + _bitsPerSample;
        }
    }
}