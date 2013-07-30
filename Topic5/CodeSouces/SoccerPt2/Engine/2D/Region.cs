using System;
using Engine._2D;

namespace Soccer.Game {
    internal class Region {
        public enum RegionModifier {
            Halfsize,
            Normal
        }

        protected double DBottom;
        protected double DHeight;

        protected double DLeft;
        protected double DRight;
        protected double DTop;

        protected double DWidth;

        protected int Id;
        protected Vector2D VCenter = new Vector2D();


        public Region() {
            DTop = 0;
            DBottom = 0;
            DLeft = 0;
            DRight = 0;
        }

        public Region(double left, double top, double right, double bottom)
            : this(left, top, right, bottom, -1) {}

        public Region(double left, double top, double right, double bottom, int id) {
            DTop = top;
            DRight = right;
            DLeft = left;
            DBottom = bottom;
            Id = id;
            //calculate center of region
            VCenter = new Vector2D((left + right)*0.5, (top + bottom)*0.5);

            DWidth = Math.Abs(right - left);
            DHeight = Math.Abs(bottom - top);
        }

        public virtual void Dispose() {}

        public virtual void Render() {}

        public virtual void Render(bool showId) {
            /*
            Cgdi.Instance().HollowBrush();
            Cgdi.Instance().GreenPen();
            Cgdi.Instance().Rect(m_dLeft, m_dTop, m_dRight, m_dBottom);

            if (ShowID) {
                Cgdi.Instance().TextColor(Cgdi.AnonymousEnum.green);
                Cgdi.Instance().TextAtPos(Center(), GlobalMembersRegion.ttos(ID()));
            }*/
        }

        public bool Inside(Vector2D pos) {
            return Inside(pos, RegionModifier.Normal);
        }

        public bool Inside(Vector2D pos, RegionModifier r) {
            if (r == RegionModifier.Normal)
                return ((pos.X > DLeft) && (pos.X < DRight) && (pos.Y > DTop) && (pos.Y < DBottom));

            double marginX = DWidth*0.25;
            double marginY = DHeight*0.25;

            return ((pos.X > (DLeft + marginX)) && (pos.X < (DRight - marginX)) && (pos.Y > (DTop + marginY)) &&
                    (pos.Y < (DBottom - marginY)));
        }

        public Vector2D GetRandomPosition() {
            return new Vector2D(RandomNumbers.RandInRange(DLeft, DRight),
                                RandomNumbers.RandInRange(DTop, DBottom));
        }

        public double Top() {
            return DTop;
        }

        public double Bottom() {
            return DBottom;
        }

        public double Left() {
            return DLeft;
        }

        public double Right() {
            return DRight;
        }

        public double Width() {
            return Math.Abs(DRight - DLeft);
        }

        public double Height() {
            return Math.Abs(DTop - DBottom);
        }

        public double Length() {
            return Width() > Height() ? Width() : Height();
        }

        public double Breadth() {
            return Width() < Height() ? Width() : Height();
        }

        public Vector2D Center() {
            return VCenter;
        }

        public int ID() {
            return Id;
        }
    }
}