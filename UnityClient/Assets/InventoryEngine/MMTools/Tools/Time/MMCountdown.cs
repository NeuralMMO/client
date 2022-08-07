using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    [RequireComponent(typeof(Text))]
    public class MMCountdown : MonoBehaviour
    {
        [Serializable]
        /// <summary>
        /// A class to store floor information
        /// </summary>
        public class MMCountdownFloor
        {
            /// the value (in seconds) for this floor. Every FloorValue, the corresponding event will be triggered
            public float FloorValue;
            [MMReadOnly]
            /// the time (in seconds) this floor was last triggered at
            public float LastChangedAt = 0f;
            /// the event to trigger when this floor is reached
            public UnityEvent FloorEvent;
        }

        /// the possible directions for this countdown
        public enum MMCountdownDirections { Ascending, Descending }
        
        [Header("Debug")]
        [MMReadOnly]
        /// the time left in our countdown 
        public float CurrentTime;
        [MMReadOnly]
        /// the direction of the countdown (going 1, 2, 3 if Ascending, and 3, 2, 1 if Descending)
        public MMCountdownDirections Direction;

        [Header("Countdown")]
        [MMInformation("You can define the bounds of the countdown (how much it should count down from, and to how much, the format it should be displayed in (standard Unity float ToString formatting).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// the time (in seconds) to count down from
        public float CountdownFrom = 60f;
        /// the time (in seconds) to count down to
        public float CountdownTo = 0f;

        [Header("Display")]
        /// the format (standard Unity ToString) to use when displaying the time left in the text field
        public string Format = "00.00";
        /// whether or not values should be floored before displaying them
        public bool FloorValues = true;

        [Header("Settings")]
        [MMInformation("You can choose whether or not the countdown should automatically start on its Start, at what frequency (in seconds) it should refresh (0 means every frame), and the countdown's speed multiplier " +
            "(2 will be twice as fast, 0.5 half normal speed, etc). Floors are used to define and trigger events when certain floors are reached. For each floor, define a floor value (in seconds). Everytime this floor gets reached, the corresponding event will be triggered." +
            "Bind events here to trigger them when the countdown reaches its To destination, or every time it gets refreshed.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// if this is true, the countdown will start as soon as this object Starts
        public bool AutoStart = true;
        /// if this is true, the countdown will automatically go back to its initial value when it reaches its destination
        public bool AutoReset = false;
        /// the frequency (in seconds) at which to refresh the text field
        public float RefreshFrequency = 0.02f;
        /// the speed of the countdown (2 : twice the normal speed, 0.5 : twice slower)
        public float CountdownSpeed = 1f;

        [Header("Floors")]
        /// a list of floors this countdown will evaluate and trigger if met
        public List<MMCountdownFloor> Floors;
        
        [Header("Events")]
        /// an event to trigger when the countdown reaches its destination
        public UnityEvent CountdownCompleteEvent;
        /// an event to trigger every time the countdown text gets refreshed
        public UnityEvent CountdownRefreshEvent;

        protected Text _text;
        protected float _lastRefreshAt;
        protected bool _countdowning = false;
        protected int _lastUnitValue = 0;

        /// <summary>
        /// On Start, grabs and stores the Text component, and autostarts if needed
        /// </summary>
        protected virtual void Start()
        {
            _text = this.gameObject.GetComponent<Text>();
            Initialization();
        }

        protected virtual void Initialization()
        {
            CurrentTime = CountdownFrom;
            _lastUnitValue = (int)CurrentTime;
            Direction = (CountdownFrom > CountdownTo) ? MMCountdownDirections.Descending : MMCountdownDirections.Ascending;
            if (AutoStart)
            {
                StartCountdown();
            }
            foreach (MMCountdownFloor floor in Floors)
            {
                floor.LastChangedAt = CountdownFrom;
            }
        }

        /// <summary>
        /// On Update, updates the Time, text, checks for floors and checks for the end of the countdown
        /// </summary>
        protected virtual void Update()
        {
            // if we're not countdowning, we do nothing and exit
            if (!_countdowning)
            {
                return;
            }
            
            // we update our current time
            UpdateTime();
            UpdateText();
            CheckForFloors();
            CheckForEnd();
        }

        /// <summary>
        /// Updates the CurrentTime value by substracting the delta time, factored by the defined speed
        /// </summary>
        protected virtual void UpdateTime()
        {
            if (Direction == MMCountdownDirections.Descending)
            {
                CurrentTime -= Time.deltaTime * CountdownSpeed;
            }
            else
            {
                CurrentTime += Time.deltaTime * CountdownSpeed;
            }
        }

        /// <summary>
        /// Refreshes the text component at the specified refresh frequency
        /// </summary>
        protected virtual void UpdateText()
        {
            if (Time.time - _lastRefreshAt > RefreshFrequency)
            {
                if (_text != null)
                {
                    if (FloorValues)
                    {
                        _text.text = Mathf.Floor(CurrentTime).ToString(Format);
                    }
                    else
                    {
                        _text.text = CurrentTime.ToString(Format);
                    }                    
                }
                if (CountdownRefreshEvent != null)
                {
                    CountdownRefreshEvent.Invoke();
                }
                _lastRefreshAt = Time.time;
            }
        }

        /// <summary>
        /// Checks whether or not we've reached the end of the countdown
        /// </summary>
        protected virtual void CheckForEnd()
        {
            if (CurrentTime <= CountdownTo)
            {
                if (CountdownCompleteEvent != null)
                {
                    CountdownCompleteEvent.Invoke();
                }
                if (AutoReset)
                {
                    _countdowning = true;
                    CurrentTime = CountdownFrom;
                }
                else
                {
                    CurrentTime = CountdownTo;
                    _countdowning = false;
                }
            }
        }

        /// <summary>
        /// Every frame, checks if we've reached one of the defined floors, and triggers the corresponding events if that's the case
        /// </summary>
        protected virtual void CheckForFloors()
        {
            foreach(MMCountdownFloor floor in Floors)
            {
                if (Mathf.Abs(CurrentTime - floor.LastChangedAt) >= floor.FloorValue)
                {
                    if (floor.FloorEvent != null)
                    {
                        floor.FloorEvent.Invoke();
                    }

                    if (Direction == MMCountdownDirections.Descending)
                    {
                        if (floor.LastChangedAt == CountdownFrom)
                        {                         
                            floor.LastChangedAt = CountdownFrom - floor.FloorValue;
                        }
                        else
                        {
                            floor.LastChangedAt = floor.LastChangedAt - floor.FloorValue;
                        }
                    }
                    else
                    {
                        if (floor.LastChangedAt == CountdownFrom)
                        {
                            floor.LastChangedAt = CountdownFrom + floor.FloorValue;
                        }
                        else
                        {
                            floor.LastChangedAt = floor.LastChangedAt + floor.FloorValue;
                        }
                    }                    
                }
            }
        }

        /// <summary>
        /// Starts (or restarts) the countdown
        /// </summary>
        public virtual void StartCountdown()
        {
            _countdowning = true;
        }

        /// <summary>
        /// Stops the countdown from countdowning
        /// </summary>
        public virtual void StopCountdown()
        {
            _countdowning = false;
        }

        /// <summary>
        /// Resets the countdown, setting its current time to the one defined in the inspector
        /// </summary>
        public virtual void ResetCountdown()
        {
            CurrentTime = CountdownFrom;
            Initialization();
        }
    }
}

