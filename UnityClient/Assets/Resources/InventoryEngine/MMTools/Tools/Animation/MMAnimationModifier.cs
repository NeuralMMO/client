using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this script to an animation in Mecanim and you'll be able to control its start position and speed
    /// </summary>
    public class MMAnimationModifier : StateMachineBehaviour
    {
        [MMVectorAttribute("Min", "Max")]
        /// the min and max values for the start position of the animation (between 0 and 1)
        public Vector2 StartPosition = new Vector2(0, 0);

        [MMVectorAttribute("Min", "Max")]
        /// the min and max values for the animation speed (1 is normal)
        public Vector2 AnimationSpeed = new Vector2(1, 1);

        protected bool _enteredState = false;
        protected float _initialSpeed;

        /// <summary>
        /// On state enter, we modify our speed and start position
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            // handle speed
            _initialSpeed = animator.speed;
            animator.speed = Random.Range(AnimationSpeed.x, AnimationSpeed.y);

            // handle start position
            if (!_enteredState)
            {
                animator.Play(stateInfo.fullPathHash, layerIndex, Random.Range(StartPosition.x, StartPosition.y));
            }
            _enteredState = !_enteredState;
        }

        /// <summary>
        /// On state exit, we restore our speed
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            animator.speed = _initialSpeed;            
        }
    }
}
