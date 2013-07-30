using System.Drawing;
using EngineV4;
using RoboLeague.Static;

namespace RoboLeague {
    internal class Field : StaticGameObject {
        private const int SpacingFieldWidth = 75;
        private const int SpacingFieldgHeight = 25;

        private readonly Goal _goal1;
        private readonly Goal _goal2;

        private readonly SolidBrush greenBrush = new SolidBrush(Color.DarkGreen);
        private readonly Pen whitePen = new Pen(Color.White);

        public Field(RectangleF rectangle) : base(rectangle) {
            _goal1 = new Goal(new RectangleF(SpacingFieldWidth - 50, Rec.Height/2 - 200/2, 50, 200), whitePen);
            _goal2 = new Goal(new RectangleF(Rec.Width - SpacingFieldWidth, Rec.Height/2 - 200/2, 50, 200), whitePen);
        }

        public override void Paint(Graphics g) {
            // background
            g.FillRectangle(greenBrush, 0, 0, Rec.Width, Rec.Height);

            // outher border

            g.DrawRectangle(whitePen,
                            SpacingFieldWidth,
                            SpacingFieldgHeight,
                            Rec.Width - SpacingFieldWidth*2,
                            Rec.Height - SpacingFieldgHeight*2);

            // center circle
            const int radiusCenter = 150;
            g.DrawEllipse(whitePen,
                          Rec.Width/2 - radiusCenter/2,
                          Rec.Height/2 - radiusCenter/2,
                          radiusCenter,
                          radiusCenter);

            // center line
            g.DrawLine(whitePen, Rec.Width/2,
                       SpacingFieldgHeight,
                       Rec.Width/2,
                       Rec.Height - SpacingFieldgHeight);

            // Goals
            _goal1.Paint(g);
            _goal2.Paint(g);
        }

        public RectangleF GetBoundingBox() {
            return new RectangleF(SpacingFieldWidth,
                                  SpacingFieldgHeight,
                                  Rec.Width - SpacingFieldWidth*2,
                                  Rec.Height - SpacingFieldgHeight*2);
        }

        public Goal GetGoal(int goalNr) {
            return goalNr == 1 ? _goal1 : _goal2;
        }
    }
}