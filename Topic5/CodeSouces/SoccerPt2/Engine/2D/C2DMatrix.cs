using System;
using System.Collections.Generic;
using Engine._2D;

public class C2DMatrix {
    private Matrix _Matrix = new Matrix();


    public C2DMatrix() {
        Identity();
    }

    private void MatrixMultiply(Matrix mIn) {
        var matTemp = new Matrix {
            _11 = (_Matrix._11*mIn._11) + (_Matrix._12*mIn._21) + (_Matrix._13*mIn._31),
            _12 = (_Matrix._11*mIn._12) + (_Matrix._12*mIn._22) + (_Matrix._13*mIn._32),
            _13 = (_Matrix._11*mIn._13) + (_Matrix._12*mIn._23) + (_Matrix._13*mIn._33),
            _21 = (_Matrix._21*mIn._11) + (_Matrix._22*mIn._21) + (_Matrix._23*mIn._31),
            _22 = (_Matrix._21*mIn._12) + (_Matrix._22*mIn._22) + (_Matrix._23*mIn._32),
            _23 = (_Matrix._21*mIn._13) + (_Matrix._22*mIn._23) + (_Matrix._23*mIn._33),
            _31 = (_Matrix._31*mIn._11) + (_Matrix._32*mIn._21) + (_Matrix._33*mIn._31),
            _32 = (_Matrix._31*mIn._12) + (_Matrix._32*mIn._22) + (_Matrix._33*mIn._32),
            _33 = (_Matrix._31*mIn._13) + (_Matrix._32*mIn._23) + (_Matrix._33*mIn._33)
        };

        //first row

        //second

        //third

        _Matrix = matTemp;
    }

    public void Identity() {
        _Matrix._11 = 1;
        _Matrix._12 = 0;
        _Matrix._13 = 0;

        _Matrix._21 = 0;
        _Matrix._22 = 1;
        _Matrix._23 = 0;

        _Matrix._31 = 0;
        _Matrix._32 = 0;
        _Matrix._33 = 1;
    }

    public void Translate(double x, double y) {
        var mat = new Matrix {_11 = 1, _12 = 0, _13 = 0, _21 = 0, _22 = 1, _23 = 0, _31 = x, _32 = y, _33 = 1};

        MatrixMultiply(mat);
    }

    //create a scale matrix

    //create a scale matrix
    public void Scale(double xScale, double yScale) {
        var mat = new Matrix();

        mat._11 = xScale;
        mat._12 = 0;
        mat._13 = 0;

        mat._21 = 0;
        mat._22 = yScale;
        mat._23 = 0;

        mat._31 = 0;
        mat._32 = 0;
        mat._33 = 1;

        //and multiply
        MatrixMultiply(mat);
    }

    //create a rotation matrix

    //create a rotation matrix
    public void Rotate(double rot) {
        var mat = new Matrix();

        double Sin = Math.Sin(rot);
        double Cos = Math.Cos(rot);

        mat._11 = Cos;
        mat._12 = Sin;
        mat._13 = 0;

        mat._21 = -Sin;
        mat._22 = Cos;
        mat._23 = 0;

        mat._31 = 0;
        mat._32 = 0;
        mat._33 = 1;

        //and multiply
        MatrixMultiply(mat);
    }

    //create a rotation matrix from a fwd and side 2D vector

    //create a rotation matrix from a 2D vector
    public void Rotate(Vector2D fwd, Vector2D side) {
        var mat = new Matrix();

        mat._11 = fwd.X;
        mat._12 = fwd.Y;
        mat._13 = 0;

        mat._21 = side.X;
        mat._22 = side.Y;
        mat._23 = 0;

        mat._31 = 0;
        mat._32 = 0;
        mat._33 = 1;

        MatrixMultiply(mat);
    }


    public void TransformVector2Ds(List<Vector2D> vPoint) {
        foreach (Vector2D t in vPoint) {
            double tempX = (_Matrix._11*t.X) + (_Matrix._21*t.Y) + (_Matrix._31);

            double tempY = (_Matrix._12*t.X) + (_Matrix._22*t.Y) + (_Matrix._32);

            t.X = tempX;

            t.Y = tempY;
        }
    }

    public void TransformVector2Ds(Vector2D vPoint) {
        double tempX = (_Matrix._11*vPoint.X) + (_Matrix._21*vPoint.Y) + (_Matrix._31);
        double tempY = (_Matrix._12*vPoint.X) + (_Matrix._22*vPoint.Y) + (_Matrix._32);

        vPoint.X = tempX;
        vPoint.Y = tempY;
    }

    public void _11(double val) {
        _Matrix._11 = val;
    }

    public void _12(double val) {
        _Matrix._12 = val;
    }

    public void _13(double val) {
        _Matrix._13 = val;
    }

    public void _21(double val) {
        _Matrix._21 = val;
    }

    public void _22(double val) {
        _Matrix._22 = val;
    }

    public void _23(double val) {
        _Matrix._23 = val;
    }

    public void _31(double val) {
        _Matrix._31 = val;
    }

    public void _32(double val) {
        _Matrix._32 = val;
    }

    public void _33(double val) {
        _Matrix._33 = val;
    }

    private class Matrix {
        public double _11;
        public double _12;
        public double _13;
        public double _21;
        public double _22;
        public double _23;
        public double _31;
        public double _32;
        public double _33;

        public Matrix() {
            _11 = 0.0;
            _12 = 0.0;
            _13 = 0.0;
            _21 = 0.0;
            _22 = 0.0;
            _23 = 0.0;
            _31 = 0.0;
            _32 = 0.0;
            _33 = 0.0;
        }
    }
}