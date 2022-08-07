using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Actions are behaviours and describe what your character is doing. Examples include patrolling, shooting, jumping, etc. 
    /// </summary>
    public abstract class AIAction : MonoBehaviour
    {
        public string Label;
        public abstract void PerformAction();
        public bool ActionInProgress { get; set; }
        protected AIBrain _brain;

        /// <summary>
        /// On Start we trigger our init method
        /// </summary>
        protected virtual void Start()
        {
            _brain = this.gameObject.GetComponent<AIBrain>();
            Initialization();
        }

        /// <summary>
        /// Initializes the action. Meant to be overridden
        /// </summary>
        protected virtual void Initialization()
        {

        }

        /// <summary>
        /// Describes what happens when the brain enters the state this action is in. Meant to be overridden.
        /// </summary>
        public virtual void OnEnterState()
        {
            ActionInProgress = true;
        }

        /// <summary>
        /// Describes what happens when the brain exits the state this action is in. Meant to be overridden.
        /// </summary>
        public virtual void OnExitState()
        {
            ActionInProgress = false;
        }
    }
}