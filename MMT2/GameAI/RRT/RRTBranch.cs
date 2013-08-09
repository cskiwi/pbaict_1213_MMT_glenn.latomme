using System.Windows;

namespace GameAI.RRT {
    public class RrtBranch {
        public static int BranchLength = 50;

        public RrtBranch(Point loc, int parentID) {
            Location = loc;
            ParentID = parentID;
        }

        public RrtBranch() {}
        public Point Location { get; set; }
        public int ParentID { get; set; }

        public RrtBranch NextBranch(Point goalLoc, int parent) {
            var newBranch = new RrtBranch();
            var vector = new Vector(goalLoc.X - Location.X, goalLoc.Y - Location.Y);
            vector.Normalize();
            newBranch.Location = new Point(
                (int) (Location.X + (vector.X * BranchLength)),
                (int) (Location.Y + (vector.Y * BranchLength)));
            newBranch.ParentID = parent;
            return newBranch;
        }
    }
}