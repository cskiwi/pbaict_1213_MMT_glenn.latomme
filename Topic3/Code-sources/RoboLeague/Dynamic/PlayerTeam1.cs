using System.Drawing;
using RoboLeague.Dynamic;

namespace RoboLeague {
    internal class PlayerTeam1 : Player {
        public PlayerTeam1(PointF position, PointF startingAcceleration, float straal)
            : base(position, startingAcceleration, straal) {
            CircleColor = new SolidBrush(Color.Blue);
        }
    }
}