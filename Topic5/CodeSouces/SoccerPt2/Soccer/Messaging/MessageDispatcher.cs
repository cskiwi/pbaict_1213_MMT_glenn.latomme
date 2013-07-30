using System.Collections.Generic;
using Soccer.Game;

namespace Soccer.Messaging {
    public class Telegram {
        //the entity that sent this telegram

        //messages can be dispatched immediately or delayed for a specified amount
        //of time. If a delay is necessary this field is stamped with the time
        //the message should be dispatched.
        public double DispatchTime;

        //any additional information that may accompany the message
        public object ExtraInfo;
        public int Msg;
        public int Receiver;
        public int Sender;

        public Telegram() {
            DispatchTime = -1;
            Sender = -1;
            Receiver = -1;
            Msg = -1;
        }

        public Telegram(double time, int sender, int receiver, int msg)
            : this(time, sender, receiver, msg, null) {}

        //C++ TO C# CONVERTER NOTE: Overloaded method(s) are created above to convert the following method having default parameters:
        //ORIGINAL LINE: Telegram(double time, int sender, int receiver, int msg, object* info = null): DispatchTime(time), Sender(sender), Receiver(receiver), Msg(msg), ExtraInfo(info)
        public Telegram(double time, int sender, int receiver, int msg, object info) {
            DispatchTime = time;
            Sender = sender;
            Receiver = receiver;
            Msg = msg;
            ExtraInfo = info;
        }
    }

    public class MessageDispatcher {
        //a std::set is used as the container for the delayed messages
        //because of the benefit of automatic sorting and avoidance
        //of duplicates. Messages are sorted by their dispatch time.
        private static readonly MessageDispatcher Instance_instance = new MessageDispatcher();
        private readonly HashSet<Telegram> PriorityQ = new HashSet<Telegram>();

        //this method is utilized by DispatchMsg or DispatchDelayedMessages.
        //This method calls the message handling member function of the receiving
        //entity, pReceiver, with the newly created telegram

        //----------------------------- Dispatch ---------------------------------
        //
        //  see description in header
        //------------------------------------------------------------------------

        private MessageDispatcher() {}

        private void Discharge(BaseGameEntity pReceiver, Telegram telegram) {
            if (!pReceiver.HandleMessage(telegram)) {
                //telegram could not be handled
#if SHOW_MESSAGING_INFO
#if debug_con_ConditionalDefinition1
			DebugConsole.Instance() << "Message not handled" << "";
#elif debug_con_ConditionalDefinition2
			CSink.Instance() << "Message not handled" << "";
#else
			debug_con << "Message not handled" << "";
#endif
#endif
            }
        }

        //copy ctor and assignment should be private
        //	MessageDispatcher(MessageDispatcher NamelessParameter);
        //C++ TO C# CONVERTER NOTE: This 'CopyFrom' method was converted from the original C++ copy assignment operator:
        //ORIGINAL LINE: MessageDispatcher& operator =(const MessageDispatcher&);
        //	MessageDispatcher CopyFrom(MessageDispatcher NamelessParameter);


        //uncomment below to send message info to the debug window
        //#define SHOW_MESSAGING_INFO

        //--------------------------- Instance ----------------------------------------
        //
        //   this class is a singleton
        //-----------------------------------------------------------------------------
        //C++ TO C# CONVERTER NOTE: This was formerly a static local variable declaration (not allowed in C#):

        public static MessageDispatcher Instance() {
            //C++ TO C# CONVERTER NOTE: This static local variable declaration (not allowed in C#) has been moved just prior to the method:
            //		static MessageDispatcher instance;

            return Instance_instance;
        }

        //send a message to another agent. Receiving agent is referenced by ID.

        //---------------------------- DispatchMsg ---------------------------
        //
        //  given a message, a receiver, a sender and any time delay, this function
        //  routes the message to the correct agent (if no delay) or stores
        //  in the message queue to be dispatched at the correct time
        //------------------------------------------------------------------------
        public void DispatchMsg(double delay, int sender, int receiver, int msg) {
            DispatchMsg(delay, sender, receiver, msg, null);
        }

        //C++ TO C# CONVERTER NOTE: Overloaded method(s) are created above to convert the following method having default parameters:
        //ORIGINAL LINE: void DispatchMsg(double delay, int sender, int receiver, int msg, object* AdditionalInfo = null)
        public void DispatchMsg(double delay, int sender, int receiver, int msg, object AdditionalInfo) {
            //get a pointer to the receiver
            BaseGameEntity pReceiver = EntityManager.Instance().GetEntityFromID(receiver);

            //make sure the receiver is valid
            if (pReceiver == null) {
#if SHOW_MESSAGING_INFO
#if debug_con_ConditionalDefinition1
			DebugConsole.Instance() << "\nWarning! No Receiver with ID of " << receiver << " found" << "";
#elif debug_con_ConditionalDefinition2
			CSink.Instance() << "\nWarning! No Receiver with ID of " << receiver << " found" << "";
#else
			debug_con << "\nWarning! No Receiver with ID of " << receiver << " found" << "";
#endif
#endif

                return;
            }

            //create the telegram
            var telegram = new Telegram(0, sender, receiver, msg, AdditionalInfo);

            //if there is no delay, route telegram immediately
            if (delay <= 0.0) {
#if SHOW_MESSAGING_INFO
#if debug_con_ConditionalDefinition1
			DebugConsole.Instance() << "\nTelegram dispatched at time: " << FrameCounter.Instance().GetCurrentFrame() << " by " << sender << " for " << receiver << ". Msg is " << msg << "";
#elif debug_con_ConditionalDefinition2
			CSink.Instance() << "\nTelegram dispatched at time: " << FrameCounter.Instance().GetCurrentFrame() << " by " << sender << " for " << receiver << ". Msg is " << msg << "";
#else
			debug_con << "\nTelegram dispatched at time: " << FrameCounter.Instance().GetCurrentFrame() << " by " << sender << " for " << receiver << ". Msg is " << msg << "";
#endif
#endif

                //send the telegram to the recipient
                Discharge(pReceiver, telegram);
            }

                //else calculate the time when the telegram should be dispatched
            else {
                double CurrentTime = FrameCounter.Instance().GetCurrentFrame();

                telegram.DispatchTime = CurrentTime + delay;

                //and put it in the queue
                PriorityQ.Add(telegram);

#if SHOW_MESSAGING_INFO
#if debug_con_ConditionalDefinition1
			DebugConsole.Instance() << "\nDelayed telegram from " << sender << " recorded at time " << FrameCounter.Instance().GetCurrentFrame() << " for " << receiver << ". Msg is " << msg << "";
#elif debug_con_ConditionalDefinition2
			CSink.Instance() << "\nDelayed telegram from " << sender << " recorded at time " << FrameCounter.Instance().GetCurrentFrame() << " for " << receiver << ". Msg is " << msg << "";
#else
			debug_con << "\nDelayed telegram from " << sender << " recorded at time " << FrameCounter.Instance().GetCurrentFrame() << " for " << receiver << ". Msg is " << msg << "";
#endif
#endif
            }
        }

        //send out any delayed messages. This method is called each time through
        //the main game loop.

        //---------------------- DispatchDelayedMessages -------------------------
        //
        //  This function dispatches any telegrams with a timestamp that has
        //  expired. Any dispatched telegrams are removed from the queue
        //------------------------------------------------------------------------
        public void DispatchDelayedMessages() {
            //first get current time
            double CurrentTime = FrameCounter.Instance().GetCurrentFrame();

            //now peek at the queue to see if any telegrams need dispatching.
            //remove all telegrams from the front of the queue that have gone
            //past their sell by date
            while (PriorityQ.Count > 0 && (PriorityQ.GetEnumerator().DispatchTime < CurrentTime) &&
                   (PriorityQ.GetEnumerator().DispatchTime > 0)) {
                //read the telegram from the front of the queue
                Telegram telegram = *PriorityQ.GetEnumerator();

                //find the recipient
                BaseGameEntity pReceiver = EntityManager.Instance().GetEntityFromID(telegram.Receiver);

#if SHOW_MESSAGING_INFO
#if debug_con_ConditionalDefinition1
			DebugConsole.Instance() << "\nQueued telegram ready for dispatch: Sent to " << pReceiver.ID() << ". Msg is " << telegram.Msg << "";
#elif debug_con_ConditionalDefinition2
			CSink.Instance() << "\nQueued telegram ready for dispatch: Sent to " << pReceiver.ID() << ". Msg is " << telegram.Msg << "";
#else
			debug_con << "\nQueued telegram ready for dispatch: Sent to " << pReceiver.ID() << ". Msg is " << telegram.Msg << "";
#endif
#endif

                //send the telegram to the recipient
                Discharge(pReceiver, telegram);

                //remove it from the queue
                PriorityQ.erase(PriorityQ.GetEnumerator());
            }
        }
    }
}