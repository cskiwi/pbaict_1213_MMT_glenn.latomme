using System.Collections.Generic;
using Engine.Misc;
using Engine._2D;
using Soccer.Game;

namespace Soccer {
    public class SteeringBehaviors {
        private readonly double m_dMultSeparation;

        //how far it can 'see'
        private readonly double m_dViewDistance;
        private readonly SoccerBall m_pBall;
        private readonly PlayerBase m_pPlayer;

        //the steering force created by the combined effect of all
        //the selected behaviors
        private readonly Vector2D m_vSteeringForce = new Vector2D();

        //the current target (usually the ball or predicted ball position)
        private Vector2D m_vTarget = new Vector2D();
        private List<Vector2D> m_Antenna = new List<Vector2D>();
        private bool m_bTagged;

        //the distance the player tries to interpose from the target
        private double m_dInterposeDist;

        //multipliers.

        //binary flags to indicate whether or not a behavior should be active
        private int m_iFlags;

        public SteeringBehaviors(PlayerBase agent, SoccerPitch world, SoccerBall ball) {
            m_pPlayer = agent;
            m_iFlags = 0;
            m_dMultSeparation = Params.SeparationCoefficient;
            m_bTagged = false;
            m_dViewDistance = Params.ViewDistance;
            m_pBall = ball;
            m_dInterposeDist = 0.0;
            m_Antenna = new List<Vector2D>();
        }

        //this behavior moves the agent towards a target position

        //------------------------------- Seek -----------------------------------
        //
        //  Given a target, this behavior returns a steering force which will
        //  allign the agent with the target and move the agent in the desired
        //  direction
        //------------------------------------------------------------------------
        private Vector2D Seek(Vector2D target) {
            Vector2D desiredVelocity = Vector2D.Normalize(target - m_pPlayer.Pos())*m_pPlayer.MaxSpeed();

            return (desiredVelocity - m_pPlayer.Velocity());
        }

        //this behavior is similar to seek but it attempts to arrive
        //at the target with a zero velocity

        //--------------------------- Arrive -------------------------------------
        //
        //  This behavior is similar to seek but it attempts to arrive at the
        //  target with a zero velocity
        //------------------------------------------------------------------------
        private Vector2D Arrive(Vector2D target, Deceleration deceleration) {
            Vector2D toTarget = target - m_pPlayer.Pos();

            //calculate the distance to the target
            double dist = toTarget.Length();

            if (dist > 0) {
                //because Deceleration is enumerated as an int, this value is required
                //to provide fine tweaking of the deceleration..
                const double decelerationTweaker = 0.3;

                //calculate the speed required to reach the target given the desired
                //deceleration
                double speed = dist/((double) deceleration*decelerationTweaker);

                //make sure the velocity does not exceed the max
                speed = speed < m_pPlayer.MaxSpeed() ? speed : m_pPlayer.MaxSpeed();

                //from here proceed just like Seek except we don't need to normalize
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist.
                Vector2D desiredVelocity = toTarget*speed/dist;

                return (desiredVelocity - m_pPlayer.Velocity());
            }

            return new Vector2D(0, 0);
        }

        //This behavior predicts where its prey will be and seeks
        //to that location

        //------------------------------ Pursuit ---------------------------------
        //
        //  this behavior creates a force that steers the agent towards the
        //  ball
        //------------------------------------------------------------------------
        private Vector2D Pursuit(SoccerBall ball) {
            Vector2D ToBall = ball.Pos() - m_pPlayer.Pos();

            //the lookahead time is proportional to the distance between the ball
            //and the pursuer;
            double LookAheadTime = 0.0;

            if (ball.Speed() != 0.0) LookAheadTime = ToBall.Length()/ball.Speed();

            //calculate where the ball will be at this time in the future
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created if it does not yet exist:
//ORIGINAL LINE: m_vTarget = ball->FuturePosition(LookAheadTime);
            m_vTarget = ball.FuturePosition(LookAheadTime);

            //now seek to the predicted future position of the ball
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: return Arrive(m_vTarget, fast);
            return Arrive(m_vTarget, Deceleration.fast);
        }


        //---------------------------- Separation --------------------------------
        //
        // this calculates a force repelling from the other neighbors
        //------------------------------------------------------------------------
        private Vector2D Separation() {
            //iterate through all the neighbors and calculate the vector from the
            var steeringForce = new Vector2D();

            List<PlayerBase> allPlayers = AutoList<PlayerBase>.GetAllMembers();
            List<PlayerBase>.Enumerator curPlyr;
            for (curPlyr = allPlayers.GetEnumerator(); curPlyr.MoveNext();) {
                //make sure this agent isn't included in the calculations and that
                //the agent is close enough
                if ((curPlyr.Current != m_pPlayer) && (curPlyr.Current).Steering().Tagged()) {
                    Vector2D ToAgent = m_pPlayer.Pos() - (curPlyr.Current).Pos();

                    //scale the force inversely proportional to the agents distance
                    //from its neighbor.
                    steeringForce += Vector2D.Normalize(ToAgent)/ToAgent.Length();
                }
            }

            return steeringForce;
        }

