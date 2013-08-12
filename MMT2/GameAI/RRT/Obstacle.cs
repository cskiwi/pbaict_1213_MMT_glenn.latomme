using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameAI.RRT {
    public class Obstacle {
        public Obstacle(Point loc) {
            Point = loc;
            Radius = 75;
        }


        public Obstacle(Point loc, int rad) {
            Point = loc;
            Radius = rad;
        }

        public Obstacle() {}

        public Point Point { get; set; }
        public int Radius { get; set; }

        public Shape GetShape() {
            return new Ellipse {
                Width = Radius*2,
                Height = Radius*2,
                Margin = new Thickness(Point.X - Radius, Point.Y - Radius, 0, 0),
                Stroke = Brushes.DarkRed
            };
        }
    }
}