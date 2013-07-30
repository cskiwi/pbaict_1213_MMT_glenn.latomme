using System;
using System.Drawing;

namespace EngineV3 {

    /// <summary>
    /// Things that don't move by them self
    /// </summary>
    public abstract class StaticGameObject : IGameObject {
        private RectangleF _boundingBox;

        protected RectangleF BoundingBox {
            get { return _boundingBox; }
        }

        public Color objectColor { get; set; }

        public RectangleF Rectangle { get; set; }

        public PointF Position {
            get { return this.Rectangle.Location; }
            set { this.Rectangle = new RectangleF(value, this.Rectangle.Size); }
        }

        public SizeF Size {
            get { return this.Rectangle.Size; }
            set { this.Rectangle = new RectangleF(this.Rectangle.Location, value); }
        }

        public StaticGameObject(RectangleF boundingBox, RectangleF rec) {
            _boundingBox = boundingBox;
            Rectangle = rec;
        }

        public StaticGameObject(RectangleF boundingBox, PointF pos, SizeF size)
            : this(boundingBox, new RectangleF(pos, size)) {
        }

        public abstract void Paint(TimeSpan elapsedTime, System.Drawing.Graphics g);

        public abstract void Tick(TimeSpan elapsedTime);
    }
}