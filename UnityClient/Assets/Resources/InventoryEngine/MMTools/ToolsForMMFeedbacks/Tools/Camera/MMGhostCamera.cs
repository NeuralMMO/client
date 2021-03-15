using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools 
{
    /// <summary>
    /// Add this class to a camera and you'll be able to pilot it using the horizontal/vertical axis, and up/down controls set via its inspector. 
    /// It's got an activation button, a run button, and an option to slow down time (this will require a MMTimeManager present in the scene)
    /// </summary>
    public class MMGhostCamera : MonoBehaviour
    {
        [Header("Speed")]
        /// the camera's movement speed
        public float MovementSpeed = 10f;
        /// the factor by which to multiply the speed when "running"
        public float RunFactor = 4f;
        /// the movement's acceleration
        public float Acceleration = 5f;
        /// the movement's deceleration
        public float Deceleration = 5f;
        /// the speed at which the camera rotates
        public float RotationSpeed = 40f;

        [Header("Controls")]
        /// the button used to toggle the camera on/off
        public KeyCode ActivateButton = KeyCode.RightShift;
        /// the name of the InputManager's horizontal axis
        public string HorizontalAxisName = "Horizontal";
        /// the name of the InputManager's vertical axis
        public string VerticalAxisName = "Vertical";
        /// the button to use to go up
        public KeyCode UpButton = KeyCode.Space;
        /// the button to use to go down
        public KeyCode DownButton = KeyCode.C;
        /// the button to use to switch between mobile and desktop control mode
        public KeyCode ControlsModeSwitch = KeyCode.M;
        /// the button used to modify the timescale
        public KeyCode TimescaleModificationButton = KeyCode.F;
        /// the button used to run while it's pressed
        public KeyCode RunButton = KeyCode.LeftShift;
        /// the mouse's sensitivity
        public float MouseSensitivity = 0.02f;
        /// the right stick sensitivity
        public float MobileStickSensitivity = 2f;

        [Header("Timescale Modification")]
        /// the amount to modify the timescale by when pressing the timescale button
        public float TimescaleModifier = 0.5f;


        [Header("Settings")]
        /// whether or not this camera should activate on start
        public bool AutoActivation = true;
        /// whether or not movement (up/down/left/right/forward/backward) is enabled
        public bool MovementEnabled = true;
        // whether or not rotation is enabled
        public bool RotationEnabled = true;
        [MMReadOnly]
        /// whether this camera is active or not right now
        public bool Active = false;
        [MMReadOnly]
        /// whether time is being altered right now or not
        public bool TimeAltered = false;

        [Header("Virtual Joysticks")]
        public bool UseMobileControls;
        [MMCondition("UseMobileControls", true)]
        public GameObject LeftStickContainer;
        [MMCondition("UseMobileControls", true)]
        public GameObject RightStickContainer;
        [MMCondition("UseMobileControls", true)]
        public MMTouchJoystick LeftStick;
        [MMCondition("UseMobileControls", true)]
        public MMTouchJoystick RightStick;

        protected Vector3 _currentInput;
        protected Vector3 _lerpedInput;
        protected Vector3 _normalizedInput;
        protected float _acceleration;
        protected float _deceleration;
        protected Vector3 _movementVector = Vector3.zero;
        protected float _speedMultiplier;
        protected Vector3 _newEulerAngles;

        /// <summary>
        /// On start, activate our camera if needed
        /// </summary>
        protected virtual void Start()
        {
            if (AutoActivation)
            {
                ToggleFreeCamera();
            }
        }

        /// <summary>
        /// On Update we grab our input and move accordingly
        /// </summary>
        protected virtual void Update()
        {
            if (Input.GetKeyDown(ActivateButton))
            {
                ToggleFreeCamera();
            }

            if (!Active)
            {
                return;
            }

            GetInput();
            Translate();
            Rotate();
            Move();

            HandleMobileControls();
        }

        /// <summary>
        /// Grabs and stores the various input values
        /// </summary>
        protected virtual void GetInput()
        {
            if (!UseMobileControls || (LeftStick == null))
            {
                _currentInput.x = Input.GetAxis("Horizontal");
                _currentInput.y = 0f;
                _currentInput.z = Input.GetAxis("Vertical");
            }
            else
            {
                _currentInput.x = LeftStick._joystickValue.x;
                _currentInput.y = 0f;
                _currentInput.z = LeftStick._joystickValue.y;
            }

            if (Input.GetKey(UpButton))
            {
                _currentInput.y = 1f; 
            }
            if (Input.GetKey(DownButton))
            {
                _currentInput.y = -1f;
            }

            _speedMultiplier = Input.GetKey(RunButton) ? RunFactor : 1f;
            _normalizedInput = _currentInput.normalized;
            
            if (Input.GetKeyDown(TimescaleModificationButton))
            {
                ToggleSlowMotion();
            }
        }

        /// <summary>
        /// Turns controls to mobile if needed
        /// </summary>
        protected virtual void HandleMobileControls()
        {
            if (Input.GetKeyDown(ControlsModeSwitch))
            {
                UseMobileControls = !UseMobileControls;
            }
            if (UseMobileControls)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Active)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (LeftStickContainer != null)
            {
                LeftStickContainer?.SetActive(UseMobileControls);
            }
            if (RightStickContainer != null)
            {
                RightStickContainer?.SetActive(UseMobileControls);
            }
        }

        /// <summary>
        /// Computes the new position
        /// </summary>
        protected virtual void Translate()
        {
            if (!MovementEnabled)
            {
                return;
            }

            if ((Acceleration == 0) || (Deceleration == 0))
            {
                _lerpedInput = _currentInput;
            }
            else
            {
                if (_normalizedInput.magnitude == 0)
                {
                    _acceleration = Mathf.Lerp(_acceleration, 0f, Deceleration * Time.deltaTime);
                    _lerpedInput = Vector3.Lerp(_lerpedInput, _lerpedInput * _acceleration, Time.deltaTime * Deceleration);
                }
                else
                {
                    _acceleration = Mathf.Lerp(_acceleration, 1f, Acceleration * Time.deltaTime);
                    _lerpedInput = Vector3.ClampMagnitude(_normalizedInput, _acceleration);
                }
            }

            _movementVector = _lerpedInput;
            _movementVector *= MovementSpeed * _speedMultiplier;

            if (_movementVector.magnitude > MovementSpeed * _speedMultiplier)
            {
                _movementVector = Vector3.ClampMagnitude(_movementVector, MovementSpeed * _speedMultiplier);
            }
        }

        /// <summary>
        /// Computes the new rotation
        /// </summary>
        protected virtual void Rotate()
        {
            if (!RotationEnabled)
            {
                return;
            }
            _newEulerAngles = this.transform.eulerAngles;

            if (!UseMobileControls || (LeftStick == null))
            {
                _newEulerAngles.x += -Input.GetAxis("Mouse Y") * 359f * MouseSensitivity;
                _newEulerAngles.y += Input.GetAxis("Mouse X") * 359f * MouseSensitivity;
            }
            else
            {
                _newEulerAngles.x += -RightStick._joystickValue.y * MobileStickSensitivity;
                _newEulerAngles.y += RightStick._joystickValue.x * MobileStickSensitivity;
            }                

            _newEulerAngles = Vector3.Lerp(this.transform.eulerAngles, _newEulerAngles, Time.deltaTime * RotationSpeed);
        }

        /// <summary>
        /// Modifies the camera's transform's position and rotation
        /// </summary>
        protected virtual void Move()
        {
            transform.eulerAngles = _newEulerAngles;
            transform.position += transform.rotation * _movementVector * Time.deltaTime;
        }

        /// <summary>
        /// Toggles the timescale modification
        /// </summary>
        protected virtual void ToggleSlowMotion()
        {
            TimeAltered = !TimeAltered;
            if (TimeAltered)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimescaleModifier, 1f, true, 5f, true);
            }
            else
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);

            }
        }

        /// <summary>
        /// Toggles the camera's active state
        /// </summary>
        protected virtual void ToggleFreeCamera()
        {
            Active = !Active;
            Cursor.lockState = Active ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !Active;
        }
    }
}