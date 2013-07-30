using System;
using System.Drawing;

namespace EngineV3 {
    /// <summary>
    ///     Things that move by themself
    /// </summary>
    public abstract class DynamicGameObject : StaticGameObject {
        protected DynamicGameObject(RectangleF boundingBox, RectangleF rec, PointF vel, PointF acc, PointF fric)
            : base(boundingBox, rec) {
            Velocity = vel;
            Acceleration = acc;
            Friction = fric;
        }

        protected DynamicGameObject(RectangleF boundingBox, PointF pos, SizeF size, PointF vel, PointF acc, PointF fric)
            : base(boundingBox, pos, size) {
            Velocity = vel;
            Acceleration = acc;
        }

        public PointF Velocity { get; set; }
        public PointF Acceleration { get; set; }
        public PointF Friction { get; set; }


        public PointF NextPosition(TimeSpan elapsedTime) {
            return new PointF(
                Rectangle.X + (float) elapsedTime.TotalSeconds*Velocity.X,
                Rectangle.Y + (float) elapsedTime.TotalSeconds*Velocity.Y
                );
            // return new PointF(this.Rectangle.X + this.Velocity.X, this.Rectangle.Y + this.Velocity.Y);
        }

        public PointF NextVelocity(TimeSpan elapsedTime) {
            return new PointF(
                Velocity.X + (float) elapsedTime.TotalSeconds*Acceleration.X,
                Velocity.Y + (float) elapsedTime.TotalSeconds*Acceleration.Y
                );
        }

        public void ApplyFriction(TimeSpan elapsedTime) {
            var NewVelocity = new PointF();
            // veloctiy
            if (Math.Round(Velocity.X, 1) != 0)
                NewVelocity.X = Velocity.X + ((Velocity.X > 0 ? -1 : 1)*Friction.X);

            if (Math.Round(Velocity.Y, 1) != 0)
                NewVelocity.Y = Velocity.Y + ((Velocity.Y > 0 ? -1 : 1)*Friction.Y);

            /*
            PointF NewAcceleration = new PointF();
            
            // Acceleration (not needed for physics, just for lazy ness)
            if (Math.Round(this.Acceleration.X, 1) != 0)
                NewAcceleration.X = this.Acceleration.X + ((this.Acceleration.X > 0 ? -1 : 1) * (float)elapsedTime.TotalSeconds * this.Acceleration.X);

            if (Math.Round(this.Acceleration.Y, 1) != 0)
                NewAcceleration.Y = this.Acceleration.Y + ((this.Acceleration.Y > 0 ? -1 : 1) * (float)elapsedTime.TotalSeconds * this.Acceleration.Y);
            
            this.Acceleration = NewAcceleration;
             */
            Velocity = NewVelocity;
        }

        public void CheckCollision(DynamicGameObject Circle2, TimeSpan elapsedTime) {
            /* simple check
            var radius = this.Rectangle.Width / 2 + C2.Rectangle.Width/2;
            var deltaX = (this.Rectangle.X + this.Rectangle.Width / 2) - (C2.Rectangle.X + C2.Rectangle.Width / 2);
            var deltaY = (this.Rectangle.Y + this.Rectangle.Height / 2) - (C2.Rectangle.Y + C2.Rectangle.Height / 2);

            if ((deltaX * deltaX + deltaY * deltaY <= radius * radius)) {
                this.Velocity = new PointF(this.Velocity.X * -1, this.Velocity.Y * -1);
                C2.Velocity = new PointF(C2.Velocity.X * -1, C2.Velocity.Y * -1);
            }
            */

            // getting more advanced

            // distance between
            var locThis = new PointF(Rectangle.X + Rectangle.Width/2, Rectangle.Y + Rectangle.Height/2);
            var locC2 = new PointF(Circle2.Rectangle.X + Circle2.Rectangle.Width/2, Circle2.Rectangle.Y + Circle2.Rectangle.Height/2);

            var distance = new PointF(locThis.X - locC2.X, locThis.Y - locC2.Y);

            // magnitude
            double magnitude = Math.Sqrt(distance.X*distance.X + distance.Y*distance.Y);

            // if collision
            if (!(magnitude < Rectangle.Width/2 + Circle2.Rectangle.Width/2)) return;

            // vars
            PointF bTemp1 = new PointF(), bTemp2 = new PointF();
            PointF vTemp1 = new PointF(), vTemp2 = new PointF();
            PointF vFinal1 = new PointF(), vFinal2 = new PointF();
            PointF bFinal1 = new PointF(), bFinal2 = new PointF();

            // get angle
            double theta = Math.Atan2(distance.Y, distance.X);

            // calc sin and cosin
            var Sin = (float) Math.Sin(theta);
            var Cos = (float) Math.Cos(theta);

            // impact direction vector
            bTemp2.X = Cos*distance.X + Sin*distance.Y;
            bTemp2.Y = Cos*distance.Y - Sin*distance.X;

            // new Velocities
            vTemp1.X = Cos*Velocity.X + Sin*Velocity.Y;
            vTemp1.Y = Cos*Velocity.Y - Sin*Velocity.X;
            vTemp2.X = Cos*Circle2.Velocity.X + Sin*Circle2.Velocity.Y;
            vTemp2.Y = Cos*Circle2.Velocity.Y - Sin*Circle2.Velocity.X;

            // final velocities
            float m1 = Rectangle.Width/2*.1f;
            float m2 = Circle2.Rectangle.Width/2*.1f;

            vFinal1.X = ((m1 - m2)*vTemp1.X + 2*m2*vTemp2.X)/(m1 + m2);
            vFinal1.Y = vTemp1.Y;
            vFinal2.X = ((m2 - m1)*vTemp2.X + 2*m1*vTemp1.X)/(m1 + m2);
            vFinal2.Y = vTemp2.Y;

            // hack to avoid clumping
            bTemp1.X += vFinal1.X;
            bTemp2.X += vFinal2.X;

            // final positions
            bFinal1.X = (float) ((Cos*bTemp1.X - Sin*bTemp1.Y)*elapsedTime.TotalSeconds);
            bFinal1.Y = (float) ((Cos*bTemp1.Y + Sin*bTemp1.X)*elapsedTime.TotalSeconds);
            bFinal2.X = (float) ((Cos*bTemp2.X - Sin*bTemp2.Y)*elapsedTime.TotalSeconds);
            bFinal2.Y = (float) ((Cos*bTemp2.Y + Sin*bTemp2.X)*elapsedTime.TotalSeconds);

            Position = new PointF(locThis.X + bFinal1.X, locThis.Y + bFinal1.Y);
            Circle2.Position = new PointF(locThis.X + bFinal2.X, locThis.Y + bFinal2.Y);

            Velocity = new PointF(Cos*vTemp1.X - Sin*vTemp1.Y, Cos*vTemp1.Y + Sin*vTemp1.X);
            Circle2.Velocity = new PointF(Cos*vTemp2.X - Sin*vTemp2.Y, Cos*vTemp2.Y + Sin*vTemp2.X);
        }
    }
}