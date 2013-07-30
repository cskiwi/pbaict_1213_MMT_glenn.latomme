/****************************************
*	Author:		Nathaniel Meyer			*
*	E-Mail:		nath_meyer@hotmail.com	*
*	Website:	http://www.nutty.ca		*
*										*
*   You are free to use, redistribute,  *
*   and alter this file in anyway, so   *
*   long as credit is given where due.	*
****************************************/


#ifndef State_h
#define State_h


// Necessary includes
	#include <string.h>


/*
	State Class
		- A state consists of a set of specifications and transitions
		- A state can only do what its told to and nothing more
*/
class State
{
	private:
		/*
			funcPtr / func
				- Function pointer for state actions
				- All state functions are in the form: char*, suplemented with args
		*/
		typedef void* (*mFuncPtr) (char *args);
		mFuncPtr mFunc;

		struct _transition
		{
			/*
				expr guardCondition	NOT IMPLEMENTED
				expr action			NOT IMPLEMENTED
				expr sendEvent		NOT IMPLEMENTED
				expr sendArguments	NOT IMPLEMENTED
				expr sendTarget		NOT IMPLEMENTED
			*/
			//State *cFrom;			// From State	NOT NEEDED
			State *cTo;				// To State

			char event[50];			// Event ties to outgoing state
		} mTransition[6];

		/*
			Specification
				- There are exactly 4 specifications as defined in eWhen
		*/
		struct _specification
		{
			void *func;			// Pointer to the action
			char name[50],		// Name of the action
				 event[50];		// Event this specification is tied to
			int type;			// Type of action
		} mSpecification[10];	// 3 main actions + 4-mMaxEvents

		/*
			eWhen
				- Actions to define within the state
		*/
		enum eWhen
		{
			eOnEntry = 0,	// Action taken on entering the state
			eDo,			// Action taken during entering the state until leaving the state
			eOnExit,		// Action taken on leaving the state
			eOnEvent		// Action taken based on the event received
							// - Specify Event, Arguments, and Condition
		};

		/*
			eType
				- Specifies the action type
				- Only action is supported
		*/
		enum eType
		{
			eAction = 0,	// Action is a function
							// Specify Name
			eSendEvent		// Send an event
							// - Specify Name, Arguments, and Target
		};

		char mName[50],		// Name of the state
			 mArgs[255],	// History
			 mEvent[50];	// History

		int mNumTransitions,	// Number of transitions a state outputs
			mNumEvents,			// Number of events supported in this state
			mMaxEvents;			// Max number of events (6)

	public:
		/*
			Constructor / Destructor
		*/
		State ();
		~State ();

		/*
			Clean
				- Cleans/Resets all data within the state
		*/
		void clean ();

		/*
			Transitions
				@param: event
					- Name which identifies the proper action to take
				@param: args
					- If provided, it will be passed on to the action
				- Return true if switch is successful
		*/
		void addTransition (char *event, State *cState);
		bool incoming (char *event, char *args);
		State *outgoing (char *event);

		/*
			addAction
				@param: when
					- Specify the action activation (0-4)
				@param: type
					- Specify the action type (0-1)
				@param: name
					- Name of the action to call
				@param: event
					- Name of the event to cause the action
				@param: funcPtr
					- Specify what function will be executed for this state
		*/
		void addAction (int when, int type, char *name, void *funcPtr);	// Set default action
		void addAction (int when, int type, char *name, char *event, void *funcPtr);	// Set OnEvent action

		/*
			Operator Methods
		*/
		void setName (char *name);

		/*
			Accessor Methods
		*/
		char *getName ();
		int getNumEvents ();
		int getNumTransitions ();
};


#endif