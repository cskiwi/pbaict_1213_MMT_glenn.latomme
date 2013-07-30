using System;
using System.Diagnostics;
using Engine._2D;

namespace Soccer.Game {
    public abstract class MovingEntity : BaseGameEntity {
        protected double DMass;

        protected double DMaxForce;
        protected double DMaxSpeed;

        protected double DMaxTurnRate;
        protected Vector2D VHeading = new Vector2D();

        protected Vector2D VSide = new Vector2D();
        protected Vector2D VVelocity = new Vector2D();


        protected MovingEntity(Vector2D position, double radius, Vector2D velocity, double maxSpeed, Vector2D heading,
                               double mass, Vector2D scale, double turnRate, double maxForce)
            : base(GetNextValidId()) {
            VHeading = heading;
            VVelocity = velocity;
            DMass = mass;
            VSide = VHeading.Perp();
            DMaxSpeed = maxSpeed;
            DMaxTurnRate = turnRate;
            DMaxForce = maxForce;
            VPosition = position;
            MdBoundingRadius = radius;

            VScale = scale;
        }


        public Vector2D Velocity() {
            return VVelocity;
        }

        public void SetVelocity(Vector2D newVel) {
            VVelocity = newVel;
        }

        public double Mass() {
            return DMass;
        }

        public Vector2D Side() {
            return VSide;
        }

        public double MaxSpeed() {
            return DMaxSpeed;
        }

        public void SetMaxSpeed(double newSpeed) {
            DMaxSpeed = newSpeed;
        }

        public double MaxForce() {
            return DMaxForce;
        }

        public void SetMaxForce(double mf) {
            DMaxForce = mf;
        }

        public bool IsSpeedMaxedOut() {
            return DMaxSpeed*DMaxSpeed >= VVelocity.LengthSq();
        }

        public double Speed() {
            return VVelocity.Length();
        }

        public double SpeedSq() {
            return VVelocity.LengthSq();
        }

        public Vector2D Heading() {
            return VHeading;
        }

        public void SetHeading(Vector2D newHeading) {
            Debug.Assert((newHeading.LengthSq() - 1.0) < 0.00001);

            VHeading = newHeading;


            VSide = VHeading.Perp();
        }

        public bool RotateHeadingToFacePosition(Vector2D target) {
            Vector2D toTarget = Vector2D.Normalize(target - VPosition);

            double dot = VHeading.Dot(toTarget);

            dot.Clamp(-1, 1);

            double angle = Math.Acos(dot);

            if (angle < 0.00001) return true;

            if (angle > DMaxTurnRate) angle = DMaxTurnRate;

            var rotationMatrix = new C2DMatrix();

            rotationMatrix.Rotate(angle*VHeading.Sign(toTarget));
            rotationMatrix.TransformVector2Ds(VHeading);
            rotationMatrix.TransformVector2Ds(VVelocity);

            VSide = VHeading.Perp();

            return false;
        }

        public double MaxTurnRate() {
            return DMaxTurnRate;
        }

        public void SetMaxTurnRate(double val) {
            DMaxTurnRate = val;
        }
    }
}