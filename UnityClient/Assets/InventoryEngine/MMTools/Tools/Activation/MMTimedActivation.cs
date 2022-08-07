using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this component to an object and it'll be auto destroyed X seconds after its Start()
    /// </summary>
    public class MMTimedActivation : MonoBehaviour
    {
        /// the possible activation modes
        public enum TimedStatusChange { Enable, Disable, Destroy }
        /// the possible trigger modes
        public enum ActivationModes { Awake, Start, OnEnable, OnTriggerEnter, OnTriggerExit, OnTriggerEnter2D, OnTriggerExit2D, Script }
        /// the possible ways to check if the collider matches
        public enum TriggerModes { None, Tag, Layer }
        [Header("Trigger Mode")]
        /// the moment you want the countdown to state change to start
        public ActivationModes ActivationMode = ActivationModes.Start;
        /// the target layer for activation if using OnTriggerEnter or OnTriggerExit
        [MMEnumCondition("ActivationMode", (int)ActivationModes.OnTriggerEnter, (int)ActivationModes.OnTriggerExit)]
        public TriggerModes TriggerMode;
        /// the layer the target collider should be on
        [MMEnumCondition("TriggerMode", (int)TriggerModes.Layer)]
        public LayerMask TargetTriggerLayer;
        /// the tag the target collider should have
        [MMEnumCondition("TriggerMode", (int)TriggerModes.Tag)]
        public string TargetTriggerTag;
        [Header("Delay in seconds")]
        /// The time (in seconds) before we destroy the object
        public float TimeBeforeStateChange = 2;
        [Header("Timed Activation")]
        /// the possible targets you want the state to change
        public List<GameObject> TargetGameObjects;
        /// the destruction mode for this object : destroy or disable
        public TimedStatusChange TimeDestructionMode = TimedStatusChange.Disable;
        [Header("Actions")]
        /// Unity events to trigger after the delay
        public UnityEvent TimedActions;

        protected WaitForSeconds _timeBeforeDestructionWFS;

        /// <summary>
        /// On awake, initialize our delay and trigger our change state countdown if needed
        /// </summary>
        protected virtual void Awake()
        {
            _timeBeforeDestructionWFS = new WaitForSeconds(TimeBeforeStateChange);
            if (ActivationMode == ActivationModes.Awake)
            {
                StartChangeState();
            }
        }

        /// <summary>
        /// Call this method to start the countdown to activation
        /// </summary>
        public virtual void TriggerSequence()
        {
            StartChangeState();
        }

        /// <summary>
        /// On start, trigger our change state countdown if needed
        /// </summary>
        protected virtual void Start()
        {
            if (ActivationMode == ActivationModes.Start)
            {
                StartChangeState();
            }
        }

        /// <summary>
        /// On enable, trigger our change state countdown if needed
        /// </summary>
        protected virtual void OnEnable()
        {
            if (ActivationMode == ActivationModes.Start)
            {
                StartChangeState();
            }
        }

        /// <summary>
        /// On trigger enter, we start our countdown if needed
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter(Collider collider)
        {
            if ((ActivationMode == ActivationModes.OnTriggerEnter) && (CorrectTagOrLayer(collider.gameObject)))
            {
                StartChangeState();
            }
        }

        /// <summary>
        /// On trigger exit, we start our countdown if needed
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerExit(Collider collider)
        {
            if ((ActivationMode == ActivationModes.OnTriggerEnter) && (CorrectTagOrLayer(collider.gameObject)))
            {
                StartChangeState();
            }
        }

        /// <summary>
        /// On trigger enter 2D, we start our countdown if needed
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter2d(Collider2D collider)
        {
            if ((ActivationMode == ActivationModes.OnTriggerEnter) && (CorrectTagOrLayer(collider.gameObject)))
            {
                StartChangeState();
            }
        }

        /// <summary>
        /// On trigger exit 2D, we start our countdown if needed
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerExit2d(Collider2D collider)
        {
            if ((ActivationMode == ActivationModes.OnTriggerEnter) && (CorrectTagOrLayer(collider.gameObject)))
            {
                StartChangeState();
            }
        }

        /// <summary>
        /// Returns true if the target matches our settings, false otherwise
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual bool CorrectTagOrLayer(GameObject target)
        {
            switch (TriggerMode)
            {
                case TriggerModes.None:
                    return true;
                case TriggerModes.Layer:
                    if (((1 << target.layer) & TargetTriggerLayer) != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case TriggerModes.Tag:
                    return (target.CompareTag(TargetTriggerTag));                    
            }
            return false;
        }

        /// <summary>
        /// On start change state, starts the timed activation
        /// </summary>
        protected virtual void StartChangeState()
        {
            StartCoroutine(TimedActivationSequence());
        }

        /// <summary>
        /// Waits and triggers state change and events
        /// </summary>
        protected virtual IEnumerator TimedActivationSequence()
        {
            yield return _timeBeforeDestructionWFS;
            StateChange();
            Activate();
        }

        /// <summary>
        /// Triggers actions if needed
        /// </summary>
        protected virtual void Activate()
        {
            if (TimedActions != null)
            {
                TimedActions.Invoke();
            }
        }

        /// <summary>
        /// Changes the object's status or destroys it
        /// </summary>
        protected virtual void StateChange()
        {
            foreach(GameObject targetGameObject in TargetGameObjects)
            {
                switch (TimeDestructionMode)
                {
                    case TimedStatusChange.Destroy:
                        Destroy(targetGameObject);
                        break;

                    case TimedStatusChange.Disable:
                        targetGameObject.SetActive(false);
                        break;

                    case TimedStatusChange.Enable:
                        targetGameObject.SetActive(true);
                        break;
                }
            }            
        }
    }
}
