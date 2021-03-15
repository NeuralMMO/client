using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMGyroscope : MonoBehaviour
    {
        public enum TimeScales { Scaled, Unscaled }
               
        [Header("Settings")]
        /// whether this rig should move in scaled or unscaled time
        public TimeScales TimeScale = TimeScales.Scaled;
        /// the clamps to apply to the values
        [MMVector("Min","Max")]
        public Vector2 Clamps = new Vector2(-1f, 1f);
        /// the speed at which to move towards the new position
        public float LerpSpeed = 1f;

        [Header("Debug")]
        /// turn this on if you want to use the inspector to test this camera
        public bool TestMode = false;
        /// the rotation to apply on the x axiswhen in test mode
        [Range(-1f, 1f)]
        public float TestXAcceleration = 0f;
        /// the rotation to apply on the y axis while in test mode
        [Range(-1f, 1f)]
        public float TestYAcceleration = 0f;
        /// the rotation to apply on the y axis while in test mode
        [Range(-1f, 1f)]
        public float TestZAcceleration = 0f;

        [Header("Raw Values")]
        [MMReadOnly]
        public Quaternion GyroscopeAttitude;
        [MMReadOnly]
        public Vector3 GyroscopeRotationRate;
        [MMReadOnly]
        public Vector3 GyroscopeAcceleration;
        [MMReadOnly]
        public Vector3 InputAcceleration;
        [MMReadOnly]
        public Vector3 GyroscopeGravity;

        [Header("AutoCalibration Values")]
        [MMReadOnly]
        public Quaternion InitialGyroscopeAttitude;
        [MMReadOnly]
        public Vector3 InitialGyroscopeRotationRate;
        [MMReadOnly]
        public Vector3 InitialGyroscopeAcceleration;
        [MMReadOnly]
        public Vector3 InitialInputAcceleration;
        [MMReadOnly]
        public Vector3 InitialGyroscopeGravity;

        [Header("Relative Values")]
        [MMReadOnly]
        public Vector3 CalibratedInputAcceleration;
        [MMReadOnly]
        public Vector3 CalibratedGyroscopeGravity;

        [Header("Lerped Values")]
        [MMReadOnly]
        public Vector3 LerpedCalibratedInputAcceleration;
        [MMReadOnly]
        public Vector3 LerpedCalibratedGyroscopeGravity;

        [MMInspectorButton("Calibrate")]
        public bool CalibrateButton;

        protected Gyroscope _gyroscope;
        protected Vector3 _testVector = Vector3.zero;
        protected bool _initialized = false;
        protected Matrix4x4 _accelerationMatrix;
        protected Matrix4x4 _gravityMatrix;


        protected virtual void Start()
        {
            GyroscopeInitialization();
        }

        public virtual void GyroscopeInitialization()
        {
            _gyroscope = Input.gyro;
            _gyroscope.enabled = true;
        }

        protected virtual void Update()
        {
            AutoCalibration();
            GetGyroValues();
        }

        protected virtual void GetGyroValues()
        {
            float deltaTime = (TimeScale == TimeScales.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime;

            GyroscopeAttitude = GyroscopeToUnity(Input.gyro.attitude);
            GyroscopeRotationRate = Input.gyro.rotationRateUnbiased;
            GyroscopeAcceleration = Input.gyro.userAcceleration;

            HandleTestMode();

            ClampAcceleration();
            
            CalibratedInputAcceleration = CalibratedAcceleration(InputAcceleration, _accelerationMatrix);
            CalibratedGyroscopeGravity = CalibratedAcceleration(GyroscopeGravity, _gravityMatrix);

            LerpedCalibratedInputAcceleration = Vector3.Lerp(LerpedCalibratedInputAcceleration, CalibratedInputAcceleration, Time.deltaTime * LerpSpeed);
            LerpedCalibratedGyroscopeGravity = Vector3.Lerp(LerpedCalibratedGyroscopeGravity, CalibratedGyroscopeGravity, Time.deltaTime * LerpSpeed);
        }

        protected virtual void AutoCalibration()
        {
            if (!_initialized && Time.time > 0.5f)
            {
                InitialGyroscopeAttitude = GyroscopeToUnity(Input.gyro.attitude);
                InitialGyroscopeRotationRate = Input.gyro.rotationRateUnbiased;
                InitialGyroscopeAcceleration = Input.gyro.userAcceleration;
                InitialInputAcceleration = Input.acceleration;

                Calibrate();

                _initialized = true;
            }
        }

        protected static Quaternion GyroscopeToUnity(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }

        protected virtual void ClampAcceleration()
        {
            InputAcceleration.x = Mathf.Clamp(InputAcceleration.x, Clamps.x, Clamps.y);
            InputAcceleration.y = Mathf.Clamp(InputAcceleration.y, Clamps.x, Clamps.y);
            InputAcceleration.z = Mathf.Clamp(InputAcceleration.z, Clamps.x, Clamps.y);

            GyroscopeGravity.x = Mathf.Clamp(GyroscopeGravity.x, Clamps.x, Clamps.y);
            GyroscopeGravity.y = Mathf.Clamp(GyroscopeGravity.y, Clamps.x, Clamps.y);
            GyroscopeGravity.z = Mathf.Clamp(GyroscopeGravity.z, Clamps.x, Clamps.y);
        }

        protected virtual void HandleTestMode()
        {
            if (TestMode)
            {
                _testVector.x = TestXAcceleration;
                _testVector.y = TestYAcceleration;
                _testVector.z = TestZAcceleration;
                InputAcceleration = _testVector;
                GyroscopeGravity = _testVector;
            }
            else
            {
                InputAcceleration = Input.acceleration;
                GyroscopeGravity = Input.gyro.gravity;
            }
        }



        protected virtual void Calibrate()
        {
            _accelerationMatrix = CalibrateAcceleration(InputAcceleration);
            _gravityMatrix = CalibrateAcceleration(Input.gyro.gravity);
        }

        protected virtual Matrix4x4 CalibrateAcceleration(Vector3 initialAcceleration)
        {
            Quaternion rotationQuaternion = Quaternion.FromToRotation(-Vector3.forward, initialAcceleration);
            Matrix4x4 newMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuaternion, Vector3.one);
            return newMatrix.inverse;
        }

        protected virtual Vector3 CalibratedAcceleration(Vector3 accelerator, Matrix4x4 matrix)
        {
            Vector3 fixedAcceleration = matrix.MultiplyVector(accelerator);
            return fixedAcceleration;
        }
    }
}
