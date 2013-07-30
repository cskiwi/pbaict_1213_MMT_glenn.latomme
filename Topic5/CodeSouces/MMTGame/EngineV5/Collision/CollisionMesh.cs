using System.Drawing;

namespace EngineV5.Collision {
    public class CollisionMeshCircle : Collisions {
        public CollisionMeshCircle(PointF pos, int radius)
            : base(MeshType.Circle, pos, radius, 0) {}
    }

    public class CollisionMeshSquare : Collisions {
        public CollisionMeshSquare(PointF pos, int width, int height)
            : base(MeshType.Square, pos, width, height) {}
    }
}