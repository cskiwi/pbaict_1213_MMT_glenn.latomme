using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine._2D;

namespace Soccer.Game {
    public static class Utils {
        public const double Pi = 3.14159;
        public static int MaxInt = Int32.MaxValue;
        public static double MaxDouble = Double.MaxValue;
        public static double MinDouble = Double.MinValue;
        public static float MaxFloat = Single.MaxValue;
        public static float MinFloat = Single.MinValue;

        public static double TwoPi = Pi*2;
        public static double HalfPi = Pi/2;
        public static double QuarterPi = Pi/4;
        private static double _randGaussianY2;
        private static int _randGaussianUseLast;

        #region utils

        public static double DegsToRads(double degs) {
            return TwoPi*(degs/360.0);
        }

        public static bool IsZero(double val) {
            return ((-MinDouble < val) && (val < MinDouble));
        }

        public static bool InRange(double start, double end, double val) {
            return start < end ? (val > start) && (val < end) : (val < start) && (val > end);
        }

        public static int RandInt(int x, int y) {
            Debug.Assert(y >= x, "<RandInt>: y is less than x");
            return RandomNumbers.NextNumber()%(y - x + 1) + x;
        }

        public static double RandFloat() {
            return ((RandomNumbers.NextNumber())/(Int32.MaxValue + 1.0));
        }

        public static double RandInRange(double x, double y) {
            return x + RandFloat()*(y - x);
        }

        public static bool RandBool() {
            return RandFloat() > 0.5;
        }

        public static double RandomClamped() {
            return RandFloat() - RandFloat();
        }

        public static double RandGaussian(double mean) {
            return RandGaussian(mean, 1.0);
        }

        public static double RandGaussian() {
            return RandGaussian(0.0, 1.0);
        }

        public static double RandGaussian(double mean, double standardDeviation) {
            double y1;

            if (_randGaussianUseLast != 0) {
                y1 = _randGaussianY2;
                _randGaussianUseLast = 0;
            }
            else {
                double x1, x2, w;
                do {
                    x1 = 2.0*RandFloat() - 1.0;
                    x2 = 2.0*RandFloat() - 1.0;
                    w = x1*x1 + x2*x2;
                } while (w >= 1.0);

                w = Math.Sqrt((-2.0*Math.Log(w))/w);
                y1 = x1*w;
                _randGaussianY2 = x2*w;
                _randGaussianUseLast = 1;
            }

            return (mean + y1*standardDeviation);
        }

        public static double Sigmoid(double input) {
            return Sigmoid(input, 1.0);
        }

        public static double Sigmoid(double input, double response) {
            return (1.0/(1.0 + Math.Exp(-input/response)));
        }

        public static T MaxOf<T>(T a, T b) where T : IComparable {
            if (a.CompareTo(b) > 0) return a;
            return b;
        }

        public static T MinOf<T>(T a, T b) where T : IComparable<T> {
            if (a.CompareTo(b) < 0) return a;
            return b;
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
            if (val.CompareTo(min) < 0) return min;
            return val.CompareTo(max) > 0 ? max : val;
        }

        //rounds a double up or down depending on its value
        public static int Rounded(double val) {
            var integral = (int) val;
            double mantissa = val - integral;

            if (mantissa < 0.5) return integral;

            return integral + 1;
        }

        //rounds a double up or down depending on whether its
        //mantissa is higher or lower than offset
        public static int RoundUnderOffset(double val, double offset) {
            var integral = (int) val;
            double mantissa = val - integral;

            if (mantissa < offset) return integral;

            return integral + 1;
        }

        //compares two real numbers. Returns true if they are equal
        public static bool IsEqual(float a, float b) {
            if (Math.Abs(a - b) < 1E-12) return true;

            return false;
        }

        public static bool IsEqual(double a, double b) {
            if (Math.Abs(a - b) < 1E-12) return true;

            return false;
        }

