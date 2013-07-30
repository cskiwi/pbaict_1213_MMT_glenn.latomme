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

        private static double CalculateDistance(Point p1, Point p2) {
            return Math.Sqrt((p1.X - p2.X)*(p1.X - p2.X) +
                             (p1.Y - p2.Y)*(p1.Y - p2.Y));
        }

        private void GenerateTree() {
            _stopWatch.Start();
            bool targetReached = false;
            while (targetReached == false) {
                _newPoint = new Point(_random.Next(0, _width), _random.Next(0, _height));
                int closest = GetClosestID(_newPoint);
                RrtBranch branch = _rrtTree[closest].NextBranch(_newPoint, closest);
                if (!CheckCollisionWithTree(branch)) {
                    _rrtTree.Add(branch);
                    // check if close enough to target, if so add final branch
                    if (CheckIfCloseEnoughToTarget(branch)) {
                        _rrtTree.Add(branch.NextBranch(Target, _rrtTree.Count - 1));
                        _rrtTree[_rrtTree.Count - 1].Location = Target;
                        targetReached = true;
                    }
                }
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
        public void CalculateRrt(bool newRandomTarget, bool newRandomStart) {
            _rrtTree.Clear();
            FinalPath.Clear();
            if (newRandomTarget) Target = new Point(_random.Next(0, _width), _random.Next(0, _height));
            if (newRandomStart) Start = new Point(_random.Next(0, _width), _random.Next(0, _height));
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

            // overwrite finalPath
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
    }
}