        //this attempts to steer the agent to a position between the opponent
        //and the object

        //--------------------------- Interpose ----------------------------------
        //
        //  Given an opponent and an object position this method returns a
        //  force that attempts to position the agent between them
        //------------------------------------------------------------------------
        private Vector2D Interpose(SoccerBall ball, Vector2D target, double DistFromTarget) {
            return Arrive(target + Vector2D.Normalize(ball.Pos() - target)*DistFromTarget,
                          Deceleration.normal);
        }

        //finds any neighbours within the view radius

        //-------------------------- FindNeighbours ------------------------------
        //
        //  tags any vehicles within a predefined radius
        //------------------------------------------------------------------------
        private void FindNeighbours() {
            List<PlayerBase> allPlayers = AutoList<PlayerBase>.GetAllMembers();
            List<PlayerBase>.Enumerator curPlyr;
            for (curPlyr = allPlayers.GetEnumerator(); curPlyr.MoveNext();) {
                //first clear any current tag
                if (curPlyr.Current != null) {
                    (curPlyr.Current).Steering().UnTag();

                    //work in distance squared to avoid sqrts
                    Vector2D to = (curPlyr.Current).Pos() - m_pPlayer.Pos();

                    if (to.LengthSq() < (m_dViewDistance*m_dViewDistance)) (curPlyr.Current).Steering().Tag();
                }
            } //next
        }

        //this function tests if a specific bit of m_iFlags is set
        private bool On(behavior_type bt) {
            return (m_iFlags & (int) bt) == (decimal) bt;
        }


        //--------------------- AccumulateForce ----------------------------------
        //
        //  This function calculates how much of its max steering force the
        //  vehicle has left to apply and then applies that amount of the
        //  force to add.
        //------------------------------------------------------------------------
        private bool AccumulateForce(Vector2D sf, Vector2D ForceToAdd) {
            //first calculate how much steering force we have left to use
            double MagnitudeSoFar = sf.Length();

            double magnitudeRemaining = m_pPlayer.MaxForce() - MagnitudeSoFar;

            //return false if there is no more force left to use
            if (magnitudeRemaining <= 0.0) return false;

            //calculate the magnitude of the force we want to add
            double MagnitudeToAdd = ForceToAdd.Length();

            //now calculate how much of the force we can really add
            if (MagnitudeToAdd > magnitudeRemaining) MagnitudeToAdd = magnitudeRemaining;

            //add it to the steering force
            sf += (Vector2D.Normalize(ForceToAdd)*MagnitudeToAdd);

            return true;
        }


