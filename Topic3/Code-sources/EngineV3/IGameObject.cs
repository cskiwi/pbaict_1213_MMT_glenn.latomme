using System;
using System.Drawing;

namespace EngineV3 {

    public interface IGameObject {

        void Paint(TimeSpan elapsedTime, Graphics g);

        void Tick(TimeSpan elapsedTime);
    }
}