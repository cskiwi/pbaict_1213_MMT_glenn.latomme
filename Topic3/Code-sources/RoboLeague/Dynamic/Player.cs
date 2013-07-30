using System;
using System.Drawing;
using EngineV4;

namespace RoboLeague.Dynamic {
    internal class Player : DynamicGameObject {
        private PointF _ballLocation;
        private PointF _startlocation;

        protected Player(PointF pos, PointF startingAcceleration, float straal)
            : base(new RectangleF(pos.X, pos.Y, straal*2, straal*2), startingAcceleration, 1f) {
            _startlocation = pos;
        }

        protected SolidBrush CircleColor { private get; set; }

        public override void Paint(Graphics g) {
            g.FillEllipse(CircleColor, new RectangleF(Rec.X - Rec.Width/2, Rec.Y - Rec.Height/2, Rec.Width, Rec.Height));
        }

        public override void Tick(TimeSpan elapsedTime) {
            PointF nextPos = NextPosition(elapsedTime);
            var distance = new PointF(_startlocation.X - _ballLocation.X, _startlocation.Y - _ballLocation.Y);
            double magnitude = Math.Sqrt(distance.X*distance.X + distance.Y*distance.Y);

            // todo fix bug here !!
            PointF target = Multiply(magnitude > 100
                                         ? Normalize(new PointF(_ballLocation.X - nextPos.X, _ballLocation.Y - nextPos.Y))
                                         : Normalize(new PointF(Rec.X - nextPos.X, Rec.Y - nextPos.Y))
                                     , new PointF(10, 10));

            Acceleration = target;

            Velocity = NextVelocity(elapsedTime);
            SetPosition(NextPosition(elapsedTime));
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

        internal void CheckCollision(Player player) {
            // just for easy reading
            float thisRadius = Rec.Width/2;
            float thatRadius = player.Rec.Width/2;

            // get locations
            var thisLoc = new PointF(Rec.X - Rec.Width/2, Rec.Y - Rec.Height/2);
            var thatLoc = new PointF(player.Rec.X - player.Rec.Width/2, player.Rec.Y - player.Rec.Height/2);

            // calculate distance
            var distance = new PointF(thisLoc.X - thatLoc.X, thisLoc.Y - thatLoc.Y);

            // calculate magintude
            double magnitude = Math.Sqrt(distance.X*distance.X + distance.Y*distance.Y);

            // check for collision
            if ((magnitude < thisRadius + thatRadius) == false)
                return;

            // Get angle between 2 centerpoints
            double theta = Math.Atan2(distance.Y, distance.X);

            // Cos and Sin
            var sin = (float) Math.Sin(theta);
            var cos = (float) Math.Cos(theta);

            /*// Impact direction vector
            var ImpactVector = new PointF {
                X = cos*distance.X + sin*distance.Y,
                Y = cos*distance.Y + sin*distance.X
            };*/

            // new velocities
            var vTemp1 = new PointF {
                X = cos*Velocity.X + sin*Velocity.Y,
                Y = cos*Velocity.Y - sin*Velocity.X
            };

            var vTemp2 = new PointF {
                X = cos*player.Velocity.X + sin*player.Velocity.Y,
                Y = cos*player.Velocity.Y - sin*player.Velocity.X
            };

            // final velocities
            float m1 = thisRadius*.1f;
            float m2 = thatRadius*.1f;

            var vFinal1 = new PointF {
                X = ((m1 - m2)*vTemp1.X + 2*m2*vTemp2.X)/(m1 + m2),
                Y = vTemp1.Y
            };
            var vFinal2 = new PointF {
                X = ((m2 - m1)*vTemp2.X + 2*m1*vTemp1.X)/(m1 + m2),
                Y = vTemp2.Y
            };

            Velocity = vFinal2;
            player.Velocity = vFinal1;
        }

        public void LookAt(Voetball ball) {
            _ballLocation = new PointF(ball.Rec.X, ball.Rec.Y);
        }
    }
}