        public static double Average<T>(IList<T> items, Func<T, double> selector) {
            double average = items.Aggregate(0.0, (current, t) => current + selector(t));

            return average/items.Count;
        }

        public static void WrapAround(Vector2D pos, int maxX, int maxY) {
            if (pos.X > maxX) pos.X = 0.0;

            if (pos.Y < 0) pos.X = maxX;

            if (pos.Y < 0) pos.Y = maxY;

            if (pos.Y > maxY) pos.Y = 0.0;
        }

        public static bool NotInsideRegion(Vector2D p, Vector2D topLeft, Vector2D botRgt) {
            return (p.X < topLeft.X) || (p.X > botRgt.X) || (p.Y < topLeft.Y) || (p.Y > botRgt.Y);
        }

        public static bool InsideRegion(Vector2D p, Vector2D topLeft, Vector2D botRgt) {
            return !((p.X < topLeft.X) || (p.X > botRgt.X) || (p.Y < topLeft.Y) || (p.Y > botRgt.Y));
        }

        public static bool InsideRegion(Vector2D p, int left, int top, int right, int bottom) {
            return !((p.X < left) || (p.X > right) || (p.Y < top) || (p.Y > bottom));
        }

        public static bool IsSecondInFovOfFirst(Vector2D posFirst, Vector2D facingFirst, Vector2D posSecond, double fov) {
            Vector2D toTarget = Vector2D.Normalize(posSecond - posFirst);

            return facingFirst.Dot(toTarget) >= Math.Cos(fov/2.0);
        }

        #endregion utils

        #region Transformations

        public static List<Vector2D> WorldTransform(List<Vector2D> points, Vector2D pos, Vector2D forward, Vector2D side,
                                                    Vector2D scale) {
            //copy the original vertices into the buffer about to be transformed
            //C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
            //ORIGINAL LINE: ClassicVector<Vector2D> TranVector2Ds = points;
            List<Vector2D> tranVector2Ds = points;

            //create a transformation matrix
            var matTransform = new C2DMatrix();

            //scale
            if ((scale.X != 1.0) || (scale.Y != 1.0)) matTransform.Scale(scale.X, scale.Y);

            //rotate
            matTransform.Rotate(forward, side);

            //and translate
            matTransform.Translate(pos.X, pos.Y);

            //now transform the object's vertices
            matTransform.TransformVector2Ds(tranVector2Ds);

            return tranVector2Ds;
        }

        public static List<Vector2D> WorldTransform(List<Vector2D> points, Vector2D pos, Vector2D forward, Vector2D side) {
            List<Vector2D> tranVector2Ds = points;

            //create a transformation matrix
            var matTransform = new C2DMatrix();

            //rotate
            matTransform.Rotate(forward, side);

            //and translate
            matTransform.Translate(pos.X, pos.Y);

            //now transform the object's vertices
            matTransform.TransformVector2Ds(tranVector2Ds);

            return tranVector2Ds;
        }

        public static Vector2D PointToWorldSpace(Vector2D point, Vector2D agentHeading, Vector2D agentSide,
                                                 Vector2D agentPosition) {
            Vector2D transPoint = point;

            //create a transformation matrix
            var matTransform = new C2DMatrix();

            //rotate
            matTransform.Rotate(agentHeading, agentSide);

            //and translate
            matTransform.Translate(agentPosition.X, agentPosition.Y);

            //now transform the vertices
            matTransform.TransformVector2Ds(transPoint);

            return transPoint;
        }

        public static Vector2D VectorToWorldSpace(Vector2D vec, Vector2D agentHeading, Vector2D agentSide) {
            Vector2D transVec = vec;

            //create a transformation matrix
            var matTransform = new C2DMatrix();

            //rotate
            matTransform.Rotate(agentHeading, agentSide);

            //now transform the vertices
            matTransform.TransformVector2Ds(transVec);

            return transVec;
        }

