using UnityEngine;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    [RequireComponent(typeof(LineRenderer))]
    public class MMBezierLineRenderer : MonoBehaviour
    {
        public Transform[] AdjustmentHandles;
        public int NumberOfSegments = 50;
        public int SortingLayerID = 0;
        [MMReadOnly]
        public int NumberOfCurves = 0;

        protected LineRenderer _lineRenderer;    

        protected virtual void Awake()
        {
            NumberOfCurves = (int)AdjustmentHandles.Length / 3;

            _lineRenderer = GetComponent<LineRenderer>();   
            if (_lineRenderer != null)
            {
                _lineRenderer.sortingLayerID = SortingLayerID;
            }                        
        }

        protected virtual void Update()
        {
            DrawCurve();
        }

        protected virtual void DrawCurve()
        {
            for (int i = 0; i < NumberOfCurves; i++)
            {
                for (int j = 1; j <= NumberOfSegments; j++)
                {
                    float t = j / (float)NumberOfSegments;
                    int pointIndex = i * 3;
                    Vector3 point = BezierPoint(t, AdjustmentHandles[pointIndex].position, AdjustmentHandles[pointIndex + 1].position, AdjustmentHandles[pointIndex + 2].position, AdjustmentHandles[pointIndex + 3].position);
                    _lineRenderer.positionCount = (i * NumberOfSegments) + j;                    
                    _lineRenderer.SetPosition((i * NumberOfSegments) + (j - 1), point);
                }
            }
        }

        protected virtual Vector3 BezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}