using System;
using System.Drawing;
using EngineV5;
using EngineV5.Collision;

namespace RoboLeague {
    public class Test : DynamicGameObject {
        private readonly int _param1;
        private readonly int _param2;

        public Test(PointF pos, int width, int height, int speed) : base(pos, speed) {
            _param2 = width;
            _param1 = height;
            CollisionMesh = new CollisionMeshSquare(pos, width, height);
            DrawColor = new SolidBrush(Color.Red);
        }

        public Test(PointF pos, int radius, int speed): base(pos, speed) {
            _param1 = radius;
            CollisionMesh = new CollisionMeshCircle(pos, radius);
            DrawColor = new SolidBrush(Color.Red);
        }

        public SolidBrush DrawColor { get; set; }

        public override void Paint(Graphics g) {
            if (CollisionMesh.GetType() == typeof(CollisionMeshSquare))
                g.FillRectangle(DrawColor, new RectangleF(CenterPos.X - _param2/2, CenterPos.Y - _param1/2, _param2, _param1));
            else
                g.FillEllipse(DrawColor, new RectangleF(CenterPos.X - _param1 / 2, CenterPos.Y - _param1 / 2, _param1, _param1));

        }

        public override void Tick(TimeSpan elapsedTime) {}
    }
}