        //-------------------------- SumForces -----------------------------------
        //
        //  this method calls each active steering behavior and acumulates their
        //  forces until the max steering force magnitude is reached at which
        //  time the function returns the steering force accumulated to that
        //  point
        //------------------------------------------------------------------------
        private Vector2D SumForces() {
            var force = new Vector2D();

            //the soccer players must always tag their neighbors
            FindNeighbours();

            if (On(behavior_type.separation)) {
                force += Separation()*m_dMultSeparation;

//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: if (!AccumulateForce(m_vSteeringForce, force))
                if (!AccumulateForce(m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(behavior_type.seek)) {
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: force += Seek(m_vTarget);
                force += Seek(m_vTarget);

//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: if (!AccumulateForce(m_vSteeringForce, force))
                if (!AccumulateForce(m_vSteeringForce, new Vector2D(force))) return m_vSteeringForce;
            }

            if (On(behavior_type.arrive)) {
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: force += Arrive(m_vTarget, fast);
                force += Arrive(new Vector2D(m_vTarget), Deceleration.fast);

//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: if (!AccumulateForce(m_vSteeringForce, force))
                if (!AccumulateForce(m_vSteeringForce, new Vector2D(force))) return m_vSteeringForce;
            }

            if (On(behavior_type.pursuit)) {
                force += Pursuit(m_pBall);

//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: if (!AccumulateForce(m_vSteeringForce, force))
                if (!AccumulateForce(m_vSteeringForce, new Vector2D(force))) return m_vSteeringForce;
            }

            if (On(behavior_type.interpose)) {
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: force += Interpose(m_pBall, m_vTarget, m_dInterposeDist);
                force += Interpose(m_pBall, new Vector2D(m_vTarget), m_dInterposeDist);

//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: if (!AccumulateForce(m_vSteeringForce, force))
                if (!AccumulateForce(m_vSteeringForce, new Vector2D(force))) return m_vSteeringForce;
            }

            return m_vSteeringForce;
        }

        //a vertex buffer to contain the feelers rqd for dribbling

        public virtual void Dispose() {}


        //---------------------- Calculate ---------------------------------------
        //
        //  calculates the overall steering force based on the currently active
        //  steering behaviors.
        //------------------------------------------------------------------------
        public Vector2D Calculate() {
            //reset the force
            m_vSteeringForce.Zero();

            //this will hold the value of each individual steering force
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created if it does not yet exist:
//ORIGINAL LINE: m_vSteeringForce = SumForces();
            m_vSteeringForce.CopyFrom(SumForces());

            //make sure the force doesn't exceed the vehicles maximum allowable
            m_vSteeringForce.Truncate(m_pPlayer.MaxForce());

            return m_vSteeringForce;
        }

        //calculates the component of the steering force that is parallel
        //with the vehicle heading

        //------------------------- ForwardComponent -----------------------------
        //
        //  calculates the forward component of the steering force
        //------------------------------------------------------------------------
        public double ForwardComponent() {
            return m_pPlayer.Heading().Dot(m_vSteeringForce);
        }

        //calculates the component of the steering force that is perpendicuar
        //with the vehicle heading

        //--------------------------- SideComponent ------------------------------
        //
        //  //  calculates the side component of the steering force
        //------------------------------------------------------------------------
        public double SideComponent() {
            return m_pPlayer.Side().Dot(m_vSteeringForce)*m_pPlayer.MaxTurnRate();
        }

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: Vector2D Force()const
        public Vector2D Force() {
            return m_vSteeringForce;
        }

        //renders visual aids and info for seeing how each behavior is
        //calculated
//	void RenderInfo();

        //----------------------------- RenderAids -------------------------------
        //
        //------------------------------------------------------------------------
        public void RenderAids() {
            //render the steering force
            Cgdi.Instance().RedPen();

            Cgdi.Instance().Line(m_pPlayer.Pos(), m_pPlayer.Pos() + m_vSteeringForce*20);
        }

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: Vector2D Target()const
        public Vector2D Target() {
            return m_vTarget;
        }

        public void SetTarget(Vector2D t) {
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created if it does not yet exist:
//ORIGINAL LINE: m_vTarget = t;
            m_vTarget.CopyFrom(t);
        }

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: double InterposeDistance()const
        public double InterposeDistance() {
            return m_dInterposeDist;
        }

        public void SetInterposeDistance(double d) {
            m_dInterposeDist = d;
        }

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool Tagged()const
        public bool Tagged() {
            return m_bTagged;
        }

        public void Tag() {
            m_bTagged = true;
        }

        public void UnTag() {
            m_bTagged = false;
        }

        public void SeekOn() {
            m_iFlags |= behavior_type.seek;
        }

        public void ArriveOn() {
            m_iFlags |= behavior_type.arrive;
        }

        public void PursuitOn() {
            m_iFlags |= behavior_type.pursuit;
        }

        public void SeparationOn() {
            m_iFlags |= behavior_type.separation;
        }

        public void InterposeOn(double d) {
            m_iFlags |= behavior_type.interpose;
            m_dInterposeDist = d;
        }

        public void SeekOff() {
            if (On(behavior_type.seek)) m_iFlags ^= behavior_type.seek;
        }

        public void ArriveOff() {
            if (On(behavior_type.arrive)) m_iFlags ^= behavior_type.arrive;
        }

        public void PursuitOff() {
            if (On(behavior_type.pursuit)) m_iFlags ^= behavior_type.pursuit;
        }

        public void SeparationOff() {
            if (On(behavior_type.separation)) m_iFlags ^= behavior_type.separation;
        }

        public void InterposeOff() {
            if (On(behavior_type.interpose)) m_iFlags ^= behavior_type.interpose;
        }

        public bool SeekIsOn() {
            return On(behavior_type.seek);
        }

        public bool ArriveIsOn() {
            return On(behavior_type.arrive);
        }

        public bool PursuitIsOn() {
            return On(behavior_type.pursuit);
        }

        public bool SeparationIsOn() {
            return On(behavior_type.separation);
        }

        public bool InterposeIsOn() {
            return On(behavior_type.interpose);
        }

        private enum Deceleration {
            slow = 3,
            normal = 2,
            fast = 1
        }

        private enum behavior_type {
            none = 0x0000,
            seek = 0x0001,
            arrive = 0x0002,
            separation = 0x0004,
            pursuit = 0x0008,
            interpose = 0x0010
        }
    }
}

//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define gdi Cgdi::Instance()
#define gdi
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define Prm (*ParamLoader::Instance())
#define Prm