using System.Collections.Generic;
using System;
using System.Diagnostics;
using Engine._2D;
using Soccer.Game;

public class SoccerTeam {

	public enum team_color {
		blue,
		red
	}


	//an instance of the state machine class
	private StateMachine<SoccerTeam> m_pStateMachine;

	//the team must know its own color!
	private team_color m_Color;

	//pointers to the team members
	private List<PlayerBase> m_Players = new List<PlayerBase>();

	//a pointer to the soccer pitch
	private SoccerPsitch m_pPitch;

	//pointers to the goals
	private Goal m_pOpponentsGoal;
	private Goal m_pHomeGoal;

	//a pointer to the opposing team
	private SoccerTeam m_pOpponents;

	//pointers to 'key' players
	private PlayerBase m_pControllingPlayer;
	private PlayerBase m_pSupportingPlayer;
	private PlayerBase m_pReceivingPlayer;
	private PlayerBase m_pPlayerClosestToBall;

	//the squared distance the closest player is from the ball
	private double m_dDistSqToBallOfClosestPlayer;

	//players use this to determine strategic positions on the playing field
	private SupportSpotCalculator m_pSupportSpotCalc;

	//creates all the players for this team

	//------------------------- CreatePlayers --------------------------------
	//
	//  creates the players
	//------------------------------------------------------------------------
	private void CreatePlayers() {
		if (Color() == team_color.blue) {
			//goalkeeper
			m_Players.Add(new GoalKeeper(this, 1, TendGoal.Instance(), new Vector2D(0, 1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale));

			//create the players
			m_Players.Add(new FieldPlayer(this, 6, Wait.Instance(), new Vector2D(0, 1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale, PlayerBase.player_role.attacker));

			m_Players.Add(new FieldPlayer(this, 8, Wait.Instance(), new Vector2D(0, 1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale, PlayerBase.player_role.attacker));

			m_Players.Add(new FieldPlayer(this, 3, Wait.Instance(), new Vector2D(0, 1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale, PlayerBase.player_role.defender));

			m_Players.Add(new FieldPlayer(this, 5, Wait.Instance(), new Vector2D(0, 1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale, PlayerBase.player_role.defender));
		}

		else {
			//goalkeeper
			m_Players.Add(new GoalKeeper(this, 16, TendGoal.Instance(), new Vector2D(0, -1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale));

			//create the players
			m_Players.Add(new FieldPlayer(this, 9, Wait.Instance(), new Vector2D(0, -1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale, PlayerBase.player_role.attacker));

			m_Players.Add(new FieldPlayer(this, 11, Wait.Instance(), new Vector2D(0, -1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale, PlayerBase.player_role.attacker));

			m_Players.Add(new FieldPlayer(this, 12, Wait.Instance(), new Vector2D(0, -1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale, PlayerBase.player_role.defender));

			m_Players.Add(new FieldPlayer(this, 14, Wait.Instance(), new Vector2D(0, -1), new Vector2D(0.0, 0.0), *ParamLoader.Instance().PlayerMass, *ParamLoader.Instance().PlayerMaxForce, *ParamLoader.Instance().PlayerMaxSpeedWithoutBall, *ParamLoader.Instance().PlayerMaxTurnRate, *ParamLoader.Instance().PlayerScale, PlayerBase.player_role.defender));
		}

		//register the players with the entity manager
		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			EntityManager.Instance().RegisterEntity(it.Current);
		}
	}

	//called each frame. Sets m_pClosestPlayerToBall to point to the player
	//closest to the ball.

	//------------------------ CalculateClosestPlayerToBall ------------------
	//
	//  sets m_iClosestPlayerToBall to the player closest to the ball
	//------------------------------------------------------------------------
	private void CalculateClosestPlayerToBall() {
		double ClosestSoFar = GlobalMembersSoccerBall.MaxFloat;

		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			//calculate the dist. Use the squared value to avoid sqrt
			double dist = GlobalMembersSoccerBall.Vec2DDistanceSq((it.Current).Pos(), Pitch().Ball().Pos());

			//keep a record of this value for each player
			(it.Current).SetDistSqToBall(dist);

			if (dist < ClosestSoFar) {
				ClosestSoFar = dist;

				m_pPlayerClosestToBall = it.Current;
			}
		}

		m_dDistSqToBallOfClosestPlayer = ClosestSoFar;
	}



	//----------------------------- ctor -------------------------------------
	//
	//------------------------------------------------------------------------
	public SoccerTeam(Goal home_goal, Goal opponents_goal, SoccerPitch pitch, team_color color) {
		this.m_pOpponentsGoal = opponents_goal;
		this.m_pHomeGoal = home_goal;
		this.m_pOpponents = null;
		this.m_pPitch = pitch;
		this.m_Color = new SoccerTeam.team_color(color);
		this.m_dDistSqToBallOfClosestPlayer = 0.0;
		this.m_pSupportingPlayer = null;
		this.m_pReceivingPlayer = null;
		this.m_pControllingPlayer = null;
		this.m_pPlayerClosestToBall = null;
		//setup the state machine
		m_pStateMachine = new StateMachine<SoccerTeam>(this);

		m_pStateMachine.SetCurrentState(Defending.Instance());
		m_pStateMachine.SetPreviousState(Defending.Instance());
		m_pStateMachine.SetGlobalState(null);

		//create the players and goalkeeper
		CreatePlayers();

		//set default steering behaviors
		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			(it.Current).Steering().SeparationOn();
		}

		//create the sweet spot calculator
		m_pSupportSpotCalc = new SupportSpotCalculator(*ParamLoader.Instance().NumSupportSpotsX, *ParamLoader.Instance().NumSupportSpotsY, this);
	}


	//----------------------- dtor -------------------------------------------
	//
	//------------------------------------------------------------------------
	public void Dispose() {
		if (m_pStateMachine != null)
			m_pStateMachine.Dispose();

		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();
		for (it; it.MoveNext();) {
			it.Current = null;
		}

		if (m_pSupportSpotCalc != null)
			m_pSupportSpotCalc.Dispose();
	}

	//the usual suspects

	//--------------------------- Render -------------------------------------
	//
	//  renders the players and any team related info
	//------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: void Render()const
	public void Render() {
		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			(it.Current).Render();
		}

		//show the controlling team and player at the top of the display
		if (*ParamLoader.Instance().bShowControllingTeam) {
			Cgdi.Instance().TextColor(Cgdi.AnonymousEnum3.white);

			if ((Color() == team_color.blue) && InControl()) {
				Cgdi.Instance().TextAtPos(20,3,"Blue in Control");
			}
			else if ((Color() == team_color.red) && InControl()) {
				Cgdi.Instance().TextAtPos(20,3,"Red in Control");
			}
			if (m_pControllingPlayer != null) {
				Cgdi.Instance().TextAtPos(Pitch().cxClient() - 150, 3, "Controlling Player: " + GlobalMembersSoccerPitch.ttos(m_pControllingPlayer.ID()));
			}
		}

		//render the sweet spots
		if (*ParamLoader.Instance().bSupportSpots && InControl()) {
			m_pSupportSpotCalc.Render();
		}

		//#define SHOW_TEAM_STATE
#if SHOW_TEAM_STATE
		if (Color() == team_color.red) {
			Cgdi.Instance().TextColor(Cgdi.AnonymousEnum3.white);

			if (CurrentState() == Attacking.Instance()) {
				Cgdi.Instance().TextAtPos(160, 20, "Attacking");
			}
			if (CurrentState() == Defending.Instance()) {
				Cgdi.Instance().TextAtPos(160, 20, "Defending");
			}
			if (CurrentState() == PrepareForKickOff.Instance()) {
				Cgdi.Instance().TextAtPos(160, 20, "Kickoff");
			}
		}
		else {
			if (CurrentState() == Attacking.Instance()) {
				Cgdi.Instance().TextAtPos(160, Pitch().cyClient() - 40, "Attacking");
			}
			if (CurrentState() == Defending.Instance()) {
				Cgdi.Instance().TextAtPos(160, Pitch().cyClient() - 40, "Defending");
			}
			if (CurrentState() == PrepareForKickOff.Instance()) {
				Cgdi.Instance().TextAtPos(160, Pitch().cyClient() - 40, "Kickoff");
			}
		}
#endif

		//#define SHOW_SUPPORTING_PLAYERS_TARGET
#if SHOW_SUPPORTING_PLAYERS_TARGET
		if (m_pSupportingPlayer != null) {
			Cgdi.Instance().BlueBrush();
			Cgdi.Instance().RedPen();
			Cgdi.Instance().Circle(m_pSupportingPlayer.Steering().Target(), 4);
		}
#endif
	}

	//-------------------------- update --------------------------------------
	//
	//  iterates through each player's update function and calculates
	//  frequently accessed info
	//------------------------------------------------------------------------
	public void Update() {
		//this information is used frequently so it's more efficient to
		//calculate it just once each frame
		CalculateClosestPlayerToBall();

		//the team state machine switches between attack/defense behavior. It
		//also handles the 'kick off' state where a team must return to their
		//kick off positions before the whistle is blown
		m_pStateMachine.Update();

		//now update each player
		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			(it.Current).Update();
		}
	}

	//calling this changes the state of all field players to that of
	//ReturnToHomeRegion. Mainly used when a goal keeper has
	//possession

	//--------------------- ReturnAllFieldPlayersToHome ---------------------------
	//
	//  sends a message to all players to return to their home areas forthwith
	//------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: void ReturnAllFieldPlayersToHome()const
	public void ReturnAllFieldPlayersToHome() {
		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			if ((it.Current).Role() != PlayerBase.player_role.goal_keeper) {
				MessageDispatcher.Instance().DispatchMsg(GlobalMembersSoccerTeam.SEND_MSG_IMMEDIATELY, 1, (it.Current).ID(), MessageType.Msg_GoHome, null);
			}
		}
	}

	//returns true if player has a clean shot at the goal and sets ShotTarget
	//to a normalized vector pointing in the direction the shot should be
	//made. Else returns false and sets heading to a zero vector

	//------------------------ CanShoot --------------------------------------
	//
	//  Given a ball position, a kicking power and a reference to a vector2D
	//  this function will sample random positions along the opponent's goal-
	//  mouth and check to see if a goal can be scored if the ball was to be
	//  kicked in that direction with the given power. If a possible shot is
	//  found, the function will immediately return true, with the target
	//  position stored in the vector ShotTarget.
	//------------------------------------------------------------------------
	public bool CanShoot(Vector2D BallPos, double power) {
		return CanShoot(BallPos, power, new Vector2D());
	}
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool CanShoot(Vector2D BallPos, double power, Vector2D& ShotTarget = Vector2D())const
//C++ TO C# CONVERTER NOTE: Overloaded method(s) are created above to convert the following method having default parameters:
	public bool CanShoot(Vector2D BallPos, double power, ref Vector2D ShotTarget) {
		//the number of randomly created shot targets this method will test
		int NumAttempts = *ParamLoader.Instance().NumAttemptsToFindValidStrike;

		while (NumAttempts--) {
			//choose a random position along the opponent's goal mouth. (making
			//sure the ball's radius is taken into account)
			ShotTarget = OpponentsGoal().Center();

			//the y value of the shot position should lay somewhere between two
			//goalposts (taking into consideration the ball diameter)
			int MinYVal = (int)(OpponentsGoal().LeftPost().y + Pitch().Ball().BRadius());
			int MaxYVal = (int)(OpponentsGoal().RightPost().y - Pitch().Ball().BRadius());

			ShotTarget.y = (double)GlobalMembersSoccerBall.RandInt(MinYVal, MaxYVal);

			//make sure striking the ball with the given power is enough to drive
			//the ball over the goal line.
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: double time = Pitch()->Ball()->TimeToCoverDistance(BallPos, ShotTarget, power);
			double time = Pitch().Ball().TimeToCoverDistance(new Vector2D(BallPos), new Vector2D(ShotTarget), power);

			//if it is, this shot is then tested to see if any of the opponents
			//can intercept it.
			if (time >= 0) {
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: if (isPassSafeFromAllOpponents(BallPos, ShotTarget, null, power))
				if (isPassSafeFromAllOpponents(new Vector2D(BallPos), new Vector2D(ShotTarget), null, power)) {
					return true;
				}
			}
		}

		return false;
	}

	//The best pass is considered to be the pass that cannot be intercepted
	//by an opponent and that is as far forward of the receiver as possible
	//If a pass is found, the receiver's address is returned in the
	//reference, 'receiver' and the position the pass will be made to is
	//returned in the  reference 'PassTarget'

	//-------------------------- FindPass ------------------------------
	//
	//  The best pass is considered to be the pass that cannot be intercepted
	//  by an opponent and that is as far forward of the receiver as possible
	//------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool FindPass(const PlayerBase *const passer, PlayerBase*& receiver, Vector2D& PassTarget, double power, double MinPassingDistance)const
	public bool FindPass(PlayerBase passer, ref PlayerBase receiver, ref Vector2D PassTarget, double power, double MinPassingDistance) {
		List<PlayerBase>.Enumerator curPlyr = Members().GetEnumerator();

		double ClosestToGoalSoFar = GlobalMembersSoccerBall.MaxFloat;
		Vector2D Target = new Vector2D();

		//iterate through all this player's team members and calculate which
		//one is in a position to be passed the ball
		for (curPlyr; curPlyr.MoveNext();) {
			//make sure the potential receiver being examined is not this player
			//and that it is further away than the minimum pass distance
			if ((curPlyr.Current != passer) && (GlobalMembersSoccerBall.Vec2DDistanceSq(passer.Pos(), (curPlyr.Current).Pos()) > MinPassingDistance * MinPassingDistance)) {
				if (GetBestPassToReceiver(passer, curPlyr.Current, Target, power)) {
					//if the pass target is the closest to the opponent's goal line found
					// so far, keep a record of it
					double Dist2Goal = Math.Abs(Target.x - OpponentsGoal().Center().x);

					if (Dist2Goal < ClosestToGoalSoFar) {
						ClosestToGoalSoFar = Dist2Goal;

						//keep a record of this player
						receiver = curPlyr.Current;

						//and the target
						PassTarget = Target;
					}
				}
			}
		} //next team member

		if (receiver != null) {
			return true;
		}

		else {
			return false;
		}
	}

	//Three potential passes are calculated. One directly toward the receiver's
	//current position and two that are the tangents from the ball position
	//to the circle of radius 'range' from the receiver.
	//These passes are then tested to see if they can be intercepted by an
	//opponent and to make sure they terminate within the playing area. If
	//all the passes are invalidated the function returns false. Otherwise
	//the function returns the pass that takes the ball closest to the
	//opponent's goal area.
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool GetBestPassToReceiver(const PlayerBase* const passer, const PlayerBase* const receiver, Vector2D& PassTarget, const double power)const;
//	bool GetBestPassToReceiver(PlayerBase passer, PlayerBase receiver, Vector2D PassTarget, double power);

	//test if a pass from positions 'from' to 'target' kicked with force
	//'PassingForce'can be intercepted by an opposing player

	//----------------------- isPassSafeFromOpponent -------------------------
	//
	//  test if a pass from 'from' to 'to' can be intercepted by an opposing
	//  player
	//------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool isPassSafeFromOpponent(Vector2D from, Vector2D target, const PlayerBase* const receiver, const PlayerBase* const opp, double PassingForce)const
	public bool isPassSafeFromOpponent(Vector2D from, Vector2D target, PlayerBase receiver, PlayerBase opp, double PassingForce) {
		//move the opponent into local space.
		Vector2D ToTarget = target - from;
		Vector2D ToTargetNormalized = GlobalMembersSoccerBall.Vec2DNormalize(ToTarget);

		Vector2D LocalPosOpp = GlobalMembersSoccerBall.PointToLocalSpace(opp.Pos(), ToTargetNormalized, ToTargetNormalized.Perp(), from);

		//if opponent is behind the kicker then pass is considered okay(this is
		//based on the assumption that the ball is going to be kicked with a
		//velocity greater than the opponent's max velocity)
		if (LocalPosOpp.x < 0) {
			return true;
		}

		//if the opponent is further away than the target we need to consider if
		//the opponent can reach the position before the receiver.
		if (GlobalMembersSoccerBall.Vec2DDistanceSq(from, target) < GlobalMembersSoccerBall.Vec2DDistanceSq(opp.Pos(), from)) {
			if (receiver != null) {
				if (GlobalMembersSoccerBall.Vec2DDistanceSq(target, opp.Pos()) > GlobalMembersSoccerBall.Vec2DDistanceSq(target, receiver.Pos())) {
					return true;
				}

				else {
					return false;
				}
			}

			else {
				return true;
			}
		}

		//calculate how long it takes the ball to cover the distance to the
		//position orthogonal to the opponents position
		double TimeForBall = Pitch().Ball().TimeToCoverDistance(new Vector2D(0, 0), new Vector2D(LocalPosOpp.x, 0), PassingForce);

		//now calculate how far the opponent can run in this time
		double reach = opp.MaxSpeed() * TimeForBall + Pitch().Ball().BRadius() + opp.BRadius();

		//if the distance to the opponent's y position is less than his running
		//range plus the radius of the ball and the opponents radius then the
		//ball can be intercepted
		if (Math.Abs(LocalPosOpp.y) < reach) {
			return false;
		}

		return true;
	}

	//tests a pass from position 'from' to position 'target' against each member
	//of the opposing team. Returns true if the pass can be made without
	//getting intercepted

	//---------------------- isPassSafeFromAllOpponents ----------------------
	//
	//  tests a pass from position 'from' to position 'target' against each member
	//  of the opposing team. Returns true if the pass can be made without
	//  getting intercepted
	//------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool isPassSafeFromAllOpponents(Vector2D from, Vector2D target, const PlayerBase* const receiver, double PassingForce)const
	public bool isPassSafeFromAllOpponents(Vector2D from, Vector2D target, PlayerBase receiver, double PassingForce) {
		List<PlayerBase>.Enumerator opp = Opponents().Members().GetEnumerator();

//C++ TO C# CONVERTER TODO TASK: Iterators are only converted within the context of 'while' and 'for' loops:
		for (opp; opp != Opponents().Members().end();) {
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy constructor call - this should be verified and a copy constructor should be created if it does not yet exist:
//ORIGINAL LINE: if (!isPassSafeFromOpponent(from, target, receiver, opp.Current, PassingForce))
			if (!isPassSafeFromOpponent(new Vector2D(from), new Vector2D(target), receiver, opp.Current, PassingForce)) {
				DebugConsole.On(); return false;
			}
		}

		return true;
	}

	//returns true if there is an opponent within radius of position

	//----------------------------- isOpponentWithinRadius ------------------------
	//
	//  returns true if an opposing player is within the radius of the position
	//  given as a parameter
	//-----------------------------------------------------------------------------
	public bool isOpponentWithinRadius(Vector2D pos, double rad) {
		List<PlayerBase>.Enumerator end = Opponents().Members().end();
		List<PlayerBase>.Enumerator it;

		for (it = Opponents().Members().GetEnumerator(); it != end; ++it) {
//C++ TO C# CONVERTER TODO TASK: Iterators are only converted within the context of 'while' and 'for' loops:
			if (GlobalMembersSoccerBall.Vec2DDistanceSq(pos, (it).Pos()) < rad * rad) {
				return true;
			}
		}

		return false;
	}

	//this tests to see if a pass is possible between the requester and
	//the controlling player. If it is possible a message is sent to the
	//controlling player to pass the ball asap.

	//------------------------- RequestPass ---------------------------------------
	//
	//  this tests to see if a pass is possible between the requester and
	//  the controlling player. If it is possible a message is sent to the
	//  controlling player to pass the ball asap.
	//-----------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: void RequestPass(FieldPlayer* requester)const
	public void RequestPass(FieldPlayer requester) {
		//maybe put a restriction here
		if (GlobalMembersSoccerBall.RandFloat() > 0.1)
			return;

		if (isPassSafeFromAllOpponents(ControllingPlayer().Pos(), requester.Pos(), requester, *ParamLoader.Instance().MaxPassingForce)) {
			//tell the player to make the pass
			//let the receiver know a pass is coming
			MessageDispatcher.Instance().DispatchMsg(GlobalMembersSoccerTeam.SEND_MSG_IMMEDIATELY, requester.ID(), ControllingPlayer().ID(), MessageType.Msg_PassToMe, requester);
		}
	}

	//calculates the best supporting position and finds the most appropriate
	//attacker to travel to the spot

	//------------- DetermineBestSupportingAttacker ------------------------
	//
	// calculate the closest player to the SupportSpot
	//------------------------------------------------------------------------
	public PlayerBase DetermineBestSupportingAttacker() {
		double ClosestSoFar = GlobalMembersSoccerBall.MaxFloat;

		PlayerBase BestPlayer = null;

		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			//only attackers utilize the BestSupportingSpot
			if (((it.Current).Role() == PlayerBase.player_role.attacker) && ((it.Current) != m_pControllingPlayer)) {
				//calculate the dist. Use the squared value to avoid sqrt
				double dist = GlobalMembersSoccerBall.Vec2DDistanceSq((it.Current).Pos(), m_pSupportSpotCalc.GetBestSupportingSpot());

				//if the distance is the closest so far and the player is not a
				//goalkeeper and the player is not the one currently controlling
				//the ball, keep a record of this player
				if ((dist < ClosestSoFar)) {
					ClosestSoFar = dist;

					BestPlayer = (it.Current);
				}
			}
		}

		return BestPlayer;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: const ClassicVector<PlayerBase*>& Members()const
	public List<PlayerBase> Members() {
		return m_Players;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: StateMachine<SoccerTeam>* GetFSM()const
	public StateMachine<SoccerTeam> GetFSM() {
		return m_pStateMachine;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: Goal *const HomeGoal()const
	public Goal HomeGoal() {
		return m_pHomeGoal;
	}
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: Goal *const OpponentsGoal()const
	public Goal OpponentsGoal() {
		return m_pOpponentsGoal;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: SoccerPitch *const Pitch()const
	public SoccerPitch Pitch() {
		return m_pPitch;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: SoccerTeam *const Opponents()const
	public SoccerTeam Opponents() {
		return m_pOpponents;
	}
	public void SetOpponents(SoccerTeam opps) {
		m_pOpponents = opps;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: team_color Color()const
	public team_color Color() {
		return m_Color;
	}

	public void SetPlayerClosestToBall(PlayerBase plyr) {
		m_pPlayerClosestToBall = plyr;
	}
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: PlayerBase* PlayerClosestToBall()const
	public PlayerBase PlayerClosestToBall() {
		return m_pPlayerClosestToBall;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: double ClosestDistToBallSq()const
	public double ClosestDistToBallSq() {
		return m_dDistSqToBallOfClosestPlayer;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: Vector2D GetSupportSpot()const
	public Vector2D GetSupportSpot() {
		return m_pSupportSpotCalc.GetBestSupportingSpot();
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: PlayerBase* SupportingPlayer()const
	public PlayerBase SupportingPlayer() {
		return m_pSupportingPlayer;
	}
	public void SetSupportingPlayer(PlayerBase plyr) {
		m_pSupportingPlayer = plyr;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: PlayerBase* Receiver()const
	public PlayerBase Receiver() {
		return m_pReceivingPlayer;
	}
	public void SetReceiver(PlayerBase plyr) {
		m_pReceivingPlayer = plyr;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: PlayerBase* ControllingPlayer()const
	public PlayerBase ControllingPlayer() {
		return m_pControllingPlayer;
	}
	public void SetControllingPlayer(PlayerBase plyr) {
		m_pControllingPlayer = plyr;

		//rub it in the opponents faces!
		Opponents().LostControl();
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool InControl()const
	public bool InControl() {
		if (m_pControllingPlayer != null) {
			return true;
		}
		else {
			return false;
		}
	}
	public void LostControl() {
		m_pControllingPlayer = null;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: PlayerBase* GetPlayerFromID(int id)const
	public PlayerBase GetPlayerFromID(int id) {
		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			if ((it.Current).ID() == id) {
				return it.Current;
			}
		}

		return null;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: void SetPlayerHomeRegion(int plyr, int region)const
	public void SetPlayerHomeRegion(int plyr, int region) {
		Debug.Assert((plyr >= 0) && (plyr < (int)m_Players.Count));

		m_Players[plyr].SetHomeRegion(region);
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: void DetermineBestSupportingPosition()const
	public void DetermineBestSupportingPosition() {
		m_pSupportSpotCalc.DetermineBestSupportingPosition();
	}


	//---------------------- UpdateTargetsOfWaitingPlayers ------------------------
	//
	//
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: void UpdateTargetsOfWaitingPlayers()const
	public void UpdateTargetsOfWaitingPlayers() {
		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			if ((it.Current).Role() != PlayerBase.player_role.goal_keeper) {
				//cast to a field player
				FieldPlayer plyr = (FieldPlayer)(it.Current);

				if (plyr.GetFSM().isInState(Wait.Instance()) || plyr.GetFSM().isInState(ReturnToHomeRegion.Instance())) {
					plyr.Steering().SetTarget(plyr.HomeRegion().Center());
				}
			}
		}
	}

	//returns false if any of the team are not located within their home region

	//--------------------------- AllPlayersAtHome --------------------------------
	//
	//  returns false if any of the team are not located within their home region
	//-----------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool AllPlayersAtHome()const
	public bool AllPlayersAtHome() {
		List<PlayerBase>.Enumerator it = m_Players.GetEnumerator();

		for (it; it.MoveNext();) {
			if ((it.Current).InHomeRegion() == false) {
				return false;
			}
		}

		return true;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: string Name()const
	public string Name() {
		if (m_Color == team_color.blue) {
			return "Blue";
		}
			return "Red";
	}
}

//------------------------------------------------------------------------
//
//  Name:   GoalKeeper.h
//
//  Desc:   class to implement a goalkeeper agent
//
//  Author: Mat Buckland 2003 (fup@ai-junkie.com)
//
//------------------------------------------------------------------------

//C++ TO C# CONVERTER NOTE: C# has no need of forward class declarations:
//class PlayerBase;

public class GoalKeeper : PlayerBase {

	//an instance of the state machine class
	private StateMachine<GoalKeeper> m_pStateMachine;

	//this vector is updated to point towards the ball and is used when
	//rendering the goalkeeper (instead of the underlaying vehicle's heading)
	//to ensure he always appears to be watching the ball
	private Vector2D m_vLookAt = new Vector2D();


//	GoalKeeper(SoccerTeam home_team, int home_region, State<GoalKeeper> start_state, Vector2D heading, Vector2D velocity, double mass, double max_force, double max_speed, double max_turn_rate, double scale);

	public new void Dispose() {
		if (m_pStateMachine != null)
			m_pStateMachine.Dispose();
		base.Dispose();
	}

	//these must be implemented
//	void Update();
//	void Render();
//	bool HandleMessage(Telegram msg);

	//returns true if the ball comes close enough for the keeper to
	//consider intercepting
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool BallWithinRangeForIntercept()const;
//	bool BallWithinRangeForIntercept();

	//returns true if the keeper has ventured too far away from the goalmouth
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool TooFarFromGoalMouth()const;
//	bool TooFarFromGoalMouth();

	//this method is called by the Intercept state to determine the spot
	//along the goalmouth which will act as one of the interpose targets
	//(the other is the ball).
	//the specific point at the goal line that the keeper is trying to cover
	//is flexible and can move depending on where the ball is on the field.
	//To achieve this we just scale the ball's y value by the ratio of the
	//goal width to playingfield width
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: Vector2D GetRearInterposeTarget()const;
//	Vector2D GetRearInterposeTarget();

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: StateMachine<GoalKeeper>* GetFSM()const
	public StateMachine<GoalKeeper> GetFSM() {
		return m_pStateMachine;
	}

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: Vector2D LookAt()const
	public Vector2D LookAt() {
		return m_vLookAt;
	}
	public void SetLookAt(Vector2D v) {
//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created if it does not yet exist:
//ORIGINAL LINE: m_vLookAt=v;
		m_vLookAt.CopyFrom(v);
	}
}

//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define Prm (*ParamLoader::Instance())
#define Prm
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EntityMgr EntityManager::Instance()
#define EntityMgr
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define KEYDOWN(vk_code) ((GetAsyncKeyState(vk_code) & 0x8000) ? 1 : 0)
#define KEYDOWN
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define WAS_KEY_PRESSED(vk_code) ((GetKeyState(vk_code) & 0x8000) != 0)
#define WAS_KEY_PRESSED
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define IS_KEY_PRESSED(vk_code) ((GetAsyncKeyState(vk_code) & 0x8000) != 0)
#define IS_KEY_PRESSED
#if DEBUG
#define debug_con_ConditionalDefinition1
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define debug_con *(DebugConsole::Instance())
#define debug_con
#else
#define debug_con_ConditionalDefinition2
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define debug_con *(CSink::Instance())
#define debug_con
#endif
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define debug_on DebugConsole::On();
#define debug_on
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define debug_off DebugConsole::Off();
#define debug_off


//---------------------- GetBestPassToReceiver ---------------------------
//
//  Three potential passes are calculated. One directly toward the receiver's
//  current position and two that are the tangents from the ball position
//  to the circle of radius 'range' from the receiver.
//  These passes are then tested to see if they can be intercepted by an
//  opponent and to make sure they terminate within the playing area. If
//  all the passes are invalidated the function returns false. Otherwise
//  the function returns the pass that takes the ball closest to the
//  opponent's goal area.
//------------------------------------------------------------------------
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool SoccerTeam::GetBestPassToReceiver(const PlayerBase* const passer, const PlayerBase* const receiver, Vector2D& PassTarget, double power)const