using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [Serializable]
    public class MMAim 
    {
        /// the list of possible control modes
        public enum AimControls { Off, PrimaryMovement, SecondaryMovement, Mouse, Script }
        /// the list of possible rotation modes
        public enum RotationModes { Free, Strict4Directions, Strict8Directions }

        [Header("Control Mode")]
        [MMInformation("Pick a control mode : mouse (aims towards the pointer), primary movement (you'll aim towards the current input direction), or secondary movement (aims " +
            "towards a second input axis, think twin stick shooters), and set minimum and maximum angles.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// the aim control mode
        public AimControls AimControl = AimControls.SecondaryMovement;
        /// the rotation mode
		public RotationModes RotationMode = RotationModes.Free;

        [Header("Limits")]
        [Range(-180, 180)]
        /// the minimum angle at which the weapon's rotation will be clamped
        public float MinimumAngle = -180f;
        [Range(-180, 180)]
        /// the maximum angle at which the weapon's rotation will be clamped
        public float MaximumAngle = 180f;


        /// the current angle the weapon is aiming at
        public float CurrentAngle { get; protected set; }

        public Vector3 CurrentPosition { get; set; }
        public Vector2 PrimaryMovement { get; set; }
        public Vector2 SecondaryMovement { get; set; }

        protected float[] _possibleAngleValues;
        protected Vector3 _currentAim = Vector3.zero;
        protected Vector3 _direction;
        protected Vector3 _mousePosition;

        protected Camera _mainCamera;

        /// <summary>
		/// Grabs the weapon component, initializes the angle values
		/// </summary>
		public virtual void Initialization()
        {
            if (RotationMode == RotationModes.Strict4Directions)
            {
                _possibleAngleValues = new float[5];
                _possibleAngleValues[0] = -180f;
                _possibleAngleValues[1] = -90f;
                _possibleAngleValues[2] = 0f;
                _possibleAngleValues[3] = 90f;
                _possibleAngleValues[4] = 180f;
            }
            if (RotationMode == RotationModes.Strict8Directions)
            {
                _possibleAngleValues = new float[9];
                _possibleAngleValues[0] = -180f;
                _possibleAngleValues[1] = -135f;
                _possibleAngleValues[2] = -90f;
                _possibleAngleValues[3] = -45f;
                _possibleAngleValues[4] = 0f;
                _possibleAngleValues[5] = 45f;
                _possibleAngleValues[6] = 90f;
                _possibleAngleValues[7] = 135f;
                _possibleAngleValues[8] = 180f;
            }

            _mainCamera = Camera.main;
        }

        /// <summary>
		/// Computes the current aim direction
		/// </summary>
		public virtual Vector2 GetCurrentAim()
        {
            switch (AimControl)
            {
                case AimControls.Off:
                    _currentAim = Vector2.zero;
                    break;

                case AimControls.Script:

                    break;

                case AimControls.PrimaryMovement:                    
                    _currentAim = PrimaryMovement;
                    break;

                case AimControls.SecondaryMovement:
                    _currentAim = SecondaryMovement;
                    break;

                case AimControls.Mouse:
                    _mousePosition = Input.mousePosition;
                    _mousePosition.z = 10;
                    _direction = _mainCamera.ScreenToWorldPoint(_mousePosition);
                    _direction.z = CurrentPosition.z;
                    _currentAim = _direction - CurrentPosition;
                    break;

                default:
                    _currentAim = Vector2.zero;
                    break;
            }

            float currentAngle = Mathf.Atan2(_currentAim.y, _currentAim.x) * Mathf.Rad2Deg;
            if (RotationMode == RotationModes.Strict4Directions || RotationMode == RotationModes.Strict8Directions)
            {
                currentAngle = MMMaths.RoundToClosest(currentAngle, _possibleAngleValues);
            }
            CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);

            _currentAim = (_currentAim.magnitude == 0f) ? Vector2.zero : MMMaths.RotateVector2(Vector2.right, currentAngle);

            return _currentAim;
        }
    }
}