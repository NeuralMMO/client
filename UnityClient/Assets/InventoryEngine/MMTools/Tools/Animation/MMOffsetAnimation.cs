using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    public class MMOffsetAnimation : MonoBehaviour
    {
        public float MinimumRandomRange = 0f;
        public float MaximumRandomRange = 1f;
        public int AnimationLayerID = 0;
        public bool OffsetOnStart = true;

        protected Animator _animator;
        protected AnimatorStateInfo _stateInfo;

        protected virtual void Awake()
        {
            _animator = this.gameObject.GetComponent<Animator>();
        }

        protected virtual void Start()
        {
            OffsetCurrentAnimation();
        }

        public virtual void OffsetCurrentAnimation()
        {
            if (!OffsetOnStart)
            {
                return;
            }
            _stateInfo = _animator.GetCurrentAnimatorStateInfo(AnimationLayerID);
            _animator.Play(_stateInfo.fullPathHash, -1, Random.Range(MinimumRandomRange, MaximumRandomRange));
        }	
	}
}