        public static Vector2D PointToLocalSpace(Vector2D point, Vector2D agentHeading, Vector2D agentSide,
                                                 Vector2D agentPosition) {
            Vector2D transPoint = point;

            //create a transformation matrix
            var matTransform = new C2DMatrix();

            double tx = -agentPosition.Dot(agentHeading);
            double ty = -agentPosition.Dot(agentSide);

            //create the transformation matrix
            matTransform._11(agentHeading.X);
            matTransform._12(agentSide.X);
            matTransform._21(agentHeading.Y);
            matTransform._22(agentSide.Y);
            matTransform._31(tx);
            matTransform._32(ty);

            //now transform the vertices
            matTransform.TransformVector2Ds(transPoint);

            return transPoint;
        }

        public static Vector2D VectorToLocalSpace(Vector2D vec, Vector2D agentHeading, Vector2D agentSide) {
            Vector2D transPoint = vec;

            //create a transformation matrix
            var matTransform = new C2DMatrix();

            //create the transformation matrix
            matTransform._11(agentHeading.X);
            matTransform._12(agentSide.X);
            matTransform._21(agentHeading.Y);
            matTransform._22(agentSide.Y);

            //now transform the vertices
            matTransform.TransformVector2Ds(transPoint);

            return transPoint;
        }

        public static void Vec2DRotateAroundOrigin(Vector2D v, double ang) {
            //create a transformation matrix
            var mat = new C2DMatrix();

            //rotate
            mat.Rotate(ang);

            //now transform the object's vertices
            mat.TransformVector2Ds(v);
        }

        public static List<Vector2D> CreateWhiskers(uint numWhiskers, double whiskerLength, double fov, Vector2D facing,
                                                    Vector2D origin) {
            double sectorSize = fov/(numWhiskers - 1);

            var whiskers = new List<Vector2D>();
            double angle = -fov*0.5;

            for (uint w = 0; w < numWhiskers; ++w) {
                Vector2D temp = facing;
                Vec2DRotateAroundOrigin(temp, angle);
                whiskers.Add(origin + whiskerLength*temp);

                angle += sectorSize;
            }

            return whiskers;
        }

        #endregion Transformations

        #region Geometry

        public enum SpanType {
            PlaneBackside,
            PlaneFront,
            OnPlane
        };

        public static double DistanceToRayPlaneIntersection(Vector2D rayOrigin, Vector2D rayHeading, Vector2D planePoint,
                                                            Vector2D planeNormal) {
            //any point on the plane
            double d = -planeNormal.Dot(planePoint);
            double numer = planeNormal.Dot(rayOrigin) + d;
            double denom = planeNormal.Dot(rayHeading);

            // normal is parallel to vector
            if ((denom < 0.000001) && (denom > -0.000001)) return (-1.0);

            return -(numer/denom);
        }

        public static SpanType WhereIsPoint(Vector2D point, Vector2D PointOnPlane, Vector2D PlaneNormal) {
            //any point on the plane
            Vector2D dir = PointOnPlane - point;

            double d = dir.Dot(PlaneNormal);

            if (d < -0.000001) return SpanType.PlaneFront;

            else if (d > 0.000001) return SpanType.PlaneBackside;

            return SpanType.OnPlane;
        }

        //-------------------------- GetRayCircleIntersec -----------------------------
        public static double GetRayCircleIntersect(Vector2D rayOrigin, Vector2D rayHeading, Vector2D circleOrigin,
                                                   double radius) {
            Vector2D toCircle = circleOrigin - rayOrigin;
            double length = toCircle.Length();
            double v = toCircle.Dot(rayHeading);
            double d = radius*radius - (length*length - v*v);

            // If there was no intersection, return -1
            if (d < 0.0) return (-1.0);

            // Return the distance to the [first] intersecting point
            return (v - Math.Sqrt(d));
        }

