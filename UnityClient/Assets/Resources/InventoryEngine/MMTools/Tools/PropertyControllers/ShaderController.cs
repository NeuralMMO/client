using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

namespace MoreMountains.Tools
{

    /// <summary>
    /// A class used to control a float in any other class, over time
    /// To use it, simply drag a monobehaviour in its target field, pick a control mode (ping pong or random), and tweak the settings
    /// </summary>
    public class ShaderController : MonoBehaviour
    {
        /// the possible types of targets
        public enum TargetTypes { Renderer, Image, Text }

        /// the possible types of properties
        public enum PropertyTypes { Bool, Float, Int, Vector, Keyword, Color }

        /// the possible control modes
        public enum ControlModes { PingPong, Random, OneTime, AudioAnalyzer }

        [Header("Target")]
        /// the type of renderer to pilot
        public TargetTypes TargetType = TargetTypes.Renderer;
        /// the renderer with the shader you want to control
        [MMEnumCondition("TargetType",(int)TargetTypes.Renderer)]
        public Renderer TargetRenderer;
        /// the ID of the material in the Materials array on the target renderer (usually 0)
        [MMEnumCondition("TargetType", (int)TargetTypes.Renderer)]
        public int TargetMaterialID = 0;
        /// the Image with the shader you want to control
        [MMEnumCondition("TargetType", (int)TargetTypes.Image)]
        public Image TargetImage;
        /// the Text with the shader you want to control
        [MMEnumCondition("TargetType", (int)TargetTypes.Text)]
        public Text TargetText;
        /// if this is true, material will be cached on Start
        public bool CacheMaterial = true;
        /// if this is true, an instance of the material will be created on start so that this controller only affects its target
        public bool CreateMaterialInstance = false;
        /// the EXACT name of the property to affect
        public string TargetPropertyName;
        /// the type of the property to affect
        public PropertyTypes PropertyType = PropertyTypes.Float;
        /// whether or not to affect its x component
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
        public bool X;
        /// whether or not to affect its y component
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
        public bool Y;
        /// whether or not to affect its z component
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
        public bool Z;
        /// whether or not to affect its w component
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
        public bool W;
        /// the color to lerp from	
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Color)]
        public Color FromColor = Color.black;
        /// the color to lerp to	
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Color)]
        public Color ToColor = Color.white;

        [Header("Settings")]
        /// the control mode (ping pong or random)
        public ControlModes ControlMode;
        /// whether or not the updated value should be added to the initial one
        public bool AddToInitialValue = false;
        /// whether or not to use unscaled time
        public bool UseUnscaledTime = true;
        /// if this is true, control will happen, otherwise it won't
        public bool Active = true;

        /// the curve to apply to the tween
        [MMEnumCondition("ControlMode", (int)ControlModes.PingPong)]
        [Header("Ping Pong")]
        public MMTween.MMTweenCurve Curve;
        /// the minimum value for the ping pong
        [MMEnumCondition("ControlMode", (int)ControlModes.PingPong)]
        public float MinValue = 0f;
        /// the maximum value for the ping pong
        [MMEnumCondition("ControlMode", (int)ControlModes.PingPong)]
        public float MaxValue = 5f;
        /// the duration of one ping (or pong)
        [MMEnumCondition("ControlMode", (int)ControlModes.PingPong)]
        public float Duration = 1f;
        /// the duration of the pause between two ping (or pongs) (in seconds)
        [MMEnumCondition("ControlMode", (int)ControlModes.PingPong)]
        public float PingPongPauseDuration = 1f;

        [Header("Random")]
        [MMVector("Min", "Max")]
        /// the noise amplitude
        [MMEnumCondition("ControlMode", (int)ControlModes.Random)]
        public Vector2 Amplitude = new Vector2(0f,5f);
        [MMVector("Min", "Max")]
        /// the noise frequency
        [MMEnumCondition("ControlMode", (int)ControlModes.Random)]
        public Vector2 Frequency = new Vector2(1f, 1f);
        [MMVector("Min", "Max")]
        /// the noise shift
        [MMEnumCondition("ControlMode", (int)ControlModes.Random)]
        public Vector2 Shift = new Vector2(0f, 1f);

        [Header("OneTime")]
        /// the duration of the One Time shake
        [MMEnumCondition("ControlMode", (int)ControlModes.OneTime)]
        public float OneTimeDuration = 1f;
        [MMEnumCondition("ControlMode", (int)ControlModes.OneTime)]
        public bool ResetValueAfterOneTime = true;
        /// the amplitude of the One Time shake (this will be multiplied by the curve's height)
        [MMEnumCondition("ControlMode", (int)ControlModes.OneTime)]
        public float OneTimeAmplitude = 1f;
        /// the curve to apply to the one time shake
        [MMEnumCondition("ControlMode", (int)ControlModes.OneTime)]
        public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [MMInspectorButton("OneTime")]
        /// a test button for the one time shake
        [MMEnumCondition("ControlMode", (int)ControlModes.OneTime)]
        public bool OneTimeButton;

        [Header("AudioAnalyzer")]
        /// the bound audio analyzer used to drive this controller
        [MMEnumCondition("ControlMode", (int)ControlModes.AudioAnalyzer)]
        public MMAudioAnalyzer AudioAnalyzer;
        /// the ID of the selected beat on the analyzer
        [MMEnumCondition("ControlMode", (int)ControlModes.AudioAnalyzer)]
        public int BeatID;
        /// the multiplier to apply to the value out of the analyzer
        [MMEnumCondition("ControlMode", (int)ControlModes.AudioAnalyzer)]
        public float AudioAnalyzerMultiplier = 1f;
        /// the offset to apply to the value out of the analyzer
        [MMEnumCondition("ControlMode", (int)ControlModes.AudioAnalyzer)]
        public float AudioAnalyzerOffset = 0f;
        /// the speed at which to lerp the value
        [MMEnumCondition("ControlMode", (int)ControlModes.AudioAnalyzer)]
        public float AudioAnalyzerLerp = 60f;

        [Header("Debug")]
        [MMReadOnly]
        /// the initial value of the controlled float
        public float InitialValue;
        [MMReadOnly]
        /// the current value of the controlled float
        public float CurrentValue;
        [MMReadOnly]
        /// the current value of the controlled float	
        public Color InitialColor;

        [MMReadOnly]
        /// the ID of the property
        public int PropertyID;
        [MMReadOnly]
        /// whether or not the property got found
        public bool PropertyFound = false;
        [MMReadOnly]
        /// the target material
        public Material TargetMaterial;

        /// internal use only
        [HideInInspector]
        public float PingPong;
        
        protected float _randomAmplitude;
        protected float _randomFrequency;
        protected float _randomShift;
        protected float _elapsedTime = 0f;

        protected bool _oneTimeShaking = false;
        protected float _oneTimeStartedTimestamp = 0f;
        protected float _remappedTimeSinceStart = 0f;
        protected Color _currentColor;

        protected Vector4 _vectorValue;

        protected float _pingPongDirection = 1f;
        protected float _lastPingPongPauseAt = 0f;

        /// <summary>
        /// Finds an attribute (property or field) on the target object
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual bool FindShaderProperty(string propertyName)
        {
            if (TargetType == TargetTypes.Renderer)
            {
                if (CreateMaterialInstance)
                {
                    TargetRenderer.materials[TargetMaterialID] = new Material(TargetRenderer.materials[TargetMaterialID]);
                }
                TargetMaterial = TargetRenderer.materials[TargetMaterialID];
            }
            else if (TargetType == TargetTypes.Image)
            {
                if (CreateMaterialInstance)
                {
                    TargetImage.material = new Material(TargetImage.material);
                }
                TargetMaterial = TargetImage.material;
            }
            else if (TargetType == TargetTypes.Text)
            {
                if (CreateMaterialInstance)
                {
                    TargetText.material = new Material(TargetText.material);
                }
                TargetMaterial = TargetText.material;
            }

            if (PropertyType == PropertyTypes.Keyword)
            {
                PropertyFound = true;
                return true;
            }
            if (TargetMaterial.HasProperty(propertyName))
            {                
                PropertyID = Shader.PropertyToID(propertyName);
                PropertyFound = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// On start we initialize our controller
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Returns true if the renderer is null, false otherwise
        /// </summary>
        /// <returns></returns>
        protected virtual bool RendererIsNull()
        {
            if ((TargetType == TargetTypes.Renderer) && (TargetRenderer == null))
            {
                return true;
            }
            if ((TargetType == TargetTypes.Image) && (TargetImage == null))
            {
                return true;
            }
            if ((TargetType == TargetTypes.Text) && (TargetText == null))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Grabs the target property and initializes stuff
        /// </summary>
        public virtual void Initialization()
        {
            if (RendererIsNull() || (TargetPropertyName == ""))
            {
                return;
            }
            
            PropertyFound = FindShaderProperty(TargetPropertyName);
            if (!PropertyFound)
            {
                return;
            }
            
            _elapsedTime = 0f;
            _randomAmplitude = Random.Range(Amplitude.x, Amplitude.y);
            _randomFrequency = Random.Range(Frequency.x, Frequency.y);
            _randomShift = Random.Range(Shift.x, Shift.y);

            GetInitialValue();
                
            _oneTimeShaking = false;
        }
        
        /// <summary>
        /// Triggers a one time shake of the float controller
        /// </summary>
        public virtual void OneTime()
        {
            if (!CacheMaterial)
            {
                Initialization();
            }

            if (RendererIsNull() || (!PropertyFound) || (!Active) || (ControlMode != ControlModes.OneTime))
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
            if (RendererIsNull() || (!Active) || (!PropertyFound))
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
                    CurrentValue = Mathf.Lerp(CurrentValue, AudioAnalyzer.Beats[BeatID].CurrentValue * AudioAnalyzerMultiplier + AudioAnalyzerOffset, AudioAnalyzerLerp * Time.deltaTime);
                    break;
            }

            if (PropertyType == PropertyTypes.Color)
            {
                _currentColor = Color.Lerp(FromColor, ToColor, CurrentValue);
            }

            if (AddToInitialValue)
            {
                CurrentValue += InitialValue;
            }

            if (_oneTimeShaking && (Time.time - _oneTimeStartedTimestamp > OneTimeDuration))
            {
                _oneTimeShaking = false;
                if (ResetValueAfterOneTime)
                {
                    CurrentValue = InitialValue;
                }                
            }

            SetValue(CurrentValue);
        }

        /// <summary>
        /// Grabs and stores the initial value
        /// </summary>
        protected virtual void GetInitialValue()
        {
            switch (PropertyType)
            {
                case PropertyTypes.Bool:
                    InitialValue = TargetMaterial.GetInt(PropertyID);
                    break;

                case PropertyTypes.Int:
                    InitialValue = TargetMaterial.GetInt(PropertyID);
                    break;

                case PropertyTypes.Float:
                    InitialValue = TargetMaterial.GetFloat(PropertyID);
                    break;

                case PropertyTypes.Vector:
                    InitialValue = TargetMaterial.GetVector(PropertyID).x;
                    break;

                case PropertyTypes.Keyword:
                    InitialValue = TargetMaterial.IsKeywordEnabled(TargetPropertyName) ? 1f : 0f;
                    break;

                case PropertyTypes.Color:
                    InitialColor = TargetMaterial.GetColor(PropertyID);
                    InitialValue = 0f;
                    break;
            }
        }

        /// <summary>
        /// Sets the value in the shader
        /// </summary>
        /// <param name="newValue"></param>
        protected virtual void SetValue(float newValue)
        {
            switch (PropertyType)
            {
                case PropertyTypes.Bool:
                    newValue = (newValue > 0f) ? 1f : 0f;
                    TargetMaterial.SetInt(PropertyID, Mathf.RoundToInt(newValue));
                    break;

                case PropertyTypes.Keyword:
                    newValue = (newValue > 0f) ? 1f : 0f;
                    if (newValue == 0f)
                    {
                        TargetMaterial.DisableKeyword(TargetPropertyName);
                    }
                    else
                    {
                        TargetMaterial.EnableKeyword(TargetPropertyName);
                    }
                    break;

                case PropertyTypes.Int:
                    TargetMaterial.SetInt(PropertyID, Mathf.RoundToInt(newValue));
                    break;

                case PropertyTypes.Float:
                    TargetMaterial.SetFloat(PropertyID, newValue);
                    break;

                case PropertyTypes.Vector:
                    _vectorValue = TargetMaterial.GetVector(PropertyID);
                    if (X)
                    {
                        _vectorValue.x = newValue;
                    }
                    if (Y)
                    {
                        _vectorValue.y = newValue;
                    }
                    if (Z)
                    {
                        _vectorValue.z = newValue;
                    }
                    if (W)
                    {
                        _vectorValue.w = newValue;
                    }
                    TargetMaterial.SetVector(PropertyID, _vectorValue);
                    break;
                    
                case PropertyTypes.Color:
                    TargetMaterial.SetColor(PropertyID, _currentColor);
                    break;
            }
        }
    }
}

