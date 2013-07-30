using System;
using System.Drawing;

namespace EngineV5 {
    public abstract class DynamicGameObject : StaticGameObject {
        protected DynamicGameObject(PointF pos, int speed) : base(pos) {
            Speed = speed;
        }

        public int Speed { get; set; }
        public PointF velocity { get; set; }

        public abstract void Tick(TimeSpan elapsedTime);
    }
}