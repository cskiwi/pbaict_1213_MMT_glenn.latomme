using System;
using System.Drawing;
using System.Windows.Forms;

namespace EngineV5 {
    public abstract class GamePanel : Panel {
        private readonly Timer _timer;
        private DateTime _currentUpdateTime;
        private float _fps;
        private DateTime _lastUpdateTime;

        protected GamePanel(float fps, int width, int height) {
            _timer = new Timer();
            DoubleBuffered = true;
            Fps = fps;

            Width = width;
            Height = height;

            _timer.Tick += HandleTick;
            _timer.Start();
            _currentUpdateTime = DateTime.Now;
            _lastUpdateTime = DateTime.Now;
        }

        public float Fps {
            get { return _fps; }
            set {
                _fps = value;
                _timer.Interval = (int) Math.Round((1f/_fps*1000f));
            }
        }

        protected abstract void OnTick(TimeSpan elapsedTime);
        protected abstract void OnPaint(Graphics g);

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            OnPaint(e.Graphics);
        }

        private void HandleTick(object sender, EventArgs e) {
            _currentUpdateTime = DateTime.Now;
            OnTick(_currentUpdateTime - _lastUpdateTime);
            Invalidate();
            _lastUpdateTime = _currentUpdateTime;
        }

        public static Size CalculateWindowBounds(Size gameRectangle) {
            Size s = gameRectangle;

            s.Width += SystemInformation.FrameBorderSize.Width*2;
            s.Height += SystemInformation.CaptionHeight + SystemInformation.FrameBorderSize.Height*2;

            return s;
        }
    }
}