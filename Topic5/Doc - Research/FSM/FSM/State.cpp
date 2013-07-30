/****************************************
*	Author:		Nathaniel Meyer			*
*	E-Mail:		nath_meyer@hotmail.com	*
*	Website:	http://www.nutty.ca		*
*										*
*   You are free to use, redistribute,  *
*   and alter this file in anyway, so   *
*   long as credit is given where due.	*
****************************************/


#include "State.h"


/*
	Constructor / Destructor
*/
State::State ()
{
	mMaxEvents = 6;

	clean();
}

State::~State ()
{
}

/*
	Clean
*/
void State::clean ()
{
	strcpy(mName, "");
	mNumTransitions = 0;
	mNumEvents = 0;

	// Clean transitions
	for (int i = 0; i < mMaxEvents; i++)
	{
		strcpy(mTransition[i].event, "");
		mTransition[i].cTo = NULL;
	}

	// Clean actions
	for (int i = 0; i < (4+mMaxEvents); i++)
	{
		mSpecification[i].func = NULL;
		strcpy(mSpecification[i].name, "");
		strcpy(mSpecification[i].event, "");
		mSpecification[i].type = eAction;
	}
}

/*
	Transitions
*/
void State::addTransition (char *event, State *cState)
{
	// Add a transition to the list
	if (mNumTransitions < mMaxEvents)
	{
		strcpy(mTransition[mNumTransitions].event, event);
		mTransition[mNumTransitions].cTo = cState;
		++mNumTransitions;
	}
}

bool State::incoming (char *event, char *args)
{
	strcpy(mEvent, event);
	strcpy(mArgs, args);

	/*
		Loop through all OnEvent functions and process them
		Otherwise continue with standard actions
	*/
	if (mNumEvents > 0)
	{
		for (int i = eOnEvent; i < (eOnEvent+mNumEvents); i++)
		{
			if ( strcmp(mSpecification[i].event, event) == 0 )
			{
				// Run the OnEvent action
				mFunc = (mFuncPtr)mSpecification[i].func;
				mFunc(args);

				return true;
			}
		}
	}
	else
	{
		// Perform OnEntry and OnDo
		for (int i = 0; i < 2; i++)
		{
			if (mSpecification[i].func != NULL)
			{
				mFunc = (mFuncPtr)mSpecification[i].func;
				mFunc(args);
			}
		}
		return true;
	}

	return false;
}

State* State::outgoing (char *event)
{
	// Find the State to which this event is tied to
	for (int i = 0; i < mNumTransitions; i++)
	{
		if ( strcmp(mTransition[i].event, event) == 0 )
		{
			// Run the exit action
			if (mSpecification[eOnExit].func != NULL)
			{
				mFunc = (mFuncPtr)mSpecification[eOnExit].func;
				mFunc(mArgs);
			}

			return mTransition[i].cTo;
		}
	}

	return NULL;
}

/*
	addAction
*/
void State::addAction (int when, int type, char *name, void *funcPtr)
{
	if ( (when == eOnEntry) || (when == eDo) || (when == eOnExit) )
	{
		if ( type == eAction )
		{
			strcpy(mSpecification[when].name, name);
			mSpecification[when].type = type;
			mSpecification[when].func = funcPtr;
		}
	}
}

void State::addAction (int when, int type, char *name, char *event, void *funcPtr)
{
	if ( (when == eOnEvent) )
	{
		if ( (type == eAction) && (mNumEvents < mMaxEvents) )
		{
			strcpy(mSpecification[when+mNumEvents].name, name);
			strcpy(mSpecification[when+mNumEvents].event, event);
			mSpecification[when+mNumEvents].type = type;
			mSpecification[when+mNumEvents].func = funcPtr;

			++mNumEvents;
		}
	}
}

/*
	Operator Methods
*/
void State::setName (char *name)
{
	strcpy(mName, name);
}

/*
	Accessor Methods
*/
char *State::getName ()
{
	return mName;
}

int State::getNumEvents ()
{
	return mNumEvents;
}

int State::getNumTransitions ()
{
	return mNumTransitions;
}