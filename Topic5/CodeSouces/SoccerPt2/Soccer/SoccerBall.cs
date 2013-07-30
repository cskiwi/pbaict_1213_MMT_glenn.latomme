using System;
using System.Collections.Generic;
using Engine._2D;
using Soccer.Game;
using Soccer.Messaging;

namespace Soccer {
    public class SoccerBall : MovingEntity {
        private readonly List<Wall2D> _pitchBoundary;
        private Vector2D _vOldPos = new Vector2D();

        public SoccerBall(Vector2D pos, double BallSize, double mass, List<Wall2D> pitchBoundary)
            : base(pos, BallSize, new Vector2D(0, 0), -1.0, new Vector2D(0, 1), mass, new Vector2D(1.0, 1.0), 0, 0)
            //max force - unused - turn rate - unused - scale     - unused - max speed - unused
        {
            _pitchBoundary = pitchBoundary;
        }

        public void TestCollisionWithWalls(List<Wall2D> walls) {
            int idxClosest = -1;

            Vector2D velNormal = Vector2D.Normalize(VVelocity);
            double distToIntersection = Utils.MaxFloat;

            for (int w = 0; w < walls.Count; ++w) {
                Vector2D thisCollisionPoint = Pos() - (walls[w].Normal()*BRadius());

                Vector2D intersectionPoint;
                if (Utils.WhereIsPoint(thisCollisionPoint, walls[w].From(), walls[w].Normal()) ==
                    Utils.SpanType.PlaneBackside) {
                    double distToWall = Utils.DistanceToRayPlaneIntersection(thisCollisionPoint, walls[w].Normal(),
                                                                             walls[w].From(), walls[w].Normal());

                    intersectionPoint = thisCollisionPoint + (distToWall*walls[w].Normal());
                }

                else {
                    double distToWall = Utils.DistanceToRayPlaneIntersection(thisCollisionPoint, velNormal,
                                                                             walls[w].From(), walls[w].Normal());

                    intersectionPoint = thisCollisionPoint + (distToWall*velNormal);
                }

                bool onLineSegment = Utils.LineIntersection2D(walls[w].From(), walls[w].To(),
                                                              thisCollisionPoint - walls[w].Normal()*20.0,
                                                              thisCollisionPoint + walls[w].Normal()*20.0);

                double distSq = thisCollisionPoint.DistanceSq(intersectionPoint);

                if ((distSq <= VVelocity.LengthSq()) && (distSq < distToIntersection) && onLineSegment) {
                    distToIntersection = distSq;
                    idxClosest = w;
                }
            }

            if ((idxClosest >= 0) && velNormal.Dot(walls[idxClosest].Normal()) < 0)
                VVelocity = VVelocity.Reflect(walls[idxClosest].Normal());
        }

        public new void Update() {
            _vOldPos = VPosition;
            TestCollisionWithWalls(_pitchBoundary);

            if (VVelocity.LengthSq() > Params.Friction*Params.Friction) {
                VVelocity += Vector2D.Normalize(VVelocity)*Params.Friction;

                VPosition += VVelocity;

                //update heading
                VHeading = Vector2D.Normalize(VVelocity))
                ;
            }
        }


        public override void Render() {
            Cgdi.Instance().BlackBrush();

            Cgdi.Instance().Circle(VPosition, BRadius);
        }

        public new bool HandleMessage(Telegram msg) {
            return false;
        }

        public void Kick(Vector2D direction, double force) {
            //ensure direction is normalized
            Vector2D.Normalize(direction);

            //calculate the acceleration
            Vector2D acceleration = (direction*force)/DMass;

            //update the velocity
            VVelocity = acceleration;
        }

        public double TimeToCoverDistance(Vector2D A, Vector2D B, double force) {
            double speed = force/DMass;
            double distanceToCover = A.Distance(B);
            double term = speed*speed + 2.0*distanceToCover*Params.Friction;

            if (term <= 0.0) return -1.0;

            double v = Math.Sqrt(term);

            return (v - speed)/Params.Friction;
        }

        //this method calculates where the ball will in 'time' seconds

        public Vector2D FuturePosition(double time) {
            Vector2D ut = VVelocity*time;
            double halfAtSquared = 0.5*Params.Friction*time*time;
            Vector2D scalarToVector = halfAtSquared*Vector2D.Normalize(VVelocity);
            return Pos() + ut + scalarToVector;
        }

        public void Trap() {
            VVelocity.Zero();
        }

        public Vector2D OldPos() {
            return _vOldPos;
        }
        public void PlaceAtPosition(Vector2D NewPos) {
            VVelocity = NewPos;

            _vOldPos = VVelocity;

            VVelocity.Zero();
        }
    }

    internal static class DefineConstants {
        public const int Newtransparent = 3;
        public const int Queryropsupport = 40;
        public const int Selectdib = 41;
        public const int ScScreensave = 0xF140;
    }
}