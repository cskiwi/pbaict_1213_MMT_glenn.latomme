using System;
using System.Collections.Generic;

namespace Engine._2D {
    public class Vector2D {
        public double X;
        public double Y;

        public Vector2D() {
            X = 0.0;
            Y = 0.0;
        }

        public Vector2D(double a, double b) {
            X = a;
            Y = b;
        }

        public override int GetHashCode() {
            unchecked {
                return (X.GetHashCode()*397) ^ Y.GetHashCode();
            }
        }

        protected bool Equals(Vector2D other) {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Vector2D) obj);
        }

        public void Zero() {
            X = 0.0;
            Y = 0.0;
        }

        public static double MaxOf(Vector2D val) {
            return val.X > val.Y ? val.X : val.Y;
        }

        public bool IsZero() {
            return (X*X + Y*Y) < double.MinValue;
        }

        public double Length() {
            return Math.Sqrt(X*X + Y*Y);
        }

        public double LengthSq() {
            return (X*X + Y*Y);
        }

        public static Vector2D Normalize(Vector2D vector) {
            double vectorLength = vector.Length();

            if (vectorLength > double.Epsilon) {
                vector.X /= vectorLength;
                vector.Y /= vectorLength;
            }
            return vector;
        }


        public double Dot(Vector2D v2) {
            return X*v2.X + Y*v2.Y;
        }

        public double Sign(Vector2D v2) {
            if (Y*v2.X > X*v2.Y) return (double) Rotation.Anticlockwise;
            return (double) Rotation.Clockwise;
        }

        public Vector2D Perp() {
            return new Vector2D(-Y, X);
        }

        public Vector2D Truncate(Vector2D vector, double max) {
            if (Length() > max) {
                vector = Normalize(vector);

                vector *= max;
            }
            return vector;
        }


        public double Distance(Vector2D v2) {
            double ySeparation = v2.Y - Y;
            double xSeparation = v2.X - X;

            return Math.Sqrt(ySeparation*ySeparation + xSeparation*xSeparation);
        }

        public double DistanceSq(Vector2D v2) {
            double ySeparation = v2.Y - Y;
            double xSeparation = v2.X - X;

            return ySeparation*ySeparation + xSeparation*xSeparation;
        }


        public Vector2D GetReverse() {
            return new Vector2D(-X, -Y);
        }

        public static Vector2D operator +(Vector2D l, Vector2D r) {
            return new Vector2D(l.X += r.X, l.Y += r.X);
        }

        public static Vector2D operator -(Vector2D l, Vector2D r) {
            return new Vector2D(l.X -= r.X, l.Y -= r.X);
        }

        public static Vector2D operator *(Vector2D l, Vector2D r) {
            return new Vector2D(l.X *= r.X, l.Y *= r.X);
        }

        public static Vector2D operator *(double l, Vector2D r) {
            return new Vector2D(l*r.X, l*r.Y);
        }

        public static Vector2D operator *(Vector2D r, double l) {
            return new Vector2D(l*r.X, l*r.Y);
        }

        public static Vector2D operator /(Vector2D l, Vector2D r) {
            return new Vector2D(l.X /= r.X, l.Y /= r.X);
        }
        public static Vector2D operator /(Vector2D l, double r) {
            return new Vector2D(l.X /= r, l.Y /= r);
        }
        public static Vector2D operator /(double r, Vector2D l) {
            return new Vector2D(l.X /= r, l.Y /= r);
        }

        public Vector2D Reflect(Vector2D norm) {
            return this + (2.0 * Dot(norm) * norm.GetReverse());
        }

        public static bool operator ==(Vector2D l, Vector2D r) {
            if (ReferenceEquals(l, r))
                return true;
            if (ReferenceEquals(l, null) ||
                ReferenceEquals(r, null))
                return false;

            return Equals(l.X, r.X) && Equals(l.Y, r.Y);
        }

        public static bool operator !=(Vector2D l, Vector2D r) {
            return !(l == r);
        }
    }

    public enum Rotation {
        Clockwise = 1,
        Anticlockwise = -1
    }
}