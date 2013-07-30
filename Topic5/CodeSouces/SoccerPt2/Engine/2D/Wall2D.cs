namespace Engine._2D {
    public class Wall2D {
        protected Vector2D _VA = new Vector2D();
        protected Vector2D _VB = new Vector2D();
        protected Vector2D _VN = new Vector2D();


        public Wall2D() {}

        public Wall2D(Vector2D a, Vector2D b) {
            _VA = a;
            _VB = b;
            CalculateNormal();
        }

        public Wall2D(Vector2D a, Vector2D b, Vector2D n) {
            _VA = a;
            _VB = b;
            _VN = n;
        }

        protected void CalculateNormal() {
            Vector2D temp = Vector2D.Normalize(_VB - _VA);

            _VN.X = -temp.Y;
            _VN.Y = temp.X;
        }

        public Vector2D Normal() {
            return _VN;
        }

        public Vector2D From() {
            return _VA;
        }
        public Vector2D To() {
            return _VB;
        }
    }
}