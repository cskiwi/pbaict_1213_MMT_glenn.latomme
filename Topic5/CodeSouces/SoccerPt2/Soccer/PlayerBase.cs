using System.Collections.Generic;
using System;
using Engine.Misc;
using Engine._2D;
using Soccer;
using Soccer.Game;

public abstract class PlayerBase : MovingEntity, AutoList<PlayerBase> {

	public enum player_role {
		goal_keeper,
		attacker,
		defender
	}


	//this player's role in the team
	protected player_role m_PlayerRole;

	//a pointer to this player's team
	protected SoccerTeam m_pTeam;

	//the steering behaviors
	protected SteeringBehaviors m_pSteering;

	//the region that this player is assigned to.
	protected int m_iHomeRegion;

	//the region this player moves to before kickoff
	protected int m_iDefaultRegion;

	//the distance to the ball (in squared-space). This value is queried
	//a lot so it's calculated once each time-step and stored here.
	protected double m_dDistSqToBall;

	//the vertex buffer
	protected List<Vector2D> m_vecPlayerVB = new List<Vector2D>();
	//the buffer for the transformed vertices
	protected List<Vector2D> m_vecPlayerVBTrans = new List<Vector2D>();



	//----------------------------- ctor -------------------------------------
	//------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: base(home_team->Pitch()->GetRegionFromIndex(home_region)->Center(), scale *10.0, velocity, max_speed, heading, mass, Vector2D(scale,scale), max_turn_rate, max_force);
	public PlayerBase(SoccerTeam home_team, int home_region, Vector2D heading, Vector2D velocity, double mass, double max_force, double max_speed, double max_turn_rate, double scale, player_role role) : base(home_team.Pitch().GetRegionFromIndex(home_region).Center(), scale * 10.0, new Vector2D(velocity), max_speed, new Vector2D(heading), mass, new Vector2D(scale, scale), max_turn_rate, max_force) {
		this.m_pTeam = home_team;
		this.m_dDistSqToBall = GlobalMembersMain.MaxFloat;
		this.m_iHomeRegion = home_region;
		this.m_iDefaultRegion = home_region;
		this.m_PlayerRole = new PlayerBase.player_role(role);
		//setup the vertex buffers and calculate the bounding radius
		const int NumPlayerVerts = 4;
		Vector2D[] player = {new Vector2D(-3, 8), new Vector2D(3, 10), new Vector2D(3, -10), new Vector2D(-3, -8)};

		for (int vtx = 0; vtx < NumPlayerVerts; ++vtx) {
			m_vecPlayerVB.Add(player[vtx]);

			//set the bounding radius to the length of the
			//greatest extent
			if (Math.Abs(player[vtx].x) > m_dBoundingRadius) {
				m_dBoundingRadius = Math.Abs(player[vtx].x);
			}

			if (Math.Abs(player[vtx].y) > m_dBoundingRadius) {
				m_dBoundingRadius = Math.Abs(player[vtx].y);
			}
		}

		//set up the steering behavior class
		m_pSteering = new SteeringBehaviors(this, m_pTeam.Pitch(), Ball());

		//a player's start target is its start position (because it's just waiting)
		m_pSteering.SetTarget(home_team.Pitch().GetRegionFromIndex(home_region).Center());
	}


	//----------------------------- dtor -------------------------------------
	//------------------------------------------------------------------------
	public override void Dispose() {
		if (m_pSteering != null)
			m_pSteering.Dispose();
		base.Dispose();
	}

	//returns true if there is an opponent within this player's
	//comfort zone

