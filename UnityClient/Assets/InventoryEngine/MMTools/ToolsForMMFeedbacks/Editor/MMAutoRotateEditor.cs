using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Custom editor for the MMAutoRotate component
    /// </summary>
    [CustomEditor(typeof(MMAutoRotate), true)]
    [CanEditMultipleObjects]
    public class MMAutoRotateEditor : Editor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="autoRotate"></param>
        /// <param name="gizmoType"></param>
        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        static void DrawHandles(MMAutoRotate autoRotate, GizmoType gizmoType)
        {
            MMAutoRotate myTarget = autoRotate;

            // only draw gizmos if orbiting and gizmos enabled
            if (!myTarget.Orbiting || !myTarget.DrawGizmos)
            {
                return;
            };

            // if we're not playing, we compute our center/axis
            if (!Application.isPlaying)
            {
                if (myTarget.OrbitCenterTransform != null)
                {
                    myTarget._orbitCenter = myTarget.OrbitCenterTransform.transform.position + myTarget.OrbitCenterOffset;
                    myTarget._worldRotationAxis = myTarget.OrbitCenterTransform.TransformDirection(myTarget.OrbitRotationAxis);
                    myTarget._rotationPlane.SetNormalAndPosition(myTarget._worldRotationAxis.normalized, myTarget._orbitCenter);

                    myTarget._snappedPosition = myTarget._rotationPlane.ClosestPointOnPlane(myTarget.transform.position);
                    myTarget._radius = myTarget.OrbitRadius * Vector3.Normalize(myTarget._snappedPosition - myTarget._orbitCenter);
                }                
            }

            // draws a plane disc
            Handles.color = myTarget.OrbitPlaneColor;
            Handles.DrawSolidDisc(myTarget._orbitCenter, myTarget._rotationPlane.normal, myTarget.OrbitRadius + 0.5f);

            // draws a circle to mark the orbit
            Handles.color = myTarget.OrbitLineColor;
            Handles.DrawWireArc(myTarget._orbitCenter, myTarget._rotationPlane.normal, Vector3.ProjectOnPlane(myTarget._orbitCenter + Vector3.forward, myTarget._rotationPlane.normal), 360f, myTarget.OrbitRadius);
            
            // draws an arrow to mark the direction
            Quaternion newRotation = Quaternion.AngleAxis(1f, myTarget._worldRotationAxis);
            Vector3 origin = myTarget._orbitCenter + newRotation * myTarget._radius;
            newRotation = Quaternion.AngleAxis(15f, myTarget._worldRotationAxis);
            Vector3 direction = Vector3.zero;
            if (myTarget.OrbitRotationSpeed > 0f)
            {
                direction = (myTarget._orbitCenter + newRotation * myTarget._radius) - origin;
            }
            else
            {
                direction = origin - (myTarget._orbitCenter + newRotation * myTarget._radius);
            }
            MMDebug.DebugDrawArrow(origin, direction, myTarget.OrbitLineColor);
        }
    }
}
