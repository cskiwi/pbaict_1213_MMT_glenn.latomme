using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameAI.RRT {
    public class RrtGenerator {
        private readonly int _height;
        private readonly Random _random;
        private readonly List<RrtBranch> _rrtTree;
        private readonly Stopwatch _stopWatch;
        private readonly int _width;
        private Point _newPoint;
        private List<Obstacle> _obstacles;

        public RrtGenerator(int height, int width) {
            _height = height;
            _width = width;
            _rrtTree = new List<RrtBranch>();
            _random = new Random();
            _stopWatch = new Stopwatch();

            FinalPath = new List<RrtBranch>();
        }

        public List<RrtBranch> FinalPath { get; set; }

        public string LastGenerationTime { get; set; }

        public Point Start { get; set; }
        public Point Target { get; set; }

        private int GetClosestID(Point target) {
            int closest = -1;
            double closestDistance = -1;
            for (int i = 0; i < _rrtTree.Count; i++) {
                if (closest != -1) {
                    double d = CalculateDistance(_rrtTree[i].Location, target);
                    if (d < closestDistance) {
                        closestDistance = d;
                        closest = i;
                    }
                }
                else {
                    closest = i;
                    closestDistance = CalculateDistance(_rrtTree[i].Location, target);
                }
            }
            return closest;
        }

        public static double CalculateDistance(Point p1, Point p2) {
            return Math.Sqrt((p1.X - p2.X)*(p1.X - p2.X) +
                             (p1.Y - p2.Y)*(p1.Y - p2.Y));
        }

        private void GenerateTree() {
            _stopWatch.Start();
            bool targetReached = false;
            if (CheckCollisionWorldObjects(Start) || CheckCollisionWorldObjects(Target)) MessageBox.Show("Start or end point is overlapping with obstacle, no path is generated");
            else {
                while (targetReached == false) {
                    // generate point
                    _newPoint = new Point(_random.Next(0, _width), _random.Next(0, _height));

                    // get closest branch in tree
                    int closest = GetClosestID(_newPoint);

                    // create a branch towards that parent branch
                    RrtBranch branch = _rrtTree[closest].NextBranch(_newPoint, closest);

                    // check collisions
                    if (CheckCollisionWithTree(branch)) continue;
                    if (CheckCollisionWorldObjects(branch)) continue;

                    // add to tree
                    _rrtTree.Add(branch);

                    // check if close enough to target
                    if (!CheckIfCloseEnoughToTarget(branch)) continue;

                    // add final piece
                    _rrtTree.Add(branch.NextBranch(Target, _rrtTree.Count - 1));
                    _rrtTree[_rrtTree.Count - 1].Location = Target;
                    targetReached = true;
                }


                // final path
                int currentBranch = _rrtTree.Count - 1;
                while (currentBranch != 0) {
                    FinalPath.Add(_rrtTree[currentBranch]);
                    currentBranch = _rrtTree[currentBranch].ParentID;
                }

                _stopWatch.Stop();
                LastGenerationTime = _stopWatch.Elapsed.ToString();
            }
        }

        private bool CheckIfCloseEnoughToTarget(RrtBranch branch) {
            return CalculateDistance(branch.Location, Target) < RrtBranch.BranchLength;
        }

        private bool CheckCollisionWithTree(RrtBranch point) {
            return _rrtTree.Any(rrtBranch => CheckLineCollsion(
                new Point(rrtBranch.Location.X, rrtBranch.Location.Y),
                new Point(_rrtTree[rrtBranch.ParentID].Location.X, _rrtTree[rrtBranch.ParentID].Location.Y),
                new Point(point.Location.X, point.Location.Y),
                new Point(_rrtTree[point.ParentID].Location.X, _rrtTree[point.ParentID].Location.Y)));
        }

        public bool CheckCollisionWorldObjects(Point point) {
            return (from obstacle in _obstacles
                    let distance = Math.Sqrt((obstacle.Point.X - point.X)*(obstacle.Point.X - point.X) + (obstacle.Point.Y - point.Y)*(obstacle.Point.Y - point.Y))
                    where distance <= obstacle.Radius
                    select obstacle).Any();
        }


        public bool CheckCollisionWorldObjects(RrtBranch branch) {
            foreach (Obstacle obstacle in _obstacles) {
                Point lineStart = branch.Location;
                Point lineEnd = _rrtTree[branch.ParentID].Location;
                Point circle = obstacle.Point;

                // find the closest point on the line segment to the center of the circle
                Vector line = lineEnd - lineStart;
                double lineLength = line.Length;
                Vector lineNorm = (1/lineLength)*line;
                Vector segmentToCircle = circle - lineStart;
                double closestPointOnSegment = Dot(segmentToCircle, line)/lineLength;

                // Special cases where the closest point happens to be the end points
                Point closest;
                if (closestPointOnSegment < 0) closest = lineStart;
                else if (closestPointOnSegment > lineLength) closest = lineEnd;
                else closest = lineStart + closestPointOnSegment*lineNorm;

                // Find that distance.  If it is less than the radius, then we 
                // are within the circle
                Vector distanceFromClosest = circle - closest;
                double distanceFromClosestLength = distanceFromClosest.Length;
                if (distanceFromClosestLength < obstacle.Radius) return true;
            }
            return false;
        }

        private static bool CheckLineCollsion(Point p0, Point p1, Point p2, Point p3) {
            if (p0 == p1 || p0 == p2 || p0 == p3 || p1 == p2 || p1 == p3 || p2 == p3) return false;
            double s1X = p1.X - p0.X;
            double s1Y = p1.Y - p0.Y;
            double s2X = p3.X - p2.X;
            double s2Y = p3.Y - p2.Y;

            double s = (-s1Y*(p0.X - p2.X) + s1X*(p0.Y - p2.Y))/(-s2X*s1Y + s1X*s2Y);
            double t = (s2X*(p0.Y - p2.Y) - s2Y*(p0.X - p2.X))/(-s2X*s1Y + s1X*s2Y);

            return (s >= 0 && s <= 1 && t >= 0 && t <= 1);
        }

        /// <summary>
        ///     Calculates a path between start and target with RRT alogrithm
        /// </summary>
        /// <param name="newRandomTarget">Generate a random target location</param>
        /// <param name="newRandomStart">Generate a random start location</param>
        /// <param name="obstacle"></param>
        public void CalculateRrt(bool newRandomTarget, bool newRandomStart, List<Obstacle> obstacle) {
            _obstacles = obstacle;
            _rrtTree.Clear();
            FinalPath.Clear();


            if (newRandomTarget) {
                Target = new Point(_random.Next(0, _width), _random.Next(0, _height));
                if (obstacle != null) {
                    while (CheckCollisionWorldObjects(Target))
                        Target = new Point(_random.Next(0, _width), _random.Next(0, _height));
                }
            }


            if (newRandomStart) {
                Start = new Point(_random.Next(0, _width), _random.Next(0, _height));
                if (obstacle != null) {
                    while (CheckCollisionWorldObjects(Start))
                        Start = new Point(_random.Next(0, _width), _random.Next(0, _height));
                }
            }
            _rrtTree.Add(new RrtBranch(Start, 0));
            GenerateTree();
        }

        /// <summary>
        ///     Gives a list of all the objects of the tree
        /// </summary>
        /// <returns></returns>
        public List<Shape> GetTreeShapes() {
            // add tree
            List<Shape> objects = _rrtTree.Select(rrtBranch => new Line {
                Stroke = Brushes.Black,
                X1 = rrtBranch.Location.X,
                X2 = _rrtTree[rrtBranch.ParentID].Location.X,
                Y1 = rrtBranch.Location.Y,
                Y2 = _rrtTree[rrtBranch.ParentID].Location.Y,
                StrokeThickness = 1
            }).Cast<Shape>().ToList();

            // overdraw finalPath with blue lines
            objects.AddRange(FinalPath.Select(rrtBranch => new Line {
                Stroke = Brushes.Blue,
                X1 = rrtBranch.Location.X,
                X2 = _rrtTree[rrtBranch.ParentID].Location.X,
                Y1 = rrtBranch.Location.Y,
                Y2 = _rrtTree[rrtBranch.ParentID].Location.Y,
                StrokeThickness = 3
            }));

            // draw target
            var elipse = new Ellipse {
                Fill = Brushes.Red,
                Width = 10,
                Height = 10,
                Margin = new Thickness(Target.X - 5, Target.Y - 5, 0, 0)
            };

            objects.Add(elipse);

            // draw start
            elipse = new Ellipse {
                Fill = Brushes.Green,
                Width = 10,
                Height = 10,
                Margin = new Thickness(_rrtTree[0].Location.X - 5, _rrtTree[0].Location.Y - 5, 0, 0)
            };

            objects.Add(elipse);

            return objects;
        }

        public int TotalBranches() {
            return _rrtTree.Count;
        }

        private static double Dot(Vector v1, Vector v2) {
            return (v1.X*v2.X + v1.Y*v2.Y);
        }
    }
}