using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace MoreMountains.Tools
{
    public class MMPlotter : MonoBehaviour
    {
        public MethodInfo TweenMethod;
        public int TweenMethodIndex;

        [Header("Graph")]
        public float GraphSize = 1f;
        [Range(0, 1000)]
        public int Resolution = 100;

        [Header("Points")]
        public Transform PlotPointPrefab;
        public float PointScaleFactor = 1f;
        public Material PlotPointMaterial;
        [MMReadOnly]
        public float DistanceBetweenPoints = 1f;

        [Header("Axis")]
        public MMPlotterAxis Axis;
        /*public Material AxisMaterial;
        public bool ShouldDrawAxis = true;
        public Color AxisColor = Color.black;
        public Color AxisLabelColor = Color.black;
        public float AxisWidth = 1f;
        public float AxisCrossOffset;
        public Font AxisFont;
        public int AxisPixelsPerUnit;
        public Vector2 AxisLabelOffset;*/
        
        protected Transform[] _points;
        protected float _pointScale;

        protected Vector3 _scale;
        protected Vector3 _position;
        protected Transform _point;

        protected Vector3 _horizontalAxisStart;
        protected Vector3 _horizontalAxisEnd;
        protected Vector3 _verticalAxisStart;
        protected Vector3 _verticalAxisEnd;

        protected float _axisWidth;
        protected List<MethodInfo> _methodList;
        protected Vector2 _pointValues = Vector2.zero;
        protected object[] _parameter;
        protected MMPlotterAxis _axis;

        protected Vector3 _positionPointInitialPosition;
        protected Vector3 _positionPointVerticalInitialPosition;
        protected Vector3 _rotationPointInitialRotation;
        protected Vector3 _scalePointInitialScale;

        public virtual string[] GetMethodsList()
        {
            FillMethodList();
            List<string> methodNames = new List<string>();
            foreach (MethodInfo method in _methodList)
            {
                methodNames.Add(method.Name);
            }
            string[] _typeDisplays = methodNames.ToArray();
            return _typeDisplays;
        }

        public virtual float InvokeTween(int index, object[] parameters)
        {
            return (float)_methodList[index].Invoke(this, parameters);
        }

        public virtual string TweenName(int index)
        {
            if (_methodList == null)
            {
                FillMethodList();
            }
            return _methodList[index].Name;
        }

        protected virtual void FillMethodList()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
            MethodInfo[] methods = typeof(MMTweenDefinitions).GetMethods(flags);
            _methodList = methods.OrderBy(item => item.Name).ToList();
        }

        protected virtual void OnEnable()
        {
            _parameter = new object[1];
        }

        protected virtual void Start()
        {
            FillMethodList();   
            DrawGraph();
        }

        protected virtual void Initialization()
        {
            _points = new Transform[Resolution];
            DistanceBetweenPoints = GraphSize / Resolution;
            _pointScale = DistanceBetweenPoints * PointScaleFactor;
            _scale = _pointScale * Vector3.one;
            _position = Vector3.zero;
        }

        public virtual void DrawGraph()
        {
            Cleanup();
            Initialization();
            DrawAxis();
            DrawPoints();
        }

        protected virtual void DrawAxis()
        {
            _axis = Instantiate(Axis);

            _axis.SetLabel(TweenName(TweenMethodIndex).Replace("_"," "));
            _axis.transform.SetParent(this.transform);
            _axis.transform.localPosition = Vector3.zero;

            _positionPointInitialPosition = _axis.PositionPoint.transform.localPosition;
            _positionPointVerticalInitialPosition = _axis.PositionPointVertical.transform.localPosition;
            _rotationPointInitialRotation = _axis.RotationPoint.transform.localEulerAngles;
            _scalePointInitialScale = _axis.ScalePoint.transform.localScale;
        }

        protected virtual void DrawPoints()
        {
            for (int i = 0; i < _points.Length; i++)
            {
                _point = Instantiate(PlotPointPrefab);
                _point.name = this.name + "Point" + i;

                _pointValues.x = i * (1f / Resolution);

                _parameter[0] = _pointValues.x;
                _pointValues.y = InvokeTween(TweenMethodIndex, _parameter);
                
                _position.x = i * DistanceBetweenPoints;
                _position.y =  _pointValues.y * GraphSize;

                _point.localPosition = _position;
                _point.localScale = _scale;

                _point.gameObject.MMGetComponentNoAlloc<MeshRenderer>().material = PlotPointMaterial;

                _point.SetParent(transform, false);
                _points[i] = _point;
            }
        }

        public virtual void SetMaterial(Material newMaterial)
        {
            PlotPointMaterial = newMaterial;
        }

        /*protected virtual void DrawLine(Vector3 start, Vector3 end, Color color, float width,  Transform parent)
        {
            GameObject myLine = new GameObject(this.name+"LineRenderer");
            myLine.transform.localPosition = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.material = AxisMaterial;
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            myLine.transform.SetParent(parent);
        }*/

        protected virtual void Cleanup()
        {
            this.transform.MMDestroyAllChildren();
        }

        [Header("Movement")]
        public float MovementPauseDuration = 0.5f;
        protected float _currentMovement = 0f;
        protected float _lastMovementEndedAt = 0f;
        protected Vector3 _curvePointNewMovement = Vector3.zero;
        protected string _timeString;
        protected const float _plotterCurvePointScale = 0.1f;
        protected Vector3 _newScale;
        protected float _newValue;
        protected float _newScaleUnit;
        protected Vector3 Vector3Zero = Vector3.zero;
                
        protected virtual void Update()
        {
            _curvePointNewMovement = Vector3Zero;
            _curvePointNewMovement.x = _currentMovement;
            _parameter[0] = _currentMovement;
            _newValue = InvokeTween(TweenMethodIndex, _parameter);
            
            _curvePointNewMovement.y = _newValue;
            _curvePointNewMovement *= GraphSize;            
            _axis.PlotterCurvePoint.transform.localPosition = _curvePointNewMovement;

            _curvePointNewMovement = _positionPointInitialPosition;
            _curvePointNewMovement.x = _newValue;            
            _axis.PositionPoint.transform.localPosition = _curvePointNewMovement;

            _curvePointNewMovement = _positionPointVerticalInitialPosition;
            _curvePointNewMovement.y = _newValue;
            _axis.PositionPointVertical.transform.localPosition = _curvePointNewMovement;

            _curvePointNewMovement = _rotationPointInitialRotation;
            _curvePointNewMovement.z = _newValue * 360f;
            _axis.RotationPoint.transform.localEulerAngles = _curvePointNewMovement;            

            _curvePointNewMovement = _scalePointInitialScale;
            _curvePointNewMovement *= _newValue;
            _axis.ScalePoint.transform.localScale = _curvePointNewMovement;
            
            if (Time.unscaledTime - _lastMovementEndedAt < MovementPauseDuration)
            {
                if (Time.unscaledTime - _lastMovementEndedAt < MovementPauseDuration / 2f)
                {
                    _currentMovement = 1f;
                    _newScaleUnit = MMTween.Tween(Time.unscaledTime - _lastMovementEndedAt, 0f, (MovementPauseDuration / 2f), 1f, 0f, MMTween.MMTweenCurve.EaseInCubic);
                    _newScale = Vector3.one * _newScaleUnit;

                    _axis.PlotterCurvePoint.localScale = _newScale * _plotterCurvePointScale;
                    _axis.PositionPoint.localScale = _newScale;
                    _axis.PositionPointVertical.localScale = _newScale;
                    _axis.RotationPoint.localScale = _newScale;
                    _axis.ScalePoint.localScale = _newScale;
                }
                else
                {
                    _currentMovement = 0f;
                    _newScaleUnit = MMTween.Tween(Time.unscaledTime - _lastMovementEndedAt, (MovementPauseDuration / 2f), MovementPauseDuration, 0f, 1f, MMTween.MMTweenCurve.EaseOutCubic);
                    _newScale = Vector3.one * _newScaleUnit;
                    
                    _axis.PlotterCurvePoint.localScale = _newScale * _plotterCurvePointScale;
                    _axis.PositionPointVertical.localScale = _newScale;
                    _axis.PositionPoint.localScale = _newScale;
                    _axis.RotationPoint.localScale = _newScale;
                    _axis.ScalePoint.localScale = Vector3.zero;
                }
            }
            else
            {
                _axis.PlotterCurvePoint.localScale = Vector3.one * _plotterCurvePointScale;
                _currentMovement += Time.unscaledDeltaTime;
            }

            if (_currentMovement > 1f)
            {
                _lastMovementEndedAt = Time.unscaledTime;
                _currentMovement = 1f;
            }
            //_timeString = String.Format("t = {0}s", _currentMovement.ToString("0.000"));
            _axis.TimeLabel.text = _timeString;
        }
    }
}