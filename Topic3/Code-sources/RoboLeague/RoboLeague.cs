using System;
using System.Collections.Generic;
using System.Drawing;
using EngineV4;
using RoboLeague.Dynamic;

namespace RoboLeague {
    internal class RoboLeague : GamePanel {
        private readonly Field _field;
        private readonly List<Player> _players;
        private Voetball _voetball;

        public RoboLeague(float fps, int width, int height)
            : base(fps, width, height) {
            // rnd = new Random();
            // drawFont = new Font("Arial", 10);
            _players = new List<Player>();
            _field = new Field(new RectangleF(0, 0, width, height));

            Initalize();
        }

        private void Initalize() {
            _voetball = new Voetball(new PointF(Width/2, Height/2), new PointF(0, 0), 1f, 15) {
                Color = new SolidBrush(Color.Black)
            };

            //ShootAt(2);

            // players
            _players.Add(new PlayerTeam1(new PointF(100, 100), new PointF(5, 5), 25));
            _players.Add(new PlayerTeam2(new PointF(200, 200), new PointF(5, 5), 25));
        }


        protected override void OnTick(TimeSpan elapsedTime) {
            _voetball.Tick(elapsedTime);


            foreach (Player player in _players) {
                player.LookAt(_voetball);
                player.Tick(elapsedTime);
            }

            Collisions(elapsedTime);
        }

        private void Collisions(TimeSpan elapsedTime) {
            if (_voetball.CheckCollision(_field.GetGoal(1), elapsedTime)) Reset();
            if (_voetball.CheckCollision(_field.GetGoal(2), elapsedTime)) Reset();


            _voetball.CheckCollision(_field.GetBoundingBox(), elapsedTime);
            foreach (Player player in _players) player.CheckCollision(new RectangleF(0, 0, Width, Height), elapsedTime);

            _players[0].CheckCollision(_players[1]);
        }

        protected override void OnPaint(Graphics g) {
            _field.Paint(g);

            _voetball.Paint(g);

            foreach (Player player in _players) player.Paint(g);
        }

        private void ShootAt(int goal) {
            _voetball.Shoot(GoalCenter(goal));
        }

        private PointF GoalCenter(int goal) {
            RectangleF bound = _field.GetGoal(goal).GetBoundingBox();
            return new PointF(bound.X, bound.Y + bound.Height/2);
        }

        private void Reset() {
            _voetball.SetPosition(new PointF(Width/2, Height/2));
            _players.Clear();
            
            _players.Add(new PlayerTeam1(new PointF(100, 100), new PointF(0, 0), 25));
            _players.Add(new PlayerTeam2(new PointF(200, 200), new PointF(0, 0), 25));
        }
    }
}