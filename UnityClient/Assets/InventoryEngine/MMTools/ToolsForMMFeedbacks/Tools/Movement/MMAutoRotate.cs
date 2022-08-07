using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this class to a GameObject to make it rotate on itself
    /// </summary>
    public class MMAutoRotate : MonoBehaviour
    {
        public enum UpdateModes { Update, LateUpdate, FixedUpdate }

        [Header("Rotation")]
        /// whether or not this object should be rotating right now
        public bool Rotating = true;
        [MMCondition("Rotating", true)]
        /// the space to apply the rotation in
        public Space RotationSpace = Space.Self;
        /// whether movement should happen at Update, FixedUpdate or LateUpdate
        public UpdateModes UpdateMode = UpdateModes.Update;
        [MMCondition("Rotating", true)]
        /// The rotation speed. Positive means clockwise, negative means counter clockwise.
        public Vector3 RotationSpeed = new Vector3(100f, 0f, 0f);

        [Header("Orbit")]
        /// if this is true, the object will also move around a pivot (only the position is affected, not the rotation)
        public bool Orbiting = false;
        [MMCondition("Orbiting", true)]
        /// if this is true, the orbit plane will rotate along with the parent
        public bool AdditiveOrbitRotation = false;
        /// the pivot to rotate around (if left blank, will be the object itself
        [MMCondition("Orbiting", true)]
        public Transform OrbitCenterTransform;
        /// the pivot (relative to the object's position in local space) to rotate around
        [MMCondition("Orbiting", true)]
        public Vector3 OrbitCenterOffset = Vector3.zero;
        /// the axis around which the object should rotate (don't make it zero)
        [MMCondition("Orbiting", true)]
        public Vector3 OrbitRotationAxis = new Vector3(0f, 1f, 0f);
        /// the speed at which to rotate
        [MMCondition("Orbiting", true)]
        public float OrbitRotationSpeed = 10f;
        /// the radius at which to orbit
        [MMCondition("Orbiting", true)]
        public float OrbitRadius = 3f;
        /// the speed at which the object catches up when orbit radius or axis changes
        [MMCondition("Orbiting", true)]
        public float OrbitCorrectionSpeed = 10f;

        [Header("Settings")]
        /// if this is true, will draw gizmos to show the plane, orbit and direction
        public bool DrawGizmos = true;
        [MMCondition("DrawGizmos", true)]
        /// the color of the orbit disc
        public Color OrbitPlaneColor = new Color(54f, 169f, 225f, 0.02f);
        [MMCondition("DrawGizmos", true)]
        /// the color of the orbit line
        public Color OrbitLineColor = new Color(225f, 225f, 225f, 0.1f);
        
        [HideInInspector]
        public Vector3 _orbitCenter;
        [HideInInspector]
        public Vector3 _worldRotationAxis;
        [HideInInspector]
        public Plane _rotationPlane;
        [HideInInspector]
        public Vector3 _snappedPosition;
        [HideInInspector]
        public Vector3 _radius;

        protected Quaternion _newRotation;
        protected Vector3 _desiredOrbitPosition;
        private Vector3 _previousPosition;

        /// <summary>
        /// On start, we initialize our plane
        /// </summary>
        protected virtual void Start()
        {
            _rotationPlane = new Plane();
        }

        /// <summary>
        /// Makes the object rotate on its center at Update 
        /// </summary>
        protected virtual void Update()
        {
            if (UpdateMode == UpdateModes.Update)
            {
                Rotate();
            }
        }
        
        /// <summary>
         /// Makes the object rotate on its center at FixedUpdate
         /// </summary>
        protected virtual void FixedUpdate()
        {
            if (UpdateMode == UpdateModes.FixedUpdate)
            {
                Rotate();
            }
        }
        
        /// <summary>
         /// Makes the object rotate on its center at LateUpdate
         /// </summary>
        protected virtual void LateUpdate()
        {
            if (UpdateMode == UpdateModes.LateUpdate)
            {
                Rotate();
            }
        }

        /// <summary>
        /// Rotates the object
        /// </summary>
        protected virtual void Rotate()
        {
            if (Rotating)
            {
                transform.Rotate(RotationSpeed * Time.deltaTime, RotationSpace);
            }

            if (Orbiting)
            {
                _orbitCenter = OrbitCenterTransform.transform.position + OrbitCenterOffset;
                if (AdditiveOrbitRotation)
                {
                    _worldRotationAxis = OrbitCenterTransform.TransformDirection(OrbitRotationAxis);
                }
                else
                {
                    _worldRotationAxis = OrbitRotationAxis;
                }
                _rotationPlane.SetNormalAndPosition(_worldRotationAxis.normalized, _orbitCenter);
                _snappedPosition = _rotationPlane.ClosestPointOnPlane(this.transform.position);
                _radius = OrbitRadius * Vector3.Normalize(_snappedPosition - _orbitCenter);
                _newRotation = Quaternion.AngleAxis(OrbitRotationSpeed * Time.deltaTime, _worldRotationAxis);
                _desiredOrbitPosition = _orbitCenter + _newRotation * _radius;
                this.transform.position = Vector3.Lerp(this.transform.position, _desiredOrbitPosition, OrbitCorrectionSpeed * Time.deltaTime);
                _previousPosition = _desiredOrbitPosition;
            }
        }
    }
}