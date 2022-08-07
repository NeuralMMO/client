using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;

namespace MoreMountains.Tools
{
    [CustomEditor(typeof(MMConeOfVision2D))]
    public class MMConeOfVision2DInspector : Editor
    {
        protected MMConeOfVision2D _coneOfVision;

        protected virtual void OnSceneGUI()
        {
            // draws a circle around the character to represent the cone of vision's radius
            _coneOfVision = (MMConeOfVision2D)target;

            Handles.color = Color.yellow;
            Handles.DrawWireArc(_coneOfVision.transform.position, -Vector3.forward, Vector3.up, 360f, _coneOfVision.VisionRadius);

            // draws two lines to mark the vision angle
            Vector3 visionAngleLeft = MMMaths.DirectionFromAngle2D(-_coneOfVision.VisionAngle / 2f, _coneOfVision.EulerAngles.y);
            Vector3 visionAngleRight = MMMaths.DirectionFromAngle2D(_coneOfVision.VisionAngle / 2f, _coneOfVision.EulerAngles.y);

            Handles.DrawLine(_coneOfVision.transform.position, _coneOfVision.transform.position + visionAngleLeft * _coneOfVision.VisionRadius);
            Handles.DrawLine(_coneOfVision.transform.position, _coneOfVision.transform.position + visionAngleRight * _coneOfVision.VisionRadius);

            foreach (Transform visibleTarget in _coneOfVision.VisibleTargets)
            {
                Handles.color = MMColors.Orange;
                Handles.DrawLine(_coneOfVision.transform.position, visibleTarget.position);
            }
        }
    }
}
