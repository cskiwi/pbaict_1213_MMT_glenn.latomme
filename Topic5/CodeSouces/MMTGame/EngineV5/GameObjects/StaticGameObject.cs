using System.Drawing;
using EngineV5.Collision;

namespace EngineV5 {
    public abstract class StaticGameObject : IGameObject {
        protected StaticGameObject(PointF pos) {
            CenterPos = pos;
        }

        public Collisions CollisionMesh { get; set; }

        public PointF _centerPos;

        public PointF CenterPos {
            get {
                return _centerPos;
            }
            set {
                _centerPos = value;
                if (CollisionMesh != null)
                    CollisionMesh.Pos = value;
            }
        }

        public abstract void Paint(Graphics g);

        public void SetPosition(PointF pos) {
            CenterPos = new PointF(pos.X, pos.Y);
        }
    }
}