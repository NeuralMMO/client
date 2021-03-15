using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Use this class to enable or disable other gameobjects automatically on Start or Awake
    /// </summary>
    public class MMActivationOnStart : MonoBehaviour
    {
        /// The possible modes that define whether this should run at Awake or Start
        public enum Modes { Awake, Start }
        /// the selected mode for this instance
        public Modes Mode = Modes.Start;
        /// if true, objects will be activated on start, disabled otherwise
        public bool StateOnStart = true;
        /// the list of gameobjects whose active state will be affected on start
        public List<GameObject> TargetObjects;

        /// <summary>
        /// On Awake, we set our state if needed
        /// </summary>
        protected virtual void Awake()
        {
            if (Mode != Modes.Awake)
            {
                return;
            }
            SetState();
        }

        /// <summary>
        /// On Start, we set our state if needed
        /// </summary>
        protected virtual void Start()
        {
            if (Mode != Modes.Start)
            {
                return;
            }
            SetState();
        }        

        /// <summary>
        /// Sets the state of all target objects
        /// </summary>
        protected virtual void SetState()
        {
            foreach (GameObject obj in TargetObjects)
            {
                obj.SetActive(StateOnStart);
            }
        }
    }
}
