using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace MoreMountains.Tools
{
    public class MonoAttribute
    {
        public enum MemberTypes { Property, Field }
        public MonoBehaviour TargetObject;
        public MemberTypes MemberType;
        public PropertyInfo MemberPropertyInfo;
        public FieldInfo MemberFieldInfo;
        public string MemberName;

        public MonoAttribute(MonoBehaviour targetObject, MemberTypes type, PropertyInfo propertyInfo, FieldInfo fieldInfo, string memberName)
        {
            TargetObject = targetObject;
            MemberType = type;
            MemberPropertyInfo = propertyInfo;
            MemberFieldInfo = fieldInfo;
            MemberName = memberName;
        }

        public virtual float GetValue()
        {
            if (MemberType == MonoAttribute.MemberTypes.Property)
            {
                return (float)MemberPropertyInfo.GetValue(TargetObject);
            }
            else if (MemberType == MonoAttribute.MemberTypes.Field)
            {
                return (float)MemberFieldInfo.GetValue(TargetObject);
            }
            return 0f;
        }

        public virtual void SetValue(float newValue)
        {
            if (MemberType == MonoAttribute.MemberTypes.Property)
            {
                MemberPropertyInfo.SetValue(TargetObject, newValue);
            }
            else if (MemberType == MonoAttribute.MemberTypes.Field)
            {
                MemberFieldInfo.SetValue(TargetObject, newValue);
            }
        }
    }

    /// <summary>
    /// A class used to control a float in any other class, over time
    /// To use it, simply drag a monobehaviour in its target field, pick a control mode (ping pong or random), and tweak the settings
    /// </summary>
    public class FloatController : MonoBehaviour
    {
        /// the possible control modes
        public enum ControlModes { PingPong, Random, OneTime, AudioAnalyzer }

        [Header("Target")]
        /// the mono on which the float you want to control is
        public MonoBehaviour TargetObject;

        [Header("Settings")]
        /// the control mode (ping pong or random)
        public ControlModes ControlMode;
        /// whether or not the updated value should be added to the initial one
        public bool AddToInitialValue = false;
        /// whether or not to use unscaled time
        public bool UseUnscaledTime = true;
        /// if this is true, control will happen, otherwise it won't
        public bool Active = true;

        [Header("Ping Pong")]
        /// the curve to apply to the tween
        public MMTween.MMTweenCurve Curve;
        /// the minimum value for the ping pong
        public float MinValue = 0f;
        /// the maximum value for the ping pong
        public float MaxValue = 5f;
        /// the duration of one ping (or pong)
        public float Duration = 1f;
        /// the duration (in seconds) between a ping and a pong 
        public float PingPongPauseDuration = 0f;


        [Header("Random")]
        [MMVector("Min", "Max")]
        /// the noise amplitude
        public Vector2 Amplitude = new Vector2(0f,5f);
        [MMVector("Min", "Max")]
        /// the noise frequency
        public Vector2 Frequency = new Vector2(1f, 1f);
        [MMVector("Min", "Max")]
        /// the noise shift
        public Vector2 Shift = new Vector2(0f, 1f);

        [Header("OneTime")]
        /// the duration of the One Time shake
        public float OneTimeDuration = 1f;
        /// the amplitude of the One Time shake (this will be multiplied by the curve's height)
        public float OneTimeAmplitude = 1f;
        /// the curve to apply to the one time shake
        public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [MMInspectorButton("OneTime")]
        /// a test button for the one time shake
        public bool OneTimeButton;

        [Header("AudioAnalyzer")]
        public MMAudioAnalyzer AudioAnalyzer;
        public int BeatID;
        public float AudioAnalyzerMultiplier = 1f;

        [Header("Debug")]
        [MMReadOnly]
        /// the initial value of the controlled float
        public float InitialValue;
        [MMReadOnly]
        /// the current value of the controlled float
        public float CurrentValue;

        /// internal use only
        [HideInInspector]
        public float PingPong;
        /// internal use only
        [HideInInspector]
        public MonoAttribute TargetAttribute;
        /// internal use only
        [HideInInspector]
        public string[] AttributeNames;
        /// internal use only
        [HideInInspector]
        public string PropertyName;
        /// internal use only
        [HideInInspector]
        public int ChoiceIndex;

        public const string _undefinedString = "<Undefined Attribute>";

        protected List<string> _attributesNamesTempList;
        protected PropertyInfo[] _propertyReferences;
        protected FieldInfo[] _fieldReferences;
        protected bool _attributeFound;

        protected float _randomAmplitude;
        protected float _randomFrequency;
        protected float _randomShift;
        protected float _elapsedTime = 0f;

        protected bool _oneTimeShaking = false;
        protected float _oneTimeStartedTimestamp = 0f;
        protected float _remappedTimeSinceStart = 0f;

        protected float _pingPongDirection = 1f;
        protected float _lastPingPongPauseAt = 0f;

        /// <summary>
        /// Finds an attribute (property or field) on the target object
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual bool FindAttribute(string propertyName)
        {
            FieldInfo fieldInfo = null;
            PropertyInfo propInfo = null;
            TargetAttribute = null;

            propInfo = TargetObject.GetType().GetProperty(propertyName);
            if (propInfo == null)
            {
                fieldInfo = TargetObject.GetType().GetField(propertyName);
            }
            if (propInfo != null)
            {
                TargetAttribute = new MonoAttribute(TargetObject, MonoAttribute.MemberTypes.Property, propInfo, null, propertyName);
            }
            if (fieldInfo != null)
            {
                TargetAttribute = new MonoAttribute(TargetObject, MonoAttribute.MemberTypes.Field, null, fieldInfo, propertyName);
            }
            if (PropertyName == _undefinedString)
            {
                Debug.LogError("FloatController " + this.name + " : you need to pick a property from the Property list");
                return false;
            }
            if ((propInfo == null) && (fieldInfo == null))
            {
                Debug.LogError("FloatController " + this.name + " couldn't find any property or field named " + propertyName + " on " + TargetObject.name);
                return false;
            }

            if (TargetAttribute.MemberType == MonoAttribute.MemberTypes.Property)
            {
                TargetAttribute.MemberPropertyInfo = TargetObject.GetType().GetProperty(TargetAttribute.MemberName);
            }
            else if (TargetAttribute.MemberType == MonoAttribute.MemberTypes.Field)
            {
                TargetAttribute.MemberFieldInfo = TargetObject.GetType().GetField(TargetAttribute.MemberName);
            }

            return true;
        }

        /// <summary>
        /// On start we initialize our controller
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Grabs the target property and initializes stuff
        /// </summary>
        public virtual void Initialization()
        {
            _attributeFound = FindAttribute(PropertyName);
            if (!_attributeFound)
            {
                return;
            }

            if ((TargetObject == null) || (TargetAttribute.MemberName == ""))
            {
                return;
            }
            
            _elapsedTime = 0f;
            _randomAmplitude = Random.Range(Amplitude.x, Amplitude.y);
            _randomFrequency = Random.Range(Frequency.x, Frequency.y);
            _randomShift = Random.Range(Shift.x, Shift.y);
            
            if (TargetAttribute.MemberType == MonoAttribute.MemberTypes.Property)
            {
                InitialValue = (float)TargetAttribute.MemberPropertyInfo.GetValue(TargetObject);
            }
            else if (TargetAttribute.MemberType == MonoAttribute.MemberTypes.Field)
            {
                InitialValue = (float)TargetAttribute.MemberFieldInfo.GetValue(TargetObject);
            }

            _oneTimeShaking = false;
        }

        /// <summary>
        /// Triggers a one time shake of the float controller
        /// </summary>
        public virtual void OneTime()
        {
            if ((TargetObject == null) || (TargetAttribute == null) || (!Active) || (ControlMode != ControlModes.OneTime))
            {
                return;
            }
            if (_oneTimeShaking || !Active)
            {
                return;
            }
            else
            {
                _oneTimeStartedTimestamp = Time.time;
                _oneTimeShaking = true;
            }
        }

        /// <summary>
        /// Returns the relevant delta time
        /// </summary>
        /// <returns></returns>
        protected float GetDeltaTime()
        {
            return UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        /// <summary>
        /// Returns the relevant time
        /// </summary>
        /// <returns></returns>
        protected float GetTime()
        {
            return UseUnscaledTime ? Time.unscaledTime : Time.time;
        }

        /// <summary>
        /// On Update, we move our value based on the defined settings
        /// </summary>
        protected virtual void Update()
        {
            if ((TargetObject == null) || (TargetAttribute == null) || (!Active) || (!_attributeFound))
            {
                return;
            }

            switch (ControlMode)
            {
                case ControlModes.PingPong:
                    
                    if (GetTime() - _lastPingPongPauseAt < PingPongPauseDuration)
                    {
                        return;
                    }
                    PingPong += GetDeltaTime() * _pingPongDirection;

                    if (PingPong < 0f)
                    {
                        PingPong = 0f;
                        _pingPongDirection = -_pingPongDirection;
                        _lastPingPongPauseAt = GetTime();
                    }

                    if (PingPong > Duration)
                    {
                        PingPong = Duration;
                        _pingPongDirection = -_pingPongDirection;
                        _lastPingPongPauseAt = GetTime();
                    }


                    CurrentValue = MMTween.Tween(PingPong, 0f, Duration, MinValue, MaxValue, Curve);
                    break;
                case ControlModes.Random:
                    _elapsedTime += GetDeltaTime();
                    CurrentValue = (Mathf.PerlinNoise(_randomFrequency * _elapsedTime, _randomShift) * 2.0f - 1.0f) * _randomAmplitude;
                    break;
                case ControlModes.OneTime:
                    if (!_oneTimeShaking)
                    {
                        return;
                    }
                    _remappedTimeSinceStart = MMMaths.Remap(Time.time - _oneTimeStartedTimestamp, 0f, OneTimeDuration, 0f, 1f);
                    CurrentValue = OneTimeCurve.Evaluate(_remappedTimeSinceStart) * OneTimeAmplitude;
                    if (AddToInitialValue)
                    {
                        CurrentValue += InitialValue;
                    }
                    break;
                case ControlModes.AudioAnalyzer:
                    CurrentValue = AudioAnalyzer.Beats[BeatID].CurrentValue * AudioAnalyzerMultiplier;
                    break;
            }
                                   

            if (AddToInitialValue)
            {
                CurrentValue += InitialValue;
            }

            if (_oneTimeShaking && (Time.time - _oneTimeStartedTimestamp > OneTimeDuration))
            {
                _oneTimeShaking = false;
                CurrentValue = InitialValue;
            }

            TargetAttribute.SetValue(CurrentValue);
        }
        
        /// <summary>
        /// When the contents of the inspector change, and if the target changed, we grab all its properties and store them
        /// </summary>
        protected virtual void OnValidate()
        {
            FillDropDownList();
            if (Application.isPlaying)
            {
                Initialization();
            }
        }

        /// <summary>
        /// Fills the inspector dropdown with all the possible choices
        /// </summary>
        public virtual void FillDropDownList()
        {            
            AttributeNames = new string[0];

            if (TargetObject == null)
            {
                return;
            }

            _propertyReferences = TargetObject.GetType().GetProperties();
            _attributesNamesTempList = new List<string>();
            _attributesNamesTempList.Add(_undefinedString);

            foreach (PropertyInfo propertyInfo in _propertyReferences)
            {
                if (propertyInfo.PropertyType.Name == "Single")
                {
                    _attributesNamesTempList.Add(propertyInfo.Name);
                }
            }

            _fieldReferences = TargetObject.GetType().GetFields();
            foreach (FieldInfo fieldInfo in _fieldReferences)
            {
                if (fieldInfo.FieldType.Name == "Single")
                {
                    _attributesNamesTempList.Add(fieldInfo.Name);
                }
            }

            // we fill our dropdown list of names :
            AttributeNames = _attributesNamesTempList.ToArray();
        }
    }
}