	//------------------------- IsThreatened ---------------------------------
	//
	//  returns true if there is an opponent within this player's
	//  comfort zone
	//------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool isThreatened()const
	public bool isThreatened() {
		//check against all opponents to make sure non are within this
		//player's comfort zone
		List<PlayerBase>.Enumerator curOpp;
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created if it does not yet exist:
//ORIGINAL LINE: curOpp = Team()->Opponents()->Members().begin();
		curOpp.CopyFrom(Team().Opponents().Members().GetEnumerator());

//C++ TO C# CONVERTER TODO TASK: Iterators are only converted within the context of 'while' and 'for' loops:
		for (curOpp; curOpp != Team().Opponents().Members().end();) {
			//calculate distance to the player. if dist is less than our
			//comfort zone, and the opponent is infront of the player, return true
			if (PositionInFrontOfPlayer((curOpp.Current).Pos()) && (GlobalMembersMain.Vec2DDistanceSq(Pos(), (curOpp.Current).Pos()) < *ParamLoader.Instance().PlayerComfortZoneSq)) {
				return true;
			}
		} // next opp

		return false;
	}

	//rotates the player to face the ball or the player's current target

	//----------------------------- TrackBall --------------------------------
	//
	//  sets the player's heading to point at the ball
	//------------------------------------------------------------------------
	public void TrackBall() {
		RotateHeadingToFacePosition(Ball().Pos());
	}

	//----------------------------- TrackTarget --------------------------------
	//
	//  sets the player's heading to point at the current target
	//------------------------------------------------------------------------
	public void TrackTarget() {
		SetHeading(GlobalMembersMain.Vec2DNormalize(Steering().Target() - Pos()));
	}

	//this messages the player that is closest to the supporting spot to
	//change state to support the attacking player

	//----------------------------- FindSupport -----------------------------------
	//
	//  determines the player who is closest to the SupportSpot and messages him
	//  to tell him to change state to SupportAttacker
	//-----------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: void FindSupport()const
	public void FindSupport() {
		//if there is no support we need to find a suitable player.
		if (Team().SupportingPlayer() == null) {
			PlayerBase BestSupportPly = Team().DetermineBestSupportingAttacker();

			Team().SetSupportingPlayer(BestSupportPly);

			MessageDispatcher.Instance().DispatchMsg(GlobalMembersPlayerBase.SEND_MSG_IMMEDIATELY, ID(), Team().SupportingPlayer().ID(), MessageType.Msg_SupportAttacker, null);
		}

		PlayerBase BestSupportPly = Team().DetermineBestSupportingAttacker();

		//if the best player available to support the attacker changes, update
		//the pointers and send messages to the relevant players to update their
		//states
		if (BestSupportPly != null && (BestSupportPly != Team().SupportingPlayer())) {
			if (Team().SupportingPlayer() != null) {
				MessageDispatcher.Instance().DispatchMsg(GlobalMembersPlayerBase.SEND_MSG_IMMEDIATELY, ID(), Team().SupportingPlayer().ID(), MessageType.Msg_GoHome, null);
			}

			Team().SetSupportingPlayer(BestSupportPly);

			MessageDispatcher.Instance().DispatchMsg(GlobalMembersPlayerBase.SEND_MSG_IMMEDIATELY, ID(), Team().SupportingPlayer().ID(), MessageType.Msg_SupportAttacker, null);
		}
	}

	//returns true if the ball can be grabbed by the goalkeeper
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool BallWithinKeeperRange()const
	public bool BallWithinKeeperRange() {
		return (GlobalMembersMain.Vec2DDistanceSq(Pos(), Ball().Pos()) < *ParamLoader.Instance().KeeperInBallRangeSq);
	}

	//returns true if the ball is within kicking range
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool BallWithinKickingRange()const
	public bool BallWithinKickingRange() {
		return (GlobalMembersMain.Vec2DDistanceSq(Ball().Pos(), Pos()) < *ParamLoader.Instance().PlayerKickingDistanceSq);
	}

	//returns true if a ball comes within range of a receiver
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool BallWithinReceivingRange()const
	public bool BallWithinReceivingRange() {
		return (GlobalMembersMain.Vec2DDistanceSq(Pos(), Ball().Pos()) < *ParamLoader.Instance().BallWithinReceivingRangeSq);
	}

