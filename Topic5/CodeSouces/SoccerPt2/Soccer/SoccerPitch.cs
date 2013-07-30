using System.Collections.Generic;
using System.Diagnostics;
using Engine._2D;
using Soccer;
using Soccer.Game;

public class SoccerPitch {
    public const int NumRegionsHorizontal = 6;
    public const int NumRegionsVertical = 3;
    public SoccerBall m_pBall;

	public SoccerTeam m_pRedTeam;
	public SoccerTeam m_pBlueTeam;

	public Goal m_pRedGoal;
	public Goal m_pBlueGoal;

	//container for the boundary walls
	public List<Wall2D> m_vecWalls = new List<Wall2D>();

	//defines the dimensions of the playing area
	public Region m_pPlayingArea;

	//the playing field is broken up into regions that the team
	//can make use of to implement strategies.
	public List<Region> m_Regions = new List<Region>();

	//true if a goal keeper has possession
	public bool m_bGoalKeeperHasBall;

	//true if the game is in play. Set to false whenever the players
	//are getting ready for kickoff
	public bool m_bGameOn;

	//set true to pause the motion
	public bool m_bPaused;

	//local copy of client window dimensions
	public int m_cxClient;
	public int m_cyClient;

	//this instantiates the regions the players utilize to  position
	//themselves

	//------------------------- CreateRegions --------------------------------
	public void CreateRegions(double width, double height) {
		//index into the vector
		int idx = m_Regions.Count - 1;

		for (int col = 0; col < NumRegionsHorizontal; ++col) {
			for (int row = 0; row < NumRegionsVertical; ++row) {
				m_Regions[idx--] = new Region(PlayingArea().Left() + col * width, PlayingArea().Top() + row * height, PlayingArea().Left() + (col + 1) * width, PlayingArea().Top() + (row + 1) * height, idx);
			}
		}
	}



	//------------------------------- ctor -----------------------------------
	//------------------------------------------------------------------------
	public SoccerPitch(int cx, int cy) {
		this.m_cxClient = cx;
		this.m_cyClient = cy;
		this.m_bPaused = false;
		this.m_bGoalKeeperHasBall = false;
		this.m_Regions = new List<Region>();
		this.m_bGameOn = true;
		//define the playing area
		m_pPlayingArea = new Region(20, 20, cx - 20, cy - 20);

		//create the regions
		CreateRegions(PlayingArea().Width() / (double)NumRegionsHorizontal, PlayingArea().Height() / (double)NumRegionsVertical);

		//create the goals
		m_pRedGoal = new Goal(new Vector2D(m_pPlayingArea.Left(), (cy - Params.GoalWidth) / 2), new Vector2D(m_pPlayingArea.Left(), cy - (cy - Params.GoalWidth) / 2), new Vector2D(1, 0));

		m_pBlueGoal = new Goal(new Vector2D(m_pPlayingArea.Right(), (cy - Params.GoalWidth) / 2), new Vector2D(m_pPlayingArea.Right(), cy - (cy - Params.GoalWidth) / 2), new Vector2D(-1, 0));

		//create the soccer ball
		m_pBall = new SoccerBall(new Vector2D((double)m_cxClient / 2.0, (double)m_cyClient / 2.0), Params.BallSize, Params.BallMass, m_vecWalls);

		//create the teams
		m_pRedTeam = new SoccerTeam(m_pRedGoal, m_pBlueGoal, this, SoccerTeam.team_color.red);
		m_pBlueTeam = new SoccerTeam(m_pBlueGoal, m_pRedGoal, this, SoccerTeam.team_color.blue);

		//make sure each team knows who their opponents are
		m_pRedTeam.SetOpponents(m_pBlueTeam);
		m_pBlueTeam.SetOpponents(m_pRedTeam);

		//create the walls
		Vector2D TopLeft = new Vector2D(m_pPlayingArea.Left(), m_pPlayingArea.Top());
		Vector2D TopRight = new Vector2D(m_pPlayingArea.Right(), m_pPlayingArea.Top());
		Vector2D BottomRight = new Vector2D(m_pPlayingArea.Right(), m_pPlayingArea.Bottom());
		Vector2D BottomLeft = new Vector2D(m_pPlayingArea.Left(), m_pPlayingArea.Bottom());

		m_vecWalls.Add(new Wall2D(new Vector2D(BottomLeft), m_pRedGoal.RightPost()));
		m_vecWalls.Add(new Wall2D(m_pRedGoal.LeftPost(), new Vector2D(TopLeft)));
		m_vecWalls.Add(new Wall2D(new Vector2D(TopLeft), new Vector2D(TopRight)));
		m_vecWalls.Add(new Wall2D(new Vector2D(TopRight), m_pBlueGoal.LeftPost()));
		m_vecWalls.Add(new Wall2D(m_pBlueGoal.RightPost(), new Vector2D(BottomRight)));
		m_vecWalls.Add(new Wall2D(new Vector2D(BottomRight), new Vector2D(BottomLeft)));
	}


