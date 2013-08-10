using System.Windows;

namespace GamePhysics {
    public class Ball : BallBase {
        public Ball(Point pos, double rad, Vector vel) {
            Pos = pos;
            Rad = rad;
            _vel = vel;
        }

        public override Vector Vel {
            get { return _vel; }
            set { _vel = value; }
        }


        public override void Tick(double deltaTime, double width, double height) {
            Pos = new Point(Pos.X + Vel.X*deltaTime/1000, Pos.Y + Vel.Y*deltaTime/1000);

            // if collision with one off the walls, inverse respective vel
            if ((Pos.X + Rad) > width && Vel.X > 0) Vel = new Vector(Vel.X*-1, Vel.Y);
            if ((Pos.Y + Rad) > height && Vel.Y > 0) Vel = new Vector(Vel.X, Vel.Y*-1);
            if ((Pos.X - Rad) < 0 && Vel.X < 0) Vel = new Vector(Vel.X*-1, Vel.Y);
            if ((Pos.Y - Rad) < 0 && Vel.Y < 0) Vel = new Vector(Vel.X, Vel.Y*-1);
        }
    }
}