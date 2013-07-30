using System;
using System.Windows;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace REMPv2 {

    internal class Player {
        private IWavePlayer playbackDevice;
        private WaveStream fileStream;

        public void Load(String fileName) {
            Stop();
            CloseFile();
            EnsureDeviceCreated();
            OpenFile(fileName);
        }

        private void CloseFile() {
            if (fileStream != null) {
                fileStream.Dispose();
                fileStream = null;
            }
        }

        private void OpenFile(String fileName) {
            try {
                var inputStream = CreateInputStream(fileName);
                playbackDevice.Init(new SampleToWaveProvider(inputStream));
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Problem opening file");
                CloseFile();
            }
        }

        private ISampleProvider CreateInputStream(String fileName) {
            if (fileName.EndsWith(".wav")) {
                fileStream = OpenWavStream(fileName);
            } else if (fileName.EndsWith(".mp3")) {
                fileStream = new Mp3FileReader(fileName);
            } else {
                throw new InvalidOperationException("Unsupported extension");
            }
            var inputStream = new SampleChannel(fileStream, true);
            var sampleStream = new NotifyingSampleProvider(inputStream);
            return sampleStream;
        }

        private static WaveStream OpenWavStream(String fileName) {
            WaveStream readerStream = new WaveFileReader(fileName);
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm) {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }
            return readerStream;
        }

        private void EnsureDeviceCreated() {
            if (playbackDevice == null) {
                CreateDevice();
            }
        }

        private void CreateDevice() {
            playbackDevice = new WaveOut { DesiredLatency = 200 };
        }

        public void Play() {
            if (playbackDevice != null && fileStream != null && playbackDevice.PlaybackState != PlaybackState.Playing) {
                playbackDevice.Play();
            }
        }

        public void Pause() {
            if (playbackDevice != null) {
                playbackDevice.Pause();
            }
        }

        public void Stop() {
            if (playbackDevice != null) {
                playbackDevice.Stop();
            }
            if (fileStream != null) {
                fileStream.Position = 0;
            }
        }

        public String getPlayTime() {
            if (playbackDevice != null && fileStream != null) {
                TimeSpan currentTime = (playbackDevice.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : fileStream.CurrentTime;
                return String.Format("{0:00}:{1:00}", (int)currentTime.TotalMinutes, currentTime.Seconds);
            } else {
                return "00:00";
            }
        }

        public String getTotalPlayTime() {
            return String.Format("{0:00}:{1:00}", (int)fileStream.TotalTime.TotalMinutes, fileStream.TotalTime.Seconds);
        }

        public int currentSeconds() {
            if (playbackDevice != null && fileStream != null) {
                TimeSpan currentTime = (playbackDevice.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : fileStream.CurrentTime;
                return currentTime.Seconds + currentTime.Minutes * 60;
            } else {
                return -1;
            }
        }

        public int totalSeconds() {
            return (fileStream != null) ? fileStream.TotalTime.Seconds + fileStream.TotalTime.Minutes * 60 : -1;
        }

        public void setPlayTime(TimeSpan timespan) {
            if (playbackDevice != null && fileStream != null)
                fileStream.CurrentTime = timespan;
        }

        public void Dispose() {
            Stop();
            CloseFile();
            if (playbackDevice != null) {
                playbackDevice.Dispose();
                playbackDevice = null;
            }
        }

        public bool songLoaded() {
            return fileStream != null;
        }
    }
}