	//returns true if the player is located within the boundaries
	//of his home region
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool InHomeRegion()const
	public bool InHomeRegion() {
		if (m_PlayerRole == player_role.goal_keeper) {
			return Pitch().GetRegionFromIndex(m_iHomeRegion).Inside(Pos(), Region.RegionModifier.Normal);
		}
		else {
			return Pitch().GetRegionFromIndex(m_iHomeRegion).Inside(Pos(), Region.RegionModifier.Halfsize);
		}
	}

	//returns true if this player is ahead of the attacker
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool isAheadOfAttacker()const
	public bool isAheadOfAttacker() {
		return Math.Abs(Pos().x - Team().OpponentsGoal().Center().x) < Math.Abs(Team().ControllingPlayer().Pos().x - Team().OpponentsGoal().Center().x);
	}

	//returns true if a player is located at the designated support spot
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool AtSupportSpot()const;
//	bool AtSupportSpot();

	//returns true if the player is located at his steering target
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool AtTarget()const
	public bool AtTarget() {
		return (GlobalMembersMain.Vec2DDistanceSq(Pos(), Steering().Target()) < *ParamLoader.Instance().PlayerInTargetRangeSq);
	}

	//returns true if the player is the closest player in his team to
	//the ball
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool isClosestTeamMemberToBall()const
	public bool isClosestTeamMemberToBall() {
		return Team().PlayerClosestToBall() == this;
	}

	//returns true if the point specified by 'position' is located in
	//front of the player

	//------------------------- WithinFieldOfView ---------------------------
	//
	//  returns true if subject is within field of view of this player
	//-----------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool PositionInFrontOfPlayer(Vector2D position)const
	public bool PositionInFrontOfPlayer(Vector2D position) {
		Vector2D ToSubject = position - Pos();

		if (ToSubject.Dot(Heading()) > 0)

		{
			return true;
		}

		else

		{
			return false;
		}
	}

	//returns true if the player is the closest player on the pitch to the ball
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool isClosestPlayerOnPitchToBall()const
	public bool isClosestPlayerOnPitchToBall() {
		return isClosestTeamMemberToBall() && (DistSqToBall() < Team().Opponents().ClosestDistToBallSq());
	}

	//returns true if this player is the controlling player
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool isControllingPlayer()const
	public bool isControllingPlayer() {
		return Team().ControllingPlayer() == this;
	}

	//returns true if the player is located in the designated 'hot region' --
	//the area close to the opponent's goal
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool InHotRegion()const
	public bool InHotRegion() {
		return Math.Abs(Pos().y - Team().OpponentsGoal().Center().y) < Pitch().PlayingArea().Length() / 3.0;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: player_role Role()const
	public player_role Role() {
		return m_PlayerRole;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: double DistSqToBall()const
	public double DistSqToBall() {
		return m_dDistSqToBall;
	}
	public void SetDistSqToBall(double val) {
		m_dDistSqToBall = val;
	}

	//calculate distance to opponent's/home goal. Used frequently by the passing
	//methods

	//calculate distance to opponent's goal. Used frequently by the passing//methods
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: double DistToOppGoal()const
	public double DistToOppGoal() {
		return Math.Abs(Pos().x - Team().OpponentsGoal().Center().x);
	}
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: double DistToHomeGoal()const
	public double DistToHomeGoal() {
		return Math.Abs(Pos().x - Team().HomeGoal().Center().x);
	}

	public void SetDefaultHomeRegion() {
		m_iHomeRegion = m_iDefaultRegion;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: SoccerBall* const Ball()const
	public SoccerBall Ball() {
		return Team().Pitch().Ball();
	}
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: SoccerPitch* const Pitch()const
	public SoccerPitch Pitch() {
		return Team().Pitch();
	}
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: SteeringBehaviors *const Steering()const
	public SteeringBehaviors Steering() {
		return m_pSteering;
	}
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: const Region* const HomeRegion()const
	public Region HomeRegion() {
		return Pitch().GetRegionFromIndex(m_iHomeRegion);
	}
	public void SetHomeRegion(int NewRegion) {
		m_iHomeRegion = NewRegion;
	}
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: SoccerTeam *const Team()const
	public SoccerTeam Team() {
		return m_pTeam;
	}
}