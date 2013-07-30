using System;
using Engine._2D;

namespace Soccer.Game {
    public class Region {
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

        protected int Iid;
        protected Vector2D MvCenter;


        public Region() {
            DTop = 0;
            DBottom = 0;
            DLeft = 0;
            DRight = 0;
        }

        public Region(double left, double top, double right, double bottom)
            : this(left, top, right, bottom, -1) {}

        //C++ TO C# CONVERTER NOTE: Overloaded method(s) are created above to convert the following method having default parameters:
        //ORIGINAL LINE: Region(double left, double top, double right, double bottom, int id = -1):DTop(top), DRight(right), DLeft(left), DBottom(bottom), m_iID(id)
        public Region(double left, double top, double right, double bottom, int id) {
            DTop = top;
            DRight = right;
            DLeft = left;
            DBottom = bottom;
            Iid = id;
            MvCenter = new Vector2D((left + right)*0.5, (top + bottom)*0.5);

            DWidth = Math.Abs(right - left);
            DHeight = Math.Abs(bottom - top);
        }

        public virtual void Dispose() {}

        public virtual void Render() {
            Render(false);
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: virtual inline void Render(bool ShowID = 0)const
        //C++ TO C# CONVERTER NOTE: Overloaded method(s) are created above to convert the following method having default parameters:
        public virtual void Render(bool showID) {
            /*Cgdi.Instance().HollowBrush();
            Cgdi.Instance().GreenPen();
            Cgdi.Instance().Rect(DLeft, DTop, DRight, DBottom);

            if (ShowID) {
                Cgdi.Instance().TextColor(Cgdi.AnonymousEnum.green);
                Cgdi.Instance().TextAtPos(Center(), GlobalMembersRegion.ttos(ID()));
            }*/
        }

        //returns true if the given position lays inside the region. The
        //region modifier can be used to contract the region bounderies
        public bool Inside(Vector2D pos) {
            return Inside(pos, RegionModifier.Normal);
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: inline bool Inside(Vector2D pos, region_modifier r=normal)const
        //C++ TO C# CONVERTER NOTE: Overloaded method(s) are created above to convert the following method having default parameters:
        public bool Inside(Vector2D pos, RegionModifier r) {
            if (r == RegionModifier.Normal)
                return ((pos.X > DLeft) && (pos.X < DRight) && (pos.Y > DTop) && (pos.Y < DBottom));
            double marginX = DWidth*0.25;
            double marginY = DHeight*0.25;

            return ((pos.X > (DLeft + marginX)) && (pos.X < (DRight - marginX)) && (pos.Y > (DTop + marginY)) &&
                    (pos.Y < (DBottom - marginY)));
        }

        //returns a vector representing a random location
        //within the region
        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: inline Vector2D GetRandomPosition()const
        public Vector2D GetRandomPosition() {
            return new Vector2D(RandomNumbers.RandInRange(DLeft, DRight),
                                RandomNumbers.RandInRange(DTop, DBottom));
        }

        //-------------------------------
        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: double Top()const
        public double Top() {
            return DTop;
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: double Bottom()const
        public double Bottom() {
            return DBottom;
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: double Left()const
        public double Left() {
            return DLeft;
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: double Right()const
        public double Right() {
            return DRight;
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: double Width()const
        public double Width() {
            return Math.Abs(DRight - DLeft);
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: double Height()const
        public double Height() {
            return Math.Abs(DTop - DBottom);
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: double Length()const
        public double Length() {
            return Width() > Height() ? Width() : Height();
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: double Breadth()const
        public double Breadth() {
            return Width() < Height() ? Width() : Height();
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: Vector2D Center()const
        public Vector2D Center() {
            return MvCenter;
        }

        //C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
        //ORIGINAL LINE: int ID()const
        public int ID() {
            return Iid;
        }
    }
}