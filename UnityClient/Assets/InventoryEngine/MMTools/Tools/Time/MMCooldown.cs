using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A class to handle cooldown related properties and their resource consumption over time
    /// Remember to initialize it (once) and update it every frame from another class
    /// </summary>
    [System.Serializable]    
    public class MMCooldown 
    {
        /// all possible states for the object
        public enum CooldownStates { Idle, Consuming, PauseOnEmpty, Refilling }
        /// if this is true, the cooldown won't do anything
        public bool Unlimited = false;
        /// the time it takes, in seconds, to consume the object
        public float ConsumptionDuration = 2f;
        /// the pause to apply before refilling once the object's been depleted
        public float PauseOnEmptyDuration = 1f;
        /// the duration of the refill, in seconds, if uninterrupted
        public float RefillDuration = 1f;
        /// whether or not the refill can be interrupted by a new Start instruction
        public bool CanInterruptRefill = true;
        [MMReadOnly]
        /// the current state of the object
        public CooldownStates CooldownState = CooldownStates.Idle;
        [MMReadOnly]
        /// the amount of duration left in the object at any given time
        public float CurrentDurationLeft;

        protected WaitForSeconds _pauseOnEmptyWFS;
        protected float _emptyReachedTimestamp = 0f;

        /// <summary>
        /// An init method that ensures the object is reset
        /// </summary>
        public virtual void Initialization()
        {
            _pauseOnEmptyWFS = new WaitForSeconds(PauseOnEmptyDuration);
            CurrentDurationLeft = ConsumptionDuration;
            CooldownState = CooldownStates.Idle;
            _emptyReachedTimestamp = 0f;
        }

        /// <summary>
        /// Starts consuming the cooldown object if possible
        /// </summary>
        public virtual void Start()
        {
            if (Ready())
            {
                CooldownState = CooldownStates.Consuming;
            }
        }

        public virtual bool Ready()
        {
            if (Unlimited)
            {
                return true;
            }
            if (CooldownState == CooldownStates.Idle)
            {
                return true;
            }
            if ((CooldownState == CooldownStates.Refilling) && (CanInterruptRefill))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops consuming the object 
        /// </summary>
        public virtual void Stop()
        {
            if (CooldownState == CooldownStates.Consuming)
            {
                CooldownState = CooldownStates.PauseOnEmpty;
            }
        }

        /// <summary>
        /// Processes the object's state machine
        /// </summary>
        public virtual void Update()
        {
            if (Unlimited)
            {
                return;
            }

            switch (CooldownState)
            {
                case CooldownStates.Idle:
                    break;

                case CooldownStates.Consuming:
                    CurrentDurationLeft = CurrentDurationLeft - Time.deltaTime;
                    if (CurrentDurationLeft <= 0f)
                    {
                        CurrentDurationLeft = 0f;
                        _emptyReachedTimestamp = Time.time;
                        CooldownState = CooldownStates.PauseOnEmpty;
                    }
                    break;

                case CooldownStates.PauseOnEmpty:
                    if (Time.time - _emptyReachedTimestamp >= PauseOnEmptyDuration)
                    {
                        CooldownState = CooldownStates.Refilling;
                    }
                    break;

                case CooldownStates.Refilling:
                    CurrentDurationLeft += RefillDuration * Time.deltaTime;
                    if (CurrentDurationLeft >= ConsumptionDuration)
                    {
                        CurrentDurationLeft = ConsumptionDuration;
                        CooldownState = CooldownStates.Idle;
                    }
                    break;
            }
        }
	}
}
