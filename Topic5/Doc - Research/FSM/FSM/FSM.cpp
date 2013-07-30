/****************************************
*	Author:		Nathaniel Meyer			*
*	E-Mail:		nath_meyer@hotmail.com	*
*	Website:	http://www.nutty.ca		*
*										*
*   You are free to use, redistribute,  *
*   and alter this file in anyway, so   *
*   long as credit is given where due.	*
****************************************/


#include "FSM.h"


/*
	Constructor / Destructor
*/
FSM::FSM ()
{
	cStateList = NULL;
	cCurrentState = NULL;
}

FSM::~FSM ()
{
}

/*
	addStates
*/
void FSM::addStates (State *cStates)
{
	cStateList = cStates;
	cCurrentState = &cStateList[0];
}

/*
	event
*/
bool FSM::inEvent (char *event, char *args)
{
	// Input the event into the current state
	if ( cCurrentState != NULL )
	{
		return cCurrentState->incoming(event, args);
	}

	return false;
}

bool FSM::outEvent (char *event, char *args)
{
	// Output the event into the current state
	if ( (cStateList != NULL) && (cCurrentState != NULL) )
	{
		State *cTemp = cCurrentState->outgoing(event);
		if ( cTemp != NULL )
		{
			// Set the new output state as current and feed it the event
			cCurrentState = cTemp;
			cCurrentState->incoming(event, args);

			return true;
		}
	}

	return false;
}

/*
	Accessor Methods
*/
char *FSM::getCurrentStateName ()
{
	return cCurrentState->getName();
}