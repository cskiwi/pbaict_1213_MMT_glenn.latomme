using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GamePhysics {
    public abstract class BallBase {
        protected Vector _vel;

        public virtual Vector Vel {
            get { return _vel; }
            set { _vel = value; }
        }

        public Point Pos { get; set; }

        public double Rad { get; set; }

        public abstract void Tick(double deltaTime, double width, double height);

        public List<Shape> Paint() {
            return new List<Shape> {
                // ball shape
                new Ellipse {
                    Fill = Brushes.DarkBlue,
                    Width = Rad*2,
                    Height = Rad*2,
                    Margin = new Thickness(Pos.X - Rad, Pos.Y - Rad, 0, 0)
                },
                // velocity line
                new Line {
                    X1 = Pos.X,
                    X2 = Pos.X + Vel.X,
                    Y1 = Pos.Y,
                    Y2 = Pos.Y + Vel.Y,
                    Stroke = Brushes.Red
                }
            };
        }
    }
}