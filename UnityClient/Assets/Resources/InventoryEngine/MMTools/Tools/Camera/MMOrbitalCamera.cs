using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    [RequireComponent(typeof(Camera))]
    public class MMOrbitalCamera : MonoBehaviour
    {
        public enum Modes { Mouse, Touch }
        [Header("Setup")]
        public Modes Mode = Modes.Touch;
        public Transform Target;
        public Vector3 TargetOffset;
        [MMReadOnly]
        public float DistanceToTarget = 5f;

        [Header("Rotation")]
        public bool RotationEnabled = true;
        public Vector2 RotationSpeed = new Vector2(200f, 200f);
        public int MinVerticalAngleLimit = -80;
        public int MaxVerticalAngleLimit = 80;

        [Header("Zoom")]
        public bool ZoomEnabled = true;
        public float MinimumZoomDistance = 0.6f;
        public float MaximumZoomDistance = 20;     
        public int ZoomSpeed = 40;
        public float ZoomDampening = 5f;

        [Header("Mouse Zoom")]
        public float MouseWheelSpeed = 10f;
        public float MaxMouseWheelClamp = 10f;

        protected float _angleX = 0f;
        protected float _angleY = 0f;
        protected float _currentDistance;
        protected float _desiredDistance;
        protected Quaternion _currentRotation;
        protected Quaternion _desiredRotation;
        protected Quaternion _rotation;
        protected Vector3 _position;
        protected float _scrollWheelAmount = 0;

        protected virtual void Start()
        {
            Initialization();
        }

        public virtual void Initialization()
        {
            // if no target is set, we throw an error and exit
            if (Target == null)
            {
                Debug.LogError(this.gameObject.name + " : the MMOrbitalCamera doesn't have a target.");
                return;
            }

            DistanceToTarget = Vector3.Distance(Target.position, transform.position);
            _currentDistance = DistanceToTarget;
            _desiredDistance = DistanceToTarget;

            _position = transform.position;
            _rotation = transform.rotation;
            _currentRotation = transform.rotation;
            _desiredRotation = transform.rotation;

            _angleX = Vector3.Angle(Vector3.right, transform.right);
            _angleY = Vector3.Angle(Vector3.up, transform.up);
        }

        protected virtual void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            Rotation();
            Zoom();
            ApplyMovement();
        }

        protected virtual void Rotation()
        {
            if (!RotationEnabled)
            {
                return;
            }

            if (Mode == Modes.Touch && (Input.touchCount > 0))
            {
                if ((Input.touches[0].phase == TouchPhase.Moved) && (Input.touchCount == 1))
                {
                    float swipeSpeed = Input.touches[0].deltaPosition.magnitude / Input.touches[0].deltaTime;

                    _angleX += Input.touches[0].deltaPosition.x * RotationSpeed.x * Time.deltaTime * swipeSpeed * 0.00001f;
                    _angleY -= Input.touches[0].deltaPosition.y * RotationSpeed.y * Time.deltaTime * swipeSpeed * 0.00001f;

                    _angleY = MMMaths.ClampAngle(_angleY, MinVerticalAngleLimit, MaxVerticalAngleLimit);
                    _desiredRotation = Quaternion.Euler(_angleY, _angleX, 0);
                    _currentRotation = transform.rotation;

                    _rotation = Quaternion.Lerp(_currentRotation, _desiredRotation, Time.deltaTime * ZoomDampening);
                    transform.rotation = _rotation;
                }
                else if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)
                {
                    _desiredRotation = transform.rotation;
                }

                if (transform.rotation != _desiredRotation)
                {
                    _rotation = Quaternion.Lerp(transform.rotation, _desiredRotation, Time.deltaTime * ZoomDampening);
                    transform.rotation = _rotation;
                }
            }
            else if (Mode == Modes.Mouse)
            {
                _angleX += Input.GetAxis("Mouse X") * RotationSpeed.x * Time.deltaTime;
                _angleY += -Input.GetAxis("Mouse Y") * RotationSpeed.y * Time.deltaTime;
                _angleY = Mathf.Clamp(_angleY, MinVerticalAngleLimit, MaxVerticalAngleLimit);

                _desiredRotation = Quaternion.Euler(new Vector3(_angleY, _angleX, 0));
                _currentRotation = transform.rotation;
                _rotation = Quaternion.Lerp(_currentRotation, _desiredRotation, Time.deltaTime * ZoomDampening);
                transform.rotation = _rotation;
            }            
        }

        protected virtual void Zoom()
        {
            if (!ZoomEnabled)
            {
                return;
            }

            if (Mode == Modes.Touch && (Input.touchCount > 0))
            {
                if (Input.touchCount == 2)
                {
                    Touch firstTouch = Input.GetTouch(0);
                    Touch secondTouch = Input.GetTouch(1);

                    Vector2 firstTouchPreviousPosition = firstTouch.position - firstTouch.deltaPosition;
                    Vector2 secondTouchPreviousPosition = secondTouch.position - secondTouch.deltaPosition;

                    float previousTouchDeltaMagnitude = (firstTouchPreviousPosition - secondTouchPreviousPosition).magnitude;
                    float thisTouchDeltaMagnitude = (firstTouch.position - secondTouch.position).magnitude;
                    float deltaMagnitudeDifference = previousTouchDeltaMagnitude - thisTouchDeltaMagnitude;

                    _desiredDistance += deltaMagnitudeDifference * Time.deltaTime * ZoomSpeed * Mathf.Abs(_desiredDistance) * 0.001f;
                    _desiredDistance = Mathf.Clamp(_desiredDistance, MinimumZoomDistance, MaximumZoomDistance);
                    _currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, Time.deltaTime * ZoomDampening);
                }
            }
            else if (Mode == Modes.Mouse)
            {
                _scrollWheelAmount += - Input.GetAxis("Mouse ScrollWheel") * MouseWheelSpeed;
                _scrollWheelAmount = Mathf.Clamp(_scrollWheelAmount, -MaxMouseWheelClamp, MaxMouseWheelClamp);
                
                float deltaMagnitudeDifference = _scrollWheelAmount;

                _desiredDistance += deltaMagnitudeDifference * Time.deltaTime * ZoomSpeed * Mathf.Abs(_desiredDistance) * 0.001f;
                _desiredDistance = Mathf.Clamp(_desiredDistance, MinimumZoomDistance, MaximumZoomDistance);
                _currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, Time.deltaTime * ZoomDampening);

            }
        }

        protected virtual void ApplyMovement()
        {
            _position = Target.position - (_rotation * Vector3.forward * _currentDistance + TargetOffset);
            transform.position = _position;
        }
    }
}
