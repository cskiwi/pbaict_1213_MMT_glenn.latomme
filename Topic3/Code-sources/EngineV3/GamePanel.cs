using System;
using System.Drawing;
using System.Windows.Forms;

namespace EngineV3 {
    public abstract class GamePanel : Panel {
        private const int DEFAULT_FPS = 50;

        private Timer _timer;
        private int _FPS;
        private DateTime _lastUpdatTime;
        private DateTime _currentUpdateTime;
        private SizeF _gameSize;

        public int FPS {
            get { return _FPS; }
            set {
                _FPS = value;
                _timer.Interval = (int)Math.Round((1f / (float)_FPS * 1000f));
            }
        }

        protected GamePanel(int fps, int width, int height)
            : base() {
            _timer = new Timer();

            this.DoubleBuffered = true;
            this.FPS = fps;

            _gameSize = new SizeF(width, height);
            _timer.Tick += new EventHandler(HandleUpdate);
            _timer.Start();
            _lastUpdatTime = DateTime.Now;
        }

        public GamePanel(int width, int height)
            : this(DEFAULT_FPS, width, height) {
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            OnPaint(_currentUpdateTime - _lastUpdatTime, e.Graphics);
        }

        protected abstract void OnTick(TimeSpan elapsedTime);
        protected abstract void OnPaint(TimeSpan elapsedTime, Graphics g);

        private void HandleUpdate(object sender, EventArgs e) {
            _currentUpdateTime = DateTime.Now;

            OnTick(_currentUpdateTime - _lastUpdatTime);
            this.Invalidate();

            _lastUpdatTime = _currentUpdateTime;
        }


        public static Size CalculateWindowBounds(Size gameRectangle, FormBorderStyle borderStyle) {
            Size s = gameRectangle;

            switch (borderStyle) {
                case FormBorderStyle.Sizable:
                    s.Width += SystemInformation.FrameBorderSize.Width * 2;
                    s.Height += SystemInformation.CaptionHeight + SystemInformation.FrameBorderSize.Height * 2;
                    break;
                case FormBorderStyle.FixedDialog:
                    s.Width += SystemInformation.FixedFrameBorderSize.Width * 2;
                    s.Height += SystemInformation.CaptionHeight + SystemInformation.FixedFrameBorderSize.Height * 2;
                    break;
            }

            return s;
        }
    }
}
