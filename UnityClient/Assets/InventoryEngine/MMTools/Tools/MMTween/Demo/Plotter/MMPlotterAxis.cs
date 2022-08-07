using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace MoreMountains.Tools
{
    public class MMPlotterAxis : MonoBehaviour
    {
        public Text Label;
        public Text TimeLabel;
        public Transform PlotterCurvePoint;

        public Transform PositionPoint;
        public Transform PositionPointVertical;
        public Transform RotationPoint;
        public Transform ScalePoint;
        
        public virtual void SetLabel(string newLabel)
        {
            Label.text = newLabel;
        }
    }
}