	private int Update_tick = 0;

	public void Update() {
		if (m_bPaused)
			return;

		//update the balls
		m_pBall.Update();

		//update the teams
		m_pRedTeam.Update();
		m_pBlueTeam.Update();

		if (m_pBlueGoal.Scored(m_pBall) || m_pRedGoal.Scored(m_pBall)) {
			m_bGameOn = false;

			//reset the ball
			m_pBall.PlaceAtPosition(new Vector2D((double)m_cxClient / 2.0, (double)m_cyClient / 2.0));

			//get the teams ready for kickoff
			m_pRedTeam.GetFSM().ChangeState(PrepareForKickOff.Instance());
			m_pBlueTeam.GetFSM().ChangeState(PrepareForKickOff.Instance());
		}
	}
	
    public bool Render() {
        /*
		//draw the grass
		Cgdi.Instance().DarkGreenPen();
		Cgdi.Instance().DarkGreenBrush();
		Cgdi.Instance().Rect(0,0,m_cxClient, m_cyClient);

		//render regions
		if (Params.bRegions) {
			for (uint r = 0; r < m_Regions.Count; ++r) {
				m_Regions[r].Render(true);
			}
		}

		//render the goals
		Cgdi.Instance().HollowBrush();
		Cgdi.Instance().RedPen();
		Cgdi.Instance().Rect(m_pPlayingArea.Left(), (m_cyClient - Params.GoalWidth) / 2, m_pPlayingArea.Left() + 40, m_cyClient - (m_cyClient - Params.GoalWidth) / 2);

		Cgdi.Instance().BluePen();
		Cgdi.Instance().Rect(m_pPlayingArea.Right(), (m_cyClient - Params.GoalWidth) / 2, m_pPlayingArea.Right() - 40, m_cyClient - (m_cyClient - Params.GoalWidth) / 2);

		//render the pitch markings
		Cgdi.Instance().WhitePen();
		Cgdi.Instance().Circle(m_pPlayingArea.Center(), m_pPlayingArea.Width() * 0.125);
		Cgdi.Instance().Line(m_pPlayingArea.Center().x, m_pPlayingArea.Top(), m_pPlayingArea.Center().x, m_pPlayingArea.Bottom());
		Cgdi.Instance().WhiteBrush();
		Cgdi.Instance().Circle(m_pPlayingArea.Center(), 2.0);

		//the ball
		Cgdi.Instance().WhitePen();
		Cgdi.Instance().WhiteBrush();
		m_pBall.Render();

		//Render the teams
		m_pRedTeam.Render();
		m_pBlueTeam.Render();

		//render the walls
		Cgdi.Instance().WhitePen();
		for (uint w = 0; w < m_vecWalls.Count; ++w) {
			m_vecWalls[w].Render();
		}

		//show the score
		Cgdi.Instance().TextColor(Cgdi.AnonymousEnum3.red);
		Cgdi.Instance().TextAtPos((m_cxClient / 2) - 50, m_cyClient - 18, "Red: " + GlobalMembersSoccerPitch.ttos(m_pBlueGoal.NumGoalsScored()));

		Cgdi.Instance().TextColor(Cgdi.AnonymousEnum3.blue);
		Cgdi.Instance().TextAtPos((m_cxClient / 2) + 10, m_cyClient - 18, "Blue: " + GlobalMembersSoccerPitch.ttos(m_pRedGoal.NumGoalsScored()));
        */
		return true;
	}

	public void TogglePause() {
		m_bPaused = !m_bPaused;
	}
	public bool Paused() {
		return m_bPaused;
	}

	public int cxClient() {
		return m_cxClient;
	}
	public int cyClient() {
		return m_cyClient;
	}

	public bool GoalKeeperHasBall() {
		return m_bGoalKeeperHasBall;
	}
	public void SetGoalKeeperHasBall(bool b) {
		m_bGoalKeeperHasBall = b;
	}

	public Region PlayingArea() {
		return m_pPlayingArea;
	}
	public List<Wall2D> Walls() {
		return m_vecWalls;
	}
	public SoccerBall Ball() {
		return m_pBall;
	}

	public Region GetRegionFromIndex(int idx) {
		Debug.Assert((idx >= 0) && (idx < (int)m_Regions.Count));

		return m_Regions[idx];
	}

	public bool GameOn() {
		return m_bGameOn;
	}
	public void SetGameOn() {
		m_bGameOn = true;
	}
	public void SetGameOff() {
		m_bGameOn = false;
	}
}