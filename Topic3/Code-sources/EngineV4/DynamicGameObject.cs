using System;
using System.Drawing;

namespace EngineV4 {
    public abstract class DynamicGameObject : StaticGameObject {
        protected DynamicGameObject(RectangleF rec, PointF acceleration, float fric) : base(rec) {
            Velocity = new PointF(0, 0);
            Acceleration = acceleration;
            Friction = fric;
        }

        protected PointF Velocity { get; set; }
        protected PointF Acceleration { get; set; }
        private float Friction { get; set; }
        public abstract void Tick(TimeSpan elapsedTime);


        protected PointF NextPosition(TimeSpan elapsedTime) {
            return new PointF(Rec.X + (float) elapsedTime.TotalSeconds*Velocity.X,
                              Rec.Y + (float) elapsedTime.TotalSeconds*Velocity.Y);
        }

        protected PointF NextVelocity(TimeSpan elapsedTime) {
            return new PointF(Velocity.X*Friction + Acceleration.X,
                              Velocity.Y*Friction + Acceleration.Y);
        }

        // some static methods for easy use

        protected static PointF Normalize(PointF a) {
            var distance = (float) Math.Sqrt(a.X*a.X + a.Y*a.Y);
            distance = distance == 0 ? 1 : distance;
            return new PointF(a.X/distance, a.Y/distance);
        }

        protected static PointF Multiply(PointF a, PointF b) {
            return new PointF(a.X*b.X, a.Y*b.Y);
        }
    }
}