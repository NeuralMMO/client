using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// This component will automatically update scale and rotation 
    /// Put it one level below the top, and have the model one level below that
    /// Hierarchy should be as follows :
    /// 
    /// Parent (where the logic (and optionnally rigidbody lies)
    /// - MMSquashAndStretch
    /// - - Model / sprite
    /// 
    /// Make sure this intermediary layer only has one child
    /// If movement feels glitchy make sure your rigidbody is on Interpolate
    /// </summary>
    public class MMSquashAndStretch : MonoBehaviour
    {
        public enum Timescales { Regular, Unscaled }
        public enum Modes { Rigidbody, Rigidbody2D, Position }

        [MMInformation("This component will apply squash and stretch based on velocity (either position based or computed from a Rigidbody. It has to be put on an intermediary level in the hierarchy, between the logic (top level) and the model (bottom level).", MMInformationAttribute.InformationType.Info, false)]
        [Header("Velocity Detection")]
        /// the possible ways to get velocity from
        public Modes Mode = Modes.Position;
        /// whether we should use deltaTime or unscaledDeltaTime;
        public Timescales Timescale = Timescales.Regular;


        [Header("Settings")]
        /// the intensity of the squash and stretch
        public float Intensity = 0.02f;
        /// the maximum velocity of your parent object, used to remap the computed one
        public float MaximumVelocity = 1f;

        [Header("Rescale")]
        /// the minimum scale to apply to this object
        public Vector2 MinimumScale = new Vector2(0.5f, 0.5f);
        /// the maximum scale to apply to this object
        public Vector2 MaximumScale = new Vector2(2f, 2f);

        [Header("Squash")]
        /// if this is true, the object will squash once velocity goes below the specified threshold
        public bool AutoSquashOnStop = false;
        /// the curve to apply when squashing the object (this describes scale on x and z, will be inverted for y to maintain mass)
        public AnimationCurve SquashCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
        /// the velocity threshold after which a squash can be triggered if the object stops
        public float SquashVelocityThreshold = 0.1f;
        /// the maximum duration of the squash (will be reduced if velocity is low)
        [MMVector("Min","Max")]
        public Vector2 SquashDuration = new Vector2(0.25f, 0.5f);
        /// the maximum intensity of the squash
        [MMVector("Min", "Max")]
        public Vector2 SquashIntensity = new Vector2(0f, 1f);
        
        [Header("Debug")]
        [MMReadOnly]
        /// the current velocity of the parent object
        public Vector3 Velocity;
        [MMReadOnly]
        /// the remapped velocity
        public float RemappedVelocity;
        [MMReadOnly]
        /// the current velocity magnitude
        public float VelocityMagnitude;

        public float TimescaleTime { get { return (Timescale == Timescales.Regular) ? Time.time : Time.unscaledTime; } }
        public float TimescaleDeltaTime { get { return (Timescale == Timescales.Regular) ? Time.deltaTime : Time.unscaledDeltaTime; } }

        protected Rigidbody2D _rigidbody2D;
        protected Rigidbody _rigidbody;
        protected Transform _childTransform;
        protected Transform _parentTransform;
        protected Vector3 _direction;
        protected Vector3 _previousPosition;
        protected Vector3 _newLocalScale;
        protected Vector3 _initialScale;
        protected Quaternion _newRotation = Quaternion.identity;
        protected Quaternion _deltaRotation;
        protected float _squashStartedAt = 0f;
        protected bool _squashing = false;
        protected float _squashIntensity;
        protected float _squashDuration;

        protected bool _movementStarted = false;
        protected float _lastVelocity = 0f;


        /// <summary>
        /// On start, we initialize our component
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Stores the initial scale, grabs the rigidbodies (or tries to), as well as the parent and child
        /// </summary>
        protected virtual void Initialization()
        {
            _initialScale = this.transform.localScale;

            _rigidbody = this.transform.parent.GetComponent<Rigidbody>();
            _rigidbody2D = this.transform.parent.GetComponent<Rigidbody2D>();

            _childTransform = this.transform.GetChild(0).transform;
            _parentTransform = this.transform.parent.GetComponent<Transform>();

            _previousPosition = _parentTransform.position;
        }
        
        /// <summary>
        /// On late update, we apply our squash and stretch effect
        /// </summary>
        protected virtual void LateUpdate()
        {
            SquashAndStretch();
        }

        /// <summary>
        /// Computes velocity and applies the effect
        /// </summary>
        protected virtual void SquashAndStretch()
        {
            if (TimescaleDeltaTime <= 0f)
            {
                return;
            }

            ComputeVelocityAndDirection();
            ComputeNewRotation();
            ComputeNewLocalScale();
            StorePreviousPosition();
        }

        /// <summary>
        /// Determines the current velocity and direction of the parent object
        /// </summary>
        protected virtual void ComputeVelocityAndDirection()
        {
            Velocity = Vector3.zero;

            switch (Mode)
            {
                case Modes.Rigidbody:
                    Velocity = _rigidbody.velocity;
                    break;

                case Modes.Rigidbody2D:
                    Velocity = _rigidbody2D.velocity;
                    break;

                case Modes.Position:
                    Velocity = (_previousPosition - _parentTransform.position) / TimescaleDeltaTime;
                    break;
            }

            VelocityMagnitude = Velocity.magnitude;
            RemappedVelocity = MMMaths.Remap(VelocityMagnitude, 0f, MaximumVelocity, 0f, 1f);
            _direction = Vector3.Normalize(Velocity);

            if (AutoSquashOnStop)
            {
                // if we've moved fast enough and have now stopped, we trigger a squash
                if (VelocityMagnitude > SquashVelocityThreshold)
                {
                    _movementStarted = true;
                    _lastVelocity = Mathf.Clamp(VelocityMagnitude, 0f, MaximumVelocity);
                }
                else if (_movementStarted)
                {
                    _movementStarted = false;
                    _squashing = true;
                    float duration = MMMaths.Remap(_lastVelocity, 0f, MaximumVelocity, SquashDuration.x, SquashDuration.y);
                    float intensity = MMMaths.Remap(_lastVelocity, 0f, MaximumVelocity, SquashIntensity.x, SquashIntensity.y);
                    Squash(duration, intensity);
                }
            }            
        }

        /// <summary>
        /// Computes a new rotation for both this object and the child
        /// </summary>
        protected virtual void ComputeNewRotation()
        {
            if (VelocityMagnitude > 0.01f)
            {
                _newRotation = Quaternion.FromToRotation(Vector3.up, _direction);
            }
            _deltaRotation = _parentTransform.rotation;
            this.transform.rotation = _newRotation;
            _childTransform.rotation = _deltaRotation;
        }

        /// <summary>
        /// Computes a new local scale for this object
        /// </summary>
        protected virtual void ComputeNewLocalScale()
        {
            if (_squashing)
            {
                float elapsed = MMMaths.Remap(TimescaleTime - _squashStartedAt, 0f, _squashDuration, 0f, 1f);
                _newLocalScale.x = _initialScale.x + SquashCurve.Evaluate(elapsed) * _squashIntensity;
                _newLocalScale.y = _initialScale.y - SquashCurve.Evaluate(elapsed) * _squashIntensity;
                _newLocalScale.z = _initialScale.z + SquashCurve.Evaluate(elapsed) * _squashIntensity;

                if (elapsed >= 1f)
                {
                    _squashing = false;
                }
            }
            else
            {
                _newLocalScale.x = Mathf.Clamp01(1f / (RemappedVelocity + 0.001f));
                _newLocalScale.y = RemappedVelocity;
                _newLocalScale.z = Mathf.Clamp01(1f / (RemappedVelocity + 0.001f));
                _newLocalScale = Vector3.Lerp(Vector3.one, _newLocalScale, VelocityMagnitude * Intensity);
            }            

            _newLocalScale.x = Mathf.Clamp(_newLocalScale.x, MinimumScale.x, MaximumScale.x);
            _newLocalScale.y = Mathf.Clamp(_newLocalScale.y, MinimumScale.y, MaximumScale.y);

            this.transform.localScale = _newLocalScale;
        }

        /// <summary>
        /// Stores the previous position of the parent to compute velocity
        /// </summary>
        protected virtual void StorePreviousPosition()
        {
            _previousPosition = _parentTransform.position;
        }
        
        /// <summary>
        /// Triggered either directly or via the AutoSquash setting, this squashes the object (usually after a contact / stop)
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="intensity"></param>
        public virtual void Squash(float duration, float intensity)
        {
            _squashStartedAt = TimescaleTime;
            _squashing = true;
            _squashIntensity = intensity;
            _squashDuration = duration;
        }
    }
}
