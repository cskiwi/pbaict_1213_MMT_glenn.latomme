using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineV4;

namespace RoboLeague.Static {
    class Goal : StaticGameObject {
        private readonly Pen _pen; 
        public Goal(RectangleF rectangle, Pen pen) : base(rectangle) {
            _pen = pen;
        }

        public override void Paint(Graphics g) {
            g.DrawRectangle(_pen, RectangleConverter(Rec));
        }

        private Rectangle RectangleConverter(RectangleF Rec) {
            return new Rectangle((int) Rec.X, (int) Rec.Y, (int) Rec.Width, (int) Rec.Height);
        }

        public RectangleF GetBoundingBox() {
            return Rec;
        }
    }
}
