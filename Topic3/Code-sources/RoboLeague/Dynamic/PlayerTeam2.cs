using System.Drawing;
using RoboLeague.Dynamic;

namespace RoboLeague {
    internal class PlayerTeam2 : Player {
        public PlayerTeam2(PointF position, PointF startingAcceleration, float straal)
            : base(position, startingAcceleration, straal) {
            CircleColor = new SolidBrush(Color.Red);
        }
    }
}