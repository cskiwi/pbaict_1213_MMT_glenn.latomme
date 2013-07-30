using System;
using System.Drawing;
using System.Windows.Forms;
using EngineV5;
using EngineV5.Collision;

namespace RoboLeague {
    internal class RoboLeague : GamePanel {
        private ArrowsPressed arrowsPressed;
        private Test test, test2;

        public RoboLeague(float fps, int width, int height)
            : base(fps, width, height) {
            Init();
        }

        private void Init() {
            test = new Test(new PointF(100, 100), 100, 20);
            test.DrawColor = new SolidBrush(Color.Blue);
            

            test2 = new Test(new PointF(150, 205), 50, 100, 20);
            test2.DrawColor = new SolidBrush(Color.Green);

        }

        protected override void OnTick(TimeSpan elapsedTime) {
            test.Tick(elapsedTime);
            test2.Tick(elapsedTime);


            if (Collisions.CheckCollison(test.CollisionMesh, test2.CollisionMesh))
                Console.WriteLine("Hit");
        }

        protected override void OnPaint(Graphics g) {
            test.Paint(g);
            test2.Paint(g);
        }

        internal void GameForm_KeyDown(object sender, KeyEventArgs e) {
            base.OnKeyDown(e);
            switch (e.KeyCode) {
                case Keys.Down:
                    ChangeArrowsState(ArrowsPressed.Down, true);
                    break;
                case Keys.Up:
                    ChangeArrowsState(ArrowsPressed.Up, true);
                    break;
                case Keys.Left:
                    ChangeArrowsState(ArrowsPressed.Left, true);
                    break;
                case Keys.Right:
                    ChangeArrowsState(ArrowsPressed.Right, true);
                    break;
                default:
                    return;
            }
            HandleArrows();
            e.Handled = true;
        }

        internal void GameForm_KeyUp(object sender, KeyEventArgs e) {
            base.OnKeyUp(e);
            switch (e.KeyCode) {
                case Keys.Down:
                    ChangeArrowsState(ArrowsPressed.Down, false);
                    break;
                case Keys.Up:
                    ChangeArrowsState(ArrowsPressed.Up, false);
                    break;
                case Keys.Left:
                    ChangeArrowsState(ArrowsPressed.Left, false);
                    break;
                case Keys.Right:
                    ChangeArrowsState(ArrowsPressed.Right, false);
                    break;
                default:
                    return;
            }
        }

        internal void HandleArrows() {
            int X = 0, Y = 0, speed = 5;
            if ((arrowsPressed & ArrowsPressed.Down) != ArrowsPressed.None) Y += speed;
            if ((arrowsPressed & ArrowsPressed.Up) != ArrowsPressed.None) Y -= speed;
            if ((arrowsPressed & ArrowsPressed.Left) != ArrowsPressed.None) X -= speed;
            if ((arrowsPressed & ArrowsPressed.Right) != ArrowsPressed.None) X += speed;

            test2.CenterPos = new PointF(test2.CenterPos.X + X, test2.CenterPos.Y + Y);
        }


        internal void ChangeArrowsState(ArrowsPressed changed, bool isPressed) {
            if (isPressed) arrowsPressed |= changed;
            else arrowsPressed &= ArrowsPressed.All ^ changed;
        }

        [Flags]
        internal enum ArrowsPressed {
            None = 0x00,
            Left = 0x01,
            Right = 0x02,
            Up = 0x04,
            Down = 0x08,
            All = 0x0F
        }
    }
}