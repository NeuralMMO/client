//#define EVENTROUTER_THROWEXCEPTIONS 
#if EVENTROUTER_THROWEXCEPTIONS
//#define EVENTROUTER_REQUIRELISTENER // Uncomment this if you want listeners to be required for sending events.
#endif

using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// MMGameEvents are used throughout the game for general game events (game started, game ended, life lost, etc.)
	/// </summary>
	public struct MMGameEvent
	{
		public string EventName;
		public MMGameEvent(string newName)
		{
			EventName = newName;
        }
        static MMGameEvent e;
        public static void Trigger(string newName)
        {
            e.EventName = newName;
            MMEventManager.TriggerEvent(e);
        }
    }
    
    /// <summary>
    /// This class handles event management, and can be used to broadcast events throughout the game, to tell one class (or many) that something's happened.
    /// Events are structs, you can define any kind of events you want. This manager comes with MMGameEvents, which are 
    /// basically just made of a string, but you can work with more complex ones if you want.
    /// 
    /// To trigger a new event, from anywhere, do YOUR_EVENT.Trigger(YOUR_PARAMETERS)
    /// So MMGameEvent.Trigger("Save"); for example will trigger a Save MMGameEvent
    /// 
    /// you can also call MMEventManager.TriggerEvent(YOUR_EVENT);
    /// For example : MMEventManager.TriggerEvent(new MMGameEvent("GameStart")); will broadcast an MMGameEvent named GameStart to all listeners.
    ///
    /// To start listening to an event from any class, there are 3 things you must do : 
    ///
    /// 1 - tell that your class implements the MMEventListener interface for that kind of event.
    /// For example: public class GUIManager : Singleton<GUIManager>, MMEventListener<MMGameEvent>
    /// You can have more than one of these (one per event type).
    ///
    /// 2 - On Enable and Disable, respectively start and stop listening to the event :
    /// void OnEnable()
    /// {
    /// 	this.MMEventStartListening<MMGameEvent>();
    /// }
    /// void OnDisable()
    /// {
    /// 	this.MMEventStopListening<MMGameEvent>();
    /// }
    /// 
    /// 3 - Implement the MMEventListener interface for that event. For example :
    /// public void OnMMEvent(MMGameEvent gameEvent)
    /// {
    /// 	if (gameEvent.eventName == "GameOver")
    ///		{
    ///			// DO SOMETHING
    ///		}
    /// } 
    /// will catch all events of type MMGameEvent emitted from anywhere in the game, and do something if it's named GameOver
    /// </summary>
    [ExecuteAlways]
	public static class MMEventManager 
	{
	    private static Dictionary<Type, List<MMEventListenerBase>> _subscribersList;

		static MMEventManager()
	    {
	        _subscribersList = new Dictionary<Type, List<MMEventListenerBase>>();
	    }

	    /// <summary>
	    /// Adds a new subscriber to a certain event.
	    /// </summary>
		/// <param name="listener">listener.</param>
	    /// <typeparam name="MMEvent">The event type.</typeparam>
	    public static void AddListener<MMEvent>( MMEventListener<MMEvent> listener ) where MMEvent : struct
	    {
	        Type eventType = typeof( MMEvent );

	        if( !_subscribersList.ContainsKey( eventType ) )
	            _subscribersList[eventType] = new List<MMEventListenerBase>();

	        if( !SubscriptionExists( eventType, listener ) )
	            _subscribersList[eventType].Add( listener );
	    }

	    /// <summary>
	    /// Removes a subscriber from a certain event.
	    /// </summary>
		/// <param name="listener">listener.</param>
	    /// <typeparam name="MMEvent">The event type.</typeparam>
	    public static void RemoveListener<MMEvent>( MMEventListener<MMEvent> listener ) where MMEvent : struct
	    {
	        Type eventType = typeof( MMEvent );

	        if( !_subscribersList.ContainsKey( eventType ) )
	        {
				#if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
				#else
					return;
				#endif
	        }

			List<MMEventListenerBase> subscriberList = _subscribersList[eventType];
	        bool listenerFound;
			listenerFound = false;

			if (listenerFound)
			{
				
			}

			for (int i = 0; i<subscriberList.Count; i++)
			{
				if( subscriberList[i] == listener )
				{
					subscriberList.Remove( subscriberList[i] );
					listenerFound = true;

					if( subscriberList.Count == 0 )
						_subscribersList.Remove( eventType );

					return;
				}
			}

			#if EVENTROUTER_THROWEXCEPTIONS
		        if( !listenerFound )
		        {
					throw new ArgumentException( string.Format( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
		        }
			#endif
	    }

	    /// <summary>
	    /// Triggers an event. All instances that are subscribed to it will receive it (and will potentially act on it).
	    /// </summary>
		/// <param name="newEvent">The event to trigger.</param>
	    /// <typeparam name="MMEvent">The 1st type parameter.</typeparam>
	    public static void TriggerEvent<MMEvent>( MMEvent newEvent ) where MMEvent : struct
	    {
	        List<MMEventListenerBase> list;
	        if( !_subscribersList.TryGetValue( typeof( MMEvent ), out list ) )
			#if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( MMEvent ).ToString() ) );
			#else
			                return;
			#endif
			
			for (int i=0; i<list.Count; i++)
			{
				( list[i] as MMEventListener<MMEvent> ).OnMMEvent( newEvent );
			}
	    }

	    /// <summary>
	    /// Checks if there are subscribers for a certain type of events
	    /// </summary>
	    /// <returns><c>true</c>, if exists was subscriptioned, <c>false</c> otherwise.</returns>
	    /// <param name="type">Type.</param>
	    /// <param name="receiver">Receiver.</param>
	    private static bool SubscriptionExists( Type type, MMEventListenerBase receiver )
	    {
	        List<MMEventListenerBase> receivers;

	        if( !_subscribersList.TryGetValue( type, out receivers ) ) return false;

	        bool exists = false;

			for (int i=0; i<receivers.Count; i++)
			{
				if( receivers[i] == receiver )
				{
					exists = true;
					break;
				}
			}

	        return exists;
	    }
	}

	/// <summary>
	/// Static class that allows any class to start or stop listening to events
	/// </summary>
	public static class EventRegister
	{
	    public delegate void Delegate<T>( T eventType );

	    public static void MMEventStartListening<EventType>( this MMEventListener<EventType> caller ) where EventType : struct
	    {
			MMEventManager.AddListener<EventType>( caller );
	    }

		public static void MMEventStopListening<EventType>( this MMEventListener<EventType> caller ) where EventType : struct
	    {
			MMEventManager.RemoveListener<EventType>( caller );
	    }
	}

	/// <summary>
	/// Event listener basic interface
	/// </summary>
	public interface MMEventListenerBase { };

	/// <summary>
	/// A public interface you'll need to implement for each type of event you want to listen to.
	/// </summary>
	public interface MMEventListener<T> : MMEventListenerBase
	{
	    void OnMMEvent( T eventType );
	}
}