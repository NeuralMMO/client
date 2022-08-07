using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;

namespace MoreMountains.Tools
{
    [Serializable]
    public class MMConeOfVision2D : MonoBehaviour
    {
        public struct RaycastData
        {
            public bool Hit;
            public Vector3 Point;
            public float Distance;
            public float Angle;

            public RaycastData(bool hit, Vector3 point, float distance, float angle)
            {
                Hit = hit;
                Point = point;
                Distance = distance;
                Angle = angle;
            }
        }

        public struct MeshEdgePosition
        {
            public Vector3 PointA;
            public Vector3 PointB;

            public MeshEdgePosition(Vector3 pointA, Vector3 pointB)
            {
                PointA = pointA;
                PointB = pointB;
            }
        }

        [Header("Vision")]
        public LayerMask ObstacleMask;
        public float VisionRadius = 5f;
        [Range(0f, 360f)]
        public float VisionAngle = 20f;
        [MMReadOnly]
        public Vector3 Direction;
        [MMReadOnly]
        public Vector3 EulerAngles;

        [Header("Target scanning")]
        public bool ShouldScanForTargets = true;
        public LayerMask TargetMask;
        public float ScanFrequencyInSeconds = 1f;
        [MMReadOnly]
        public List<Transform> VisibleTargets = new List<Transform>();
        
        [Header("Mesh")]
        public float MeshDensity = 0.2f;
        public int EdgePrecision = 3;
        public float EdgeThreshold = 0.5f;

        public MeshFilter VisionMeshFilter;

        protected Mesh _visionMesh;
        protected Collider2D[] _targetsWithinDistance;
        protected Transform _target;
        protected Vector3 _directionToTarget;
        protected float _distanceToTarget;
        protected float _lastScanTimestamp;

        protected virtual void Awake()
        {
            _visionMesh = new Mesh();
            VisionMeshFilter.mesh = _visionMesh;
        }

        protected virtual void LateUpdate()
        {
            if ((Time.time - _lastScanTimestamp > ScanFrequencyInSeconds) && ShouldScanForTargets)
            {
                ScanForTargets();
            }
            DrawMesh();
        }

        public virtual void SetDirectionAndAngles(Vector3 direction, Vector3 eulerAngles)
        {
            Direction = direction;
            EulerAngles = eulerAngles;
        }
        
        protected virtual void ScanForTargets()
        {
            _lastScanTimestamp = Time.time;
            VisibleTargets.Clear();
            _targetsWithinDistance = Physics2D.OverlapCircleAll(this.transform.position, VisionRadius, TargetMask);
            foreach (Collider2D collider in _targetsWithinDistance)
            {
                _target = collider.transform;
                _directionToTarget = (_target.position - this.transform.position).normalized;
                if (Vector3.Angle(Direction, _directionToTarget) < VisionAngle / 2f)
                {
                    _distanceToTarget = Vector3.Distance(this.transform.position, _target.position);

                    RaycastHit2D hit2D = Physics2D.Raycast(this.transform.position, _directionToTarget, _distanceToTarget, ObstacleMask);
                    if (!hit2D)
                    {
                        VisibleTargets.Add(_target);
                    }
                }
            }
        }
        
        protected virtual void DrawMesh()
        {
            int steps = Mathf.RoundToInt(MeshDensity * VisionAngle);
            float stepsAngle = VisionAngle / steps;

            List<Vector3> viewPoints = new List<Vector3>();
            RaycastData oldViewCast = new RaycastData();

            for (int i = 0; i <= steps; i++)
            {
                float angle = stepsAngle * i + EulerAngles.y - VisionAngle / 2f;
                RaycastData viewCast = RaycastAtAngle(angle);

                if (i > 0)
                {
                    bool thresholdExceeded = Mathf.Abs(oldViewCast.Distance - viewCast.Distance) > EdgeThreshold;

                    if ((oldViewCast.Hit != viewCast.Hit)
                        || (oldViewCast.Hit && viewCast.Hit && thresholdExceeded))
                    {
                        MeshEdgePosition edge = FindMeshEdgePosition(oldViewCast, viewCast);
                        if (edge.PointA != Vector3.zero)
                        {
                            viewPoints.Add(edge.PointA);
                        }
                        if (edge.PointB != Vector3.zero)
                        {
                            viewPoints.Add(edge.PointB);
                        }
                    }
                }

                viewPoints.Add(viewCast.Point);
                oldViewCast = viewCast;
            }

            int numberOfVertices = viewPoints.Count + 1;
            Vector3[] vertices = new Vector3[numberOfVertices];
            int[] triangles = new int[(numberOfVertices - 2) * 3];

            vertices[0] = Vector3.zero;
            for (int i = 0; i < numberOfVertices - 1; i++)
            {
                vertices[i + 1] = this.transform.InverseTransformPoint(viewPoints[i]);

                if (i < numberOfVertices - 2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

            _visionMesh.Clear();
            _visionMesh.vertices = vertices;
            _visionMesh.triangles = triangles;
            _visionMesh.RecalculateNormals();
        }

        MeshEdgePosition FindMeshEdgePosition(RaycastData minimumViewCast, RaycastData maximumViewCast)
        {
            float minAngle = minimumViewCast.Angle;
            float maxAngle = maximumViewCast.Angle;
            Vector3 minPoint = minimumViewCast.Point;
            Vector3 maxPoint = maximumViewCast.Point;

            for (int i = 0; i < EdgePrecision; i++)
            {
                float angle = (minAngle + maxAngle) / 2;
                RaycastData newViewCast = RaycastAtAngle(angle);

                bool thresholdExceeded = Mathf.Abs(minimumViewCast.Distance - newViewCast.Distance) > EdgeThreshold;
                if (newViewCast.Hit = minimumViewCast.Hit && !thresholdExceeded)
                {
                    minAngle = angle;
                    minPoint = newViewCast.Point;
                }
                else
                {
                    maxAngle = angle;
                    maxPoint = newViewCast.Point;
                }
            }

            return new MeshEdgePosition(minPoint, maxPoint);
        }

        RaycastData RaycastAtAngle(float angle)
        {
            Vector3 direction = MMMaths.DirectionFromAngle2D(angle, 0f);

            RaycastHit2D hit2D = Physics2D.Raycast(this.transform.position, direction, VisionRadius, ObstacleMask);

            if (hit2D)
            {
                return new RaycastData(true, hit2D.point, hit2D.distance, angle);
            }
            else
            {
                return new RaycastData(false, this.transform.position + direction * VisionRadius, VisionRadius, angle);
            }
        }
    }
}