        //----------------------------- DoRayCircleIntersect --------------------------
        public static bool DoRayCircleIntersect(Vector2D rayOrigin, Vector2D rayHeading, Vector2D circleOrigin,
                                                double radius) {
            Vector2D toCircle = circleOrigin - rayOrigin;
            double length = toCircle.Length();
            double v = toCircle.Dot(rayHeading);
            double d = radius*radius - (length*length - v*v);

            // If there was no intersection, return -1
            return (d < 0.0);
        }

        //------------------------------------------------------------------------
        //  Given a point P and a circle of radius R centered at C this function
        //  determines the two points on the circle that intersect with the
        //  tangents from P to the circle. Returns false if P is within the circle.
        //
        //  thanks to Dave Eberly for this one.
        //------------------------------------------------------------------------
        public static bool GetTangentPoints(Vector2D c, double r, Vector2D p, Vector2D t1, Vector2D t2) {
            Vector2D pmC = p - c;
            double sqrLen = pmC.LengthSq();
            double rSqr = r*r;
            if (sqrLen <= rSqr) {
                // P is inside or on the circle
                return false;
            }

            double invSqrLen = 1/sqrLen;
            double root = Math.Sqrt(Math.Abs(sqrLen - rSqr));

            t1.X = c.X + r*(r*pmC.X - pmC.Y*root)*invSqrLen;
            t1.Y = c.Y + r*(r*pmC.Y + pmC.X*root)*invSqrLen;
            t2.X = c.X + r*(r*pmC.X + pmC.Y*root)*invSqrLen;
            t2.Y = c.Y + r*(r*pmC.Y - pmC.X*root)*invSqrLen;

            return true;
        }

        //------------------------- DistToLineSegment ----------------------------
        //
        //  given a line segment AB and a point P, this function calculates the
        //  perpendicular distance between them
        //------------------------------------------------------------------------
        public static double DistToLineSegment(Vector2D a, Vector2D b, Vector2D p) {
            //if the angle is obtuse between PA and AB is obtuse then the closest
            //vertex must be A
            double dotA = (p.X - a.X)*(b.X - a.X) + (p.Y - a.Y)*(b.Y - a.Y);

            if (dotA <= 0) return a.Distance(p);

            //if the angle is obtuse between PB and AB is obtuse then the closest
            //vertex must be B
            double dotB = (p.X - b.X)*(a.X - b.X) + (p.Y - b.Y)*(a.Y - b.Y);

            if (dotB <= 0) return b.Distance(p);

            //calculate the point along AB that is the closest to P
            Vector2D point = a + ((b - a)*dotA)/(dotA + dotB);

            //calculate the distance P-Point
            return p.Distance(point);
        }

        //------------------------- DistToLineSegmentSq ----------------------------
        //
        //  as above, but avoiding sqrt
        //------------------------------------------------------------------------
        public static double DistToLineSegmentSq(Vector2D a, Vector2D b, Vector2D p) {
            //if the angle is obtuse between PA and AB is obtuse then the closest
            //vertex must be A
            double dotA = (p.X - a.X)*(b.X - a.X) + (p.Y - a.Y)*(b.Y - a.Y);

            if (dotA <= 0) return a.DistanceSq(p);

            //if the angle is obtuse between PB and AB is obtuse then the closest
            //vertex must be B
            double dotB = (p.X - b.X)*(a.X - b.X) + (p.Y - b.Y)*(a.Y - b.Y);

            if (dotB <= 0) return b.DistanceSq(p);

            //calculate the point along AB that is the closest to P
            Vector2D point = a + ((b - a)*dotA)/(dotA + dotB);

            //calculate the distance P-Point
            return p.DistanceSq(point);
        }

        //--------------------LineIntersection2D-------------------------
        //
        //	Given 2 lines in 2D space AB, CD this returns true if an
        //	intersection occurs.
        //
        //-----------------------------------------------------------------

