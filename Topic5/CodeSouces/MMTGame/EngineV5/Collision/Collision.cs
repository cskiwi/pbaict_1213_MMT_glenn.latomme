using System;
using System.Drawing;

namespace EngineV5.Collision {
    public abstract class Collisions {
        public enum MeshType {
            Square,
            Circle
        }

        private readonly int _param1;
        private readonly int _param2;
        private readonly MeshType _type;

        /// <summary>
        ///     Creates an collision mesh
        /// </summary>
        /// <param name="type">Mesh Type</param>
        /// <param name="pos"></param>
        /// <param name="Param1"></param>
        /// <param name="Param2"></param>
        protected Collisions(MeshType type, PointF pos, int Param1, int Param2) {
            _type = type;
            Pos = pos;
            _param1 = Param1;
            _param2 = Param2;
        }

        internal PointF Pos { get; set; }

        public static bool CheckCollison(Collisions m1, Collisions m2) {
            if (m1._type == MeshType.Circle && m2._type == MeshType.Circle) {
                return ((m1.Pos.X - m2.Pos.X)*(m1.Pos.X - m2.Pos.X) +
                        (m1.Pos.Y - m2.Pos.Y)*(m1.Pos.Y - m2.Pos.Y) <=
                        (m1._param1 + m2._param1)*(m1._param1 + m2._param1));
            } else {
                Collisions obj1, obj2;
                if (m1._type != m2._type) {
                    obj1 = (m1._type == MeshType.Circle) ? m1 : m2;
                    obj2 = (m1._type == MeshType.Square) ? m1 : m2;
                }
                else {
                    obj1 = m1;
                    obj2 = m2;
                }

                var w = (float) (0.5*(obj1._param1 + obj2._param1));
                var h = (float) (0.5*(obj1._param1 + obj2._param2));
                float dx = obj1.Pos.X - obj2.Pos.X;
                float dy = obj1.Pos.Y - obj2.Pos.Y;

                if (Math.Abs(dx) <= w && Math.Abs(dy) <= h) {
                    float wy = w*dy;
                    float hx = h*dx;

                    if (wy > hx) {
                        if (wy > -hx) Console.WriteLine("collision at the top");
                        else
                            Console.WriteLine("collision on the left");
                    }
                    else if (wy > -hx)
                        Console.WriteLine("collision on the right ");
                    else
                        Console.WriteLine("collision at the bottom ");
                    return true;
                }
            }
            return false;
        }
    }
}