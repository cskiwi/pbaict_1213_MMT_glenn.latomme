using System.Windows;

namespace GamePhysics {
    public class StaticBall : BallBase {
        public StaticBall(Point pos, double rad) {
            Pos = pos;
            Rad = rad;
            _vel = new Vector(0, 0);
        }

        public override Vector Vel {
            get { return _vel; }
            set { }
        }


        public override void Tick(double deltaTime, double width, double height) {}
    }
}