/****************************************
*	Author:		Nathaniel Meyer			*
*	E-Mail:		nath_meyer@hotmail.com	*
*	Website:	http://www.nutty.ca		*
*										*
*   You are free to use, redistribute,  *
*   and alter this file in anyway, so   *
*   long as credit is given where due.	*
****************************************/


#ifndef FSM_h
#define FSM_h


// Necessary includes
	#include "State.h"


/*
	FSM Class
		- The FSM class controls a set of states
		- All state transitions depend on how the states are configured
		- Configure a set of states and then upload them to the FSM
*/
class FSM
{
	private:
		State *cStateList;
		State *cCurrentState;

	public:
		/*
			Constructor / Destructor
		*/
		FSM ();
		~FSM ();

		/*
			addStates
				@param: cState
					- Pointer to a list of states that the FSM will use
		*/
		void addStates (State *cState);

		/*
			events
				@param: event
					- Event that has occured
				@param: args
					- Set of arguments to pass into the next state (if any)
				- Returns true if successful
				- Incomming events are for starting the FSM
				- Outgoing events are also "incoming" when transitioning between states
		*/
		bool inEvent (char *event, char *args);
		bool outEvent (char *event, char *args);

		/*
			Accessor Methods
		*/
		char *getCurrentStateName ();
};


#endif