using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GamePhysics {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const int Milisec = 20;
        private readonly List<BallBase> _ball;
        private readonly DispatcherTimer _timer;
        private bool _prevDownKeyPressed;
        private bool _prevUpKeyPressed;

        public MainWindow() {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Tick += timer_Tick;
            _timer.Interval = TimeSpan.FromMilliseconds(Milisec);
            _timer.Start();

            var rand = new Random();
            _ball = new List<BallBase> {
                new StaticBall(new Point(100, 100), 20),
                new StaticBall(new Point(300, 100), 20)
            };

            // add some balls with random radius and veloctiy
            for (int i = 0; i < 15; i++) {
                _ball.Add(
                    new Ball(
                        new Point(45 + i*40, 250),
                        rand.Next()%20 + 20,
                        new Vector(rand.Next()%100 - 50.5, rand.Next()%100 - 50.5)
                        )
                    );
            }
        }

        private void timer_Tick(object sender, EventArgs e) {
            // Tick the balls
            foreach (BallBase ball in _ball)
                ball.Tick(Milisec, DrawCanvas.Width, DrawCanvas.Height);

            // calculate collision
            HandleCollisions();

            // dubble ball 1's vel by pressing keys
            if (Keyboard.IsKeyDown(Key.Up) && _prevUpKeyPressed == false)
                _prevUpKeyPressed = true;
            if (Keyboard.IsKeyUp(Key.Up) && _prevUpKeyPressed) {
                foreach (BallBase ball in _ball) if (ball.GetType() != typeof(StaticBall)) ball.Vel = new Vector(ball.Vel.X * 1.3, ball.Vel.Y * 1.3);
                _prevUpKeyPressed = false;
            }
            if (Keyboard.IsKeyDown(Key.Down) && _prevDownKeyPressed == false)
                _prevDownKeyPressed = true;
            if (Keyboard.IsKeyUp(Key.Down) && _prevDownKeyPressed) {
                foreach (BallBase ball in _ball) if (ball.GetType() != typeof(StaticBall)) ball.Vel = new Vector(ball.Vel.X / 1.3, ball.Vel.Y / 1.3);
                _prevDownKeyPressed = false;
            }

            // when done call paint
            Paint();
        }

        private void Paint() {
            DrawCanvas.Children.Clear();
            foreach (Shape shape in _ball.SelectMany(ball => ball.Paint()))
                DrawCanvas.Children.Add(shape);
        }

        private void HandleCollisions() {
            for (int i = 0; i < _ball.Count; ++i) {
                for (int j = i + 1; j < _ball.Count; ++j) //let op  j=i+1
                {
                    // calculate distance between centers of balls
                    double distance = (_ball[i].Pos - _ball[j].Pos).Length;
                    // calculate sum of Radius
                    double sumRadii = _ball[i].Rad + _ball[j].Rad;
                    // check collision if dist<sum of diameters 
                    if (distance < sumRadii)
                        Bounce(i, j);
                }
            }
        }

        private void Bounce(int ball1, int ball2) {
            // vector that connects between both balls
            Vector centerNorm = (_ball[ball1].Pos - _ball[ball2].Pos);
            centerNorm.Normalize();

            // Project the velocity vector on the centerNorm
            Vector projVelocity1 = Dot(centerNorm, _ball[ball1].Vel)*centerNorm;
            Vector projVelocity2 = Dot(centerNorm, _ball[ball2].Vel)*centerNorm;

            // if in same direction
            if (Dot(projVelocity1, projVelocity2) > 0) {
                // if the first one is moving faster than the second, don't interfere
                // first one is identified by dot with centerNorm
                if (Dot(centerNorm, projVelocity1) > 0) {
                    if (projVelocity1.Length > projVelocity2.Length) return;
                    if (projVelocity1.Length < projVelocity2.Length) return;
                }
            }
                // they are not moving in the same direction
            else if (Dot(centerNorm, projVelocity1) > 0) return;

            // calculate tangent
            Vector tangentVelocity1 = _ball[ball1].Vel - projVelocity1;
            Vector tangentVelocity2 = _ball[ball2].Vel - projVelocity2;

            // New vel is sum own tangent and projection of the other
            Vector newVelocity1 = tangentVelocity1 + projVelocity2;
            Vector newVelocity2 = tangentVelocity2 + projVelocity1;

            // collision with static ball is like colliding with a wall
            if (_ball[ball1].GetType() == typeof (StaticBall)) newVelocity2 = tangentVelocity2 - _ball[ball2].Vel;
            else if (_ball[ball2].GetType() == typeof (StaticBall)) newVelocity1 = tangentVelocity1 - _ball[ball1].Vel;

            // assign new velocities
            _ball[ball1].Vel = newVelocity1;
            _ball[ball2].Vel = newVelocity2;
        }


        // formule for Dot product
        private static double Dot(Vector v1, Vector v2) {
            return (v1.X*v2.X + v1.Y*v2.Y);
        }
    }
}