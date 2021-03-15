using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A class used to store ragdoll body parts informations
    /// </summary>
    public class RagdollBodyPart
    {
        public Transform BodyPartTransform;
        public Vector3 StoredPosition;
        public Quaternion StoredRotation;
    }

    /// <summary>
    /// Use this class to pilot a ragdoll on a character that is usually driven by an animator and have it fall elegantly
    /// If you have parts of your ragdoll that you don't want to be affected by this script (a weapon for example), just add a MMRagdollerIgnore component to them
    /// </summary>
    public class MMRagdoller : MonoBehaviour
    {
        /// <summary>
        /// The possible states of the ragdoll : 
        /// - animated : driven by an animator controller, rigidbodies asleep
        /// - ragdolling : full ragdoll mode, purely physics driven
        /// - blending : transitioning between ragdolling and animated
        /// </summary>
        public enum RagdollStates
        {
            Animated,
            Ragdolling,
            Blending
        }

        [Header("Ragdoll")]
        /// the current state of the ragdoll
        public RagdollStates CurrentState = RagdollStates.Animated;
        /// the duration in seconds it takes to blend from Ragdolling to Animated
        public float RagdollToMecanimBlendDuration = 0.5f;

        [Header("Rigidbodies")]
        /// The rigidbody attached to the main body part of the ragdoll (usually the Pelvis) 
        public Rigidbody MainRigidbody;
        /// if this is true, all rigidbodies will be forced to sleep every frame
        public bool ForceSleep = true;

        protected float _mecanimToGetUpTransitionTime = 0.05f;
        protected float _ragdollingEndTimestamp = -100f;
        protected Vector3 _ragdolledHipPosition;
        protected Vector3 _ragdolledHeadPosition;
        protected Vector3 _ragdolledFeetPosition;
        protected List<RagdollBodyPart> _bodyparts = new List<RagdollBodyPart>();
        protected Animator _animator;
        protected List<Component> _rigidbodiesTempList;
        protected Component[] _rigidbodies;
        protected List<int> _animatorParameters;

        protected const string _getUpFromBackAnimationParameterName = "GetUpFromBack";
        protected int _getUpFromBackAnimationParameter;
        protected const string _getUpFromBellyAnimationParameterName = "GetUpFromBelly";
        protected int _getUpFromBellyAnimationParameter;

        /// <summary>
        /// Use this to get the current state of the ragdoll or to set a new one
        /// </summary>
        public bool Ragdolling
        {
            get
            {
                // if we're not animated, we're ragdolling
                return CurrentState != RagdollStates.Animated;
            }
            set
            {
                if (value == true)
                {
                    // if we're 
                    if (CurrentState == RagdollStates.Animated)
                    {
                        SetIsKinematic(false);
                        _animator.enabled = false;
                        CurrentState = RagdollStates.Ragdolling;
                        MMAnimatorExtensions.UpdateAnimatorBool(_animator, _getUpFromBackAnimationParameter, false, _animatorParameters);
                        MMAnimatorExtensions.UpdateAnimatorBool(_animator, _getUpFromBellyAnimationParameter, false, _animatorParameters);
                    }
                }
                else
                {
                    if (CurrentState == RagdollStates.Ragdolling)
                    {
                        SetIsKinematic(true);
                        _ragdollingEndTimestamp = Time.time;
                        _animator.enabled = true;
                        CurrentState = RagdollStates.Blending;

                        foreach (RagdollBodyPart bodypart in _bodyparts)
                        {
                            bodypart.StoredRotation = bodypart.BodyPartTransform.rotation;
                            bodypart.StoredPosition = bodypart.BodyPartTransform.position;
                        }

                        _ragdolledFeetPosition = 0.5f * (_animator.GetBoneTransform(HumanBodyBones.LeftToes).position + _animator.GetBoneTransform(HumanBodyBones.RightToes).position);
                        _ragdolledHeadPosition = _animator.GetBoneTransform(HumanBodyBones.Head).position;
                        _ragdolledHipPosition = _animator.GetBoneTransform(HumanBodyBones.Hips).position;

                        if (_animator.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0)
                        {
                            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _getUpFromBackAnimationParameter, true, _animatorParameters);
                        }
                        else
                        {
                            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _getUpFromBellyAnimationParameter, true, _animatorParameters);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// On start we initialize our ragdoller
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Grabs rigidbodies, adds body parts and stores the animator
        /// </summary>
        protected virtual void Initialization()
        {
            // we grab all rigidbodies and set them to kinematic
            _rigidbodies = GetComponentsInChildren(typeof(Rigidbody));

            _rigidbodiesTempList = new List<Component>();
            foreach (Component rigidbody in _rigidbodies)
            {
                if (rigidbody.gameObject.MMGetComponentNoAlloc<MMRagdollerIgnore>() == null)
                {
                    _rigidbodiesTempList.Add(rigidbody);
                }
            }

            _rigidbodies = null;
            _rigidbodies = _rigidbodiesTempList.ToArray();


            if (CurrentState == RagdollStates.Animated)
            {
                SetIsKinematic(true);
            }
            else
            {
                SetIsKinematic(false);
            }

            // we grab all transforms and add a RagdollBodyPart to them
            Component[] transforms = GetComponentsInChildren(typeof(Transform));
            foreach (Component component in transforms)
            {
                if (component.transform != this.transform)
                {
                    RagdollBodyPart bodyPart = new RagdollBodyPart { BodyPartTransform = component as Transform };
                    _bodyparts.Add(bodyPart);
                }
            }

            // we store our animator
            _animator = GetComponent<Animator>();
            RegisterAnimatorParameters();
        }

        /// <summary>
        /// Registers our animation parameters
        /// </summary>
        protected virtual void RegisterAnimatorParameters()
        {
            _animatorParameters = new List<int>();

            _getUpFromBackAnimationParameter = Animator.StringToHash(_getUpFromBackAnimationParameterName);
            _getUpFromBellyAnimationParameter = Animator.StringToHash(_getUpFromBellyAnimationParameterName);

            if (_animator == null)
            {
                return;
            }
            if (_animator.MMHasParameterOfType(_getUpFromBackAnimationParameterName, AnimatorControllerParameterType.Bool))
            {
                _animatorParameters.Add(_getUpFromBackAnimationParameter);
            }
            if (_animator.MMHasParameterOfType(_getUpFromBellyAnimationParameterName, AnimatorControllerParameterType.Bool))
            {
                _animatorParameters.Add(_getUpFromBellyAnimationParameter);
            }
        }

        /// <summary>
        /// Sets all rigidbodies in the ragdoll to kinematic and stops them from detecting collisions (or the other way around)
        /// </summary>
        /// <param name="isKinematic"></param>
        protected virtual void SetIsKinematic(bool isKinematic)
        {
            foreach (Component rigidbody in _rigidbodies)
            {
                if (rigidbody.transform != this.transform)
                {
                    (rigidbody as Rigidbody).detectCollisions = !isKinematic;
                    (rigidbody as Rigidbody).isKinematic = isKinematic;
                }
            }
        }

        /// <summary>
        /// Forces all rigidbodies in the ragdoll to sleep
        /// </summary>
        public virtual void ForceRigidbodiesToSleep()
        {
            foreach (Component rigidbody in _rigidbodies)
            {
                if (rigidbody.transform != this.transform)
                {
                    (rigidbody as Rigidbody).Sleep();
                }
            }
        }

        /// <summary>
        /// On late update, we force our ragdoll elements to sleep and handle blending
        /// </summary>
        protected virtual void LateUpdate()
        {
            if ((CurrentState == RagdollStates.Animated) && ForceSleep)
            {
                ForceRigidbodiesToSleep();
            }

            HandleBlending();
        }

        /// <summary>
        /// Blends between ragdolling and animated and switches to Animated at the end
        /// </summary>
        protected virtual void HandleBlending()
        {
            if (CurrentState == RagdollStates.Blending)
            {
                if (Time.time <= _ragdollingEndTimestamp + _mecanimToGetUpTransitionTime)
                {
                    Vector3 animatedToRagdolling = _ragdolledHipPosition - _animator.GetBoneTransform(HumanBodyBones.Hips).position;
                    Vector3 newRootPosition = transform.position + animatedToRagdolling;

                    RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition, Vector3.down));
                    newRootPosition.y = 0;
                    foreach (RaycastHit hit in hits)
                    {
                        if (!hit.transform.IsChildOf(transform))
                        {
                            newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
                        }
                    }
                    transform.position = newRootPosition;

                    Vector3 ragdollingDirection = _ragdolledHeadPosition - _ragdolledFeetPosition;
                    ragdollingDirection.y = 0;

                    Vector3 meanFeetPosition = 0.5f * (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                    Vector3 animatedDirection = _animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                    animatedDirection.y = 0;

                    transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdollingDirection.normalized);
                }
                float ragdollBlendAmount = 1.0f - (Time.time - _ragdollingEndTimestamp - _mecanimToGetUpTransitionTime) / RagdollToMecanimBlendDuration;
                ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);

                foreach (RagdollBodyPart bodypart in _bodyparts)
                {
                    if (bodypart.BodyPartTransform != transform)
                    {
                        if (bodypart.BodyPartTransform == _animator.GetBoneTransform(HumanBodyBones.Hips))
                        {
                            bodypart.BodyPartTransform.position = Vector3.Lerp(bodypart.BodyPartTransform.position, bodypart.StoredPosition, ragdollBlendAmount);
                        }
                        bodypart.BodyPartTransform.rotation = Quaternion.Slerp(bodypart.BodyPartTransform.rotation, bodypart.StoredRotation, ragdollBlendAmount);
                    }
                }

                if (ragdollBlendAmount == 0)
                {
                    CurrentState = RagdollStates.Animated;
                    return;
                }
            }
        }
    }
}