        public static bool LineIntersection2D(Vector2D a, Vector2D b, Vector2D c, Vector2D d) {
            double rTop = (a.Y - c.Y)*(d.X - c.X) - (a.X - c.X)*(d.Y - c.Y);
            double sTop = (a.Y - c.Y)*(b.X - a.X) - (a.X - c.X)*(b.Y - a.Y);

            double bot = (b.X - a.X)*(d.Y - c.Y) - (b.Y - a.Y)*(d.X - c.X);

            if (bot == 0) {
                //parallel
                return false;
            }

            double invBot = 1.0/bot;
            double r = rTop*invBot;
            double s = sTop*invBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1)) {
                //lines intersect
                return true;
            }

            //lines do not intersect
            return false;
        }

        //--------------------LineIntersection2D-------------------------
        //
        //	Given 2 lines in 2D space AB, CD this returns true if an
        //	intersection occurs and sets dist to the distance the intersection
        //  occurs along AB
        //
        //-----------------------------------------------------------------

        public static bool LineIntersection2D(Vector2D a, Vector2D b, Vector2D c, Vector2D d, ref double dist) {
            double rTop = (a.Y - c.Y)*(d.X - c.X) - (a.X - c.X)*(d.Y - c.Y);
            double sTop = (a.Y - c.Y)*(b.X - a.X) - (a.X - c.X)*(b.Y - a.Y);

            double bot = (b.X - a.X)*(d.Y - c.Y) - (b.Y - a.Y)*(d.X - c.X);

            if (bot == 0) {
                //parallel
                return rTop.Equals(0) && sTop.Equals(0);
            }

            double r = rTop/bot;
            double s = sTop/bot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1)) {
                dist = a.Distance(b)*r;

                return true;
            }
            dist = 0;

            return false;
        }

        //-------------------- LineIntersection2D-------------------------
        //
        //	Given 2 lines in 2D space AB, CD this returns true if an
        //	intersection occurs and sets dist to the distance the intersection
        //  occurs along AB. Also sets the 2d vector point to the point of
        //  intersection
        //-----------------------------------------------------------------
        public static bool LineIntersection2D(Vector2D a, Vector2D b, Vector2D c, Vector2D d, ref double dist,
                                              ref Vector2D point) {
            double rTop = (a.Y - c.Y)*(d.X - c.X) - (a.X - c.X)*(d.Y - c.Y);
            double rBot = (b.X - a.X)*(d.Y - c.Y) - (b.Y - a.Y)*(d.X - c.X);

            double sTop = (a.Y - c.Y)*(b.X - a.X) - (a.X - c.X)*(b.Y - a.Y);
            double sBot = (b.X - a.X)*(d.Y - c.Y) - (b.Y - a.Y)*(d.X - c.X);

            if ((rBot == 0) || (sBot == 0)) {
                //lines are parallel
                return false;
            }

            double r = rTop/rBot;
            double s = sTop/sBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1)) {
                dist = a.Distance(b)*r;

                point = a + r*(b - a);

                return true;
            }
            dist = 0;

            return false;
        }

        //----------------------- ObjectIntersection2D ---------------------------
        //
        //  tests two polygons for intersection. *Does not check for enclosure*
        //------------------------------------------------------------------------
        public static bool ObjectIntersection2D(List<Vector2D> object1, List<Vector2D> object2) {
            for (int r = 0; r < object1.Count - 1; ++r) {
                for (int t = 0; t < object2.Count - 1; ++t) {
                    if (LineIntersection2D(object2[t], object2[t + 1], object1[r], object1[r + 1]))
                        return true;
                }
            }

            return false;
        }

        //----------------------------- TwoCirclesOverlapped ---------------------
        //
        //  Returns true if the two circles overlap
        //------------------------------------------------------------------------
        public static bool TwoCirclesOverlapped(double x1, double y1, double r1, double x2, double y2, double r2) {
            double distBetweenCenters = Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));

            if ((distBetweenCenters < (r1 + r2)) || (distBetweenCenters < Math.Abs(r1 - r2))) return true;

            return false;
        }

        //----------------------------- TwoCirclesOverlapped ---------------------
        //
        //  Returns true if the two circles overlap
        //------------------------------------------------------------------------
        public static bool TwoCirclesOverlapped(Vector2D c1, double r1, Vector2D c2, double r2) {
            double distBetweenCenters = Math.Sqrt((c1.X - c2.X)*(c1.X - c2.X) + (c1.Y - c2.Y)*(c1.Y - c2.Y));

            if ((distBetweenCenters < (r1 + r2)) || (distBetweenCenters < Math.Abs(r1 - r2))) return true;

            return false;
        }

        //--------------------------- TwoCirclesEnclosed ---------------------------
        //
        //  returns true if one circle encloses the other
        //-------------------------------------------------------------------------
        public static bool TwoCirclesEnclosed(double x1, double y1, double r1, double x2, double y2, double r2) {
            double distBetweenCenters = Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));

            if (distBetweenCenters < Math.Abs(r1 - r2)) return true;

            return false;
        }

        //------------------------ TwoCirclesIntersectionPoints ------------------
        //
        //  Given two circles this function calculates the intersection points
        //  of any overlap.
        //
        //  returns false if no overlap found
        //
        // see http://astronomy.swin.edu.au/~pbourke/geometry/2circle/
        //------------------------------------------------------------------------
        public static bool TwoCirclesIntersectionPoints(double x1, double y1, double r1, double x2, double y2, double r2,
                                                        ref double p3X, ref double p3Y, ref double p4X, ref double p4Y) {
            //first check to see if they overlap
            if (!TwoCirclesOverlapped(x1, y1, r1, x2, y2, r2)) return false;

            //calculate the distance between the circle centers
            double d = Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));

            //Now calculate the distance from the center of each circle to the center
            //of the line which connects the intersection points.
            double a = (r1 - r2 + (d*d))/(2*d);

            //MAYBE A TEST FOR EXACT OVERLAP?

            //calculate the point P2 which is the center of the line which
            //connects the intersection points

            double p2X = x1 + a*(x2 - x1)/d;
            double p2Y = y1 + a*(y2 - y1)/d;

            //calculate first point
            double h1 = Math.Sqrt((r1*r1) - (a*a));

            p3X = p2X - h1*(y2 - y1)/d;
            p3Y = p2Y + h1*(x2 - x1)/d;

            //calculate second point
            double h2 = Math.Sqrt((r2*r2) - (a*a));

            p4X = p2X + h2*(y2 - y1)/d;
            p4Y = p2Y - h2*(x2 - x1)/d;

            return true;
        }

        //------------------------ TwoCirclesIntersectionArea --------------------
        //
        //  Tests to see if two circles overlap and if so calculates the area
        //  defined by the union
        //
        // see http://mathforum.org/library/drmath/view/54785.html
        //-----------------------------------------------------------------------
        public static double TwoCirclesIntersectionArea(double x1, double y1, double r1, double x2, double y2, double r2) {
            //first calculate the intersection points
            double iX1 = 0;
            double iY1 = 0;
            double iX2 = 0;
            double iY2 = 0;

            if (!TwoCirclesIntersectionPoints(x1, y1, r1, x2, y2, r2, ref iX1, ref iY1, ref iX2, ref iY2))
                return 0.0; //no overlap

            //calculate the distance between the circle centers
            double d = Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));

            //find the angles given that A and B are the two circle centers
            //and C and D are the intersection points
            double cbd = 2*Math.Acos((r2*r2 + d*d - r1*r1)/(r2*d*2));

            double cad = 2*Math.Acos((r1*r1 + d*d - r2*r2)/(r1*d*2));

            //Then we find the segment of each of the circles cut off by the
            //chord CD, by taking the area of the sector of the circle BCD and
            //subtracting the area of triangle BCD. Similarly we find the area
            //of the sector ACD and subtract the area of triangle ACD.

            double area = 0.5f*cbd*r2*r2 - 0.5f*r2*r2*Math.Sin(cbd) + 0.5f*cad*r1*r1 - 0.5f*r1*r1*Math.Sin(cad);

            return area;
        }

        //-------------------------------- CircleArea ---------------------------
        //
        //  given the radius, calculates the area of a circle
        //-----------------------------------------------------------------------
        public static double CircleArea(double radius) {
            return Pi*radius*radius;
        }

        //----------------------- PointInCircle ----------------------------------
        //
        //  returns true if the point p is within the radius of the given circle
        //------------------------------------------------------------------------
        public static bool PointInCircle(Vector2D pos, double radius, Vector2D p) {
            double distFromCenterSquared = (p - pos).LengthSq();

            if (distFromCenterSquared < (radius*radius)) return true;

            return false;
        }

        //--------------------- LineSegmentCircleIntersection ---------------------------
        //
        //  returns true if the line segemnt AB intersects with a circle at
        //  position P with radius radius
        //------------------------------------------------------------------------
        public static bool LineSegmentCircleIntersection(Vector2D a, Vector2D b, Vector2D p, double radius) {
            //first determine the distance from the center of the circle to
            //the line segment (working in distance squared space)
            //C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
            //ORIGINAL LINE: double DistToLineSq = DistToLineSegmentSq(A, B, P);
            double distToLineSq = DistToLineSegmentSq(a, b, p);

            return distToLineSq < radius*radius;
        }

        //------------------- GetLineSegmentCircleClosestIntersectionPoint ------------
        //
        //  given a line segment AB and a circle position and radius, this function
        //  determines if there is an intersection and stores the position of the
        //  closest intersection in the reference IntersectionPoint
        //
        //  returns false if no intersection point is found
        //-----------------------------------------------------------------------------
        public static bool GetLineSegmentCircleClosestIntersectionPoint(Vector2D a, Vector2D b, Vector2D pos,
                                                                        double radius, ref Vector2D intersectionPoint) {
            Vector2D toBNorm = Vector2D.Normalize(b - a);

            //move the circle into the local space defined by the vector B-A with origin
            //at A
            Vector2D localPos = PointToLocalSpace(pos, toBNorm, toBNorm.Perp(), a);

            bool ipFound = false;

            //if the local position + the radius is negative then the circle lays behind
            //point A so there is no intersection possible. If the local x pos minus the
            //radius is greater than length A-B then the circle cannot intersect the
            //line segment
            if ((localPos.X + radius >= 0) &&
                ((localPos.X - radius)*(localPos.X - radius) <= b.DistanceSq(a))) {
                //if the distance from the x axis to the object's position is less
                //than its radius then there is a potential intersection.
                if (Math.Abs(localPos.Y) < radius) {
                    //now to do a line/circle intersection test. The center of the
                    //circle is represented by A, B. The intersection points are
                    //given by the formulae x = A +/-sqrt(r^2-B^2), y=0. We only
                    //need to look at the smallest positive value of x.
                    double posX = localPos.X;
                    double posY = localPos.Y;

                    double ip = posX - Math.Sqrt(radius*radius - posY*posY);

                    if (ip <= 0) ip = posX + Math.Sqrt(radius*radius - posY*posY);

                    ipFound = true;

                    intersectionPoint = a + toBNorm*ip;
                }
            }

            return ipFound;
        }

        #endregion Geometry
    }

    public static class Params {
        public static double GoalWidth = 100;

        //use to set up the sweet spot calculator
        public static double NumSweetSpotsX = 13;
        public static double NumSweetSpotsY = 6;

        //these values tweak the various rules used to calculate the support spots
        public static double Spot_CanPassScore = 2.0;
        public static double Spot_CanScoreFromPositionScore = 1.0;
        public static double Spot_DistFromControllingPlayerScore = 2.0;
        public static double Spot_ClosenessToSupportingPlayerScore = 0.0;
        public static double Spot_AheadOfAttackerScore = 0.0;

        //how many times per second the support spots will be calculated
        public static double SupportSpotUpdateFreq = 1;

        //the chance a player might take a random pot shot at the goal
        public static double ChancePlayerAttemptsPotShot = 0.005;

        //this is the chance that a player will receive a pass using the arrive
        //steering behavior, rather than Pursuit
        public static double ChanceOfUsingArriveTypeReceiveBehavior = 0.5;

        public static double BallSize = 5.0;
        public static double BallMass = 1.0;
        public static double Friction = -0.015;

        //the goalkeeper has to be this close to the ball to be able to interact with it
        public static double KeeperInBallRange = 10.0;
        public static double PlayerInTargetRange = 10.0;

        //player has to be this close to the ball to be able to kick it. The higher
        //the value this gets, the easier it gets to tackle. 
        public static double PlayerKickingDistance = 6.0;

        //the number of times a player can kick the ball per second
        public static double PlayerKickFrequency = 8;

        public static double PlayerMass = 3.0;
        public static double PlayerMaxForce = 1.0;
        public static double PlayerMaxSpeedWithBall = 1.2;
        public static double PlayerMaxSpeedWithoutBall = 1.6;
        public static double PlayerMaxTurnRate = 0.4;
        public static double PlayerScale = 1.0;

        //when an opponents comes within this range the player will attempt to pass
        //the ball. Players tend to pass more often, the higher the value
        public static double PlayerComfortZone = 60.0;

        //in the range zero to 1.0. adjusts the amount of noise added to a kick,
        //the lower the value the worse the players get.
        public static double PlayerKickingAccuracy = 0.99;

        //the number of times the SoccerTeam::CanShoot method attempts to find
        //a valid shot
        public static double NumAttemptsToFindValidStrike = 5;

        public static double MaxDribbleForce = 1.5;
        public static double MaxShootingForce = 6.0;
        public static double MaxPassingForce = 3.0;


        //the distance away from the center of its home region a player
        //must be to be considered at home
        public static double WithinRangeOfHome = 15.0;

        //how close a player must get to a sweet spot before he can change state
        public static double WithinRangeOfSweetSpot = 15.0;

        //the minimum distance a receiving player must be from the passing player
        public static double MinPassDistance = 120.0;
        //the minimum distance a player must be from the goalkeeper before it will
        //pass the ball
        public static double GoalkeeperMinPassDistance = 50.0;

        //this is the distance the keeper puts between the back of the net 
        //and the ball when using the interpose steering behavior
        public static double GoalKeeperTendingDistance = 20.0;

        //when the ball becomes within this distance of the goalkeeper he
        //changes state to intercept the ball
        public static double GoalKeeperInterceptRange = 100.0;

        //how close the ball must be to a receiver before he starts chasing it
        public static double BallWithinReceivingRange = 10.0;

        //these (boolean) values control the amount of player and pitch info shown
        //1=ON; 0=OFF
        public static bool ViewStates = true;
        public static bool ViewIDs = true;
        public static bool ViewSupportSpots = true;
        public static bool ViewRegions = false;
        public static bool BShowControllingTeam = true;
        public static bool ViewTargets = false;
        public static bool HighlightIfThreatened = false;

        //simple soccer's physics are calculated using each tick as the unit of time
        //so changing this will adjust the speed
        public static double FrameRate = 60;


        //steering behavior stuff
        public static double SeparationCoefficient = 10.0;

        //how close a neighbour must be to be considered for separation
        public static double ViewDistance = 30.0;

        //1=ON; 0=OFF
        public static bool bNonPenetrationConstraint = false;
    }
}