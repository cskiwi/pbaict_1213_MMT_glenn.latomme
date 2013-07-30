using System.Drawing;

namespace EngineV4 {
    public abstract class StaticGameObject : IGameObject {
        protected StaticGameObject(RectangleF rectangle) {
            Rec = rectangle;
        }

        public RectangleF Rec { get; set; }


        public abstract void Paint(Graphics g);

        public void SetPosition(PointF pos) {
            Rec = new RectangleF(pos.X, pos.Y, Rec.Width, Rec.Height);
        }
    }
}