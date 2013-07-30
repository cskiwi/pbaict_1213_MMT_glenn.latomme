using System;
using System.Drawing;
using EngineV4;
using RoboLeague.Static;

namespace RoboLeague {
    public class Voetball : DynamicGameObject {
        private readonly Random _rnd;
        private bool _shoot;

        public Voetball(PointF pos, PointF startingAcceleration, float fric, float straal)
            : base(new RectangleF(pos.X, pos.Y, straal*2, straal*2), startingAcceleration, fric) {
            _rnd = new Random();
        }

        public SolidBrush Color { get; set; }

        public override void Paint(Graphics g) {
            g.FillEllipse(Color, new RectangleF(Rec.X - Rec.Width/2, Rec.Y - Rec.Height/2, Rec.Width, Rec.Height));
        }

        public override void Tick(TimeSpan elapsedTime) {
            Velocity = NextVelocity(elapsedTime);
            SetPosition(NextPosition(elapsedTime));

            if (_shoot) {
                _shoot = false;
                Acceleration = new PointF(0, 0);
            }
        }

        internal bool CheckCollision(Goal goal, TimeSpan elapsedTime) {
            PointF nextPosition = NextPosition(elapsedTime);
            RectangleF goalRec = goal.GetBoundingBox();


            return nextPosition.X - Rec.Width/2 <= goalRec.X + goalRec.Width &&
                   nextPosition.X + Rec.Width/2 >= goalRec.X &&
                   nextPosition.Y - Rec.Height/2 <= goalRec.Y + goalRec.Height &&
                   nextPosition.Y + Rec.Height/2 >= goalRec.Y;
        }

        internal void CheckCollision(RectangleF rectangle, TimeSpan elapsedTime) {
            PointF newVelocity = NextVelocity(elapsedTime);
            PointF nextPosition = NextPosition(elapsedTime);

            // Check X
            if (nextPosition.X - Rec.Width/2 < rectangle.X) {
                nextPosition.X = rectangle.X + Rec.Width/2;
                newVelocity.X *= -1;
            }
            else if (nextPosition.X + Rec.Width/2 > rectangle.Width + rectangle.X) {
                nextPosition.X = rectangle.Width + rectangle.X - Rec.Width/2;
                newVelocity.X *= -1;
            }

            // Check Y
            if (nextPosition.Y - Rec.Height/2 < rectangle.Y) {
                nextPosition.Y = rectangle.Y + Rec.Height/2;
                newVelocity.Y *= -1;
            }
            else if (nextPosition.Y + Rec.Height/2 > rectangle.Height + rectangle.Y) {
                nextPosition.Y = rectangle.Height + rectangle.Y - Rec.Height/2;
                newVelocity.Y *= -1;
            }

            Velocity = newVelocity;
            SetPosition(nextPosition);
        }

        public void Shoot(PointF directionGoal) {
            float centerX = directionGoal.X - Rec.X;
            var direction = new PointF(_rnd.Next((int) centerX - 50, (int) centerX + 50), directionGoal.Y - Rec.Y);
            PointF normalized = Normalize(direction);
            Acceleration = Multiply(normalized, new PointF(200, 200));
            _shoot = true;
        }
    }
}