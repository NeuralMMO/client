using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A class to store ratio display info
    /// </summary>
    [Serializable]    
    public class Ratio
    {
        /// whether or not that ratio should be drawn
        public bool DrawRatio = true;
        /// the ratio's size (4:3, 16:9, etc)
        public Vector2 Size;
        /// the color of the handle to draw
        public Color RatioColor;

        public Ratio(bool drawRatio, Vector2 size, Color ratioColor)
        {
            DrawRatio = drawRatio;
            Size = size;
            RatioColor = ratioColor;
        }
    }

    /// <summary>
    /// A class to handle the automatic display of safe zones for the different ratios setup in the inspector
    /// </summary>
    public class MMAspectRatioSafeZones : MonoBehaviour
    {
        [Header("Center")]
        /// whether or not to draw the center crosshair
        public bool DrawCenterCrosshair = true;
        /// the size of the center crosshair
        public float CenterCrosshairSize = 1f;
        /// the color of the center crosshair
        public Color CenterCrosshairColor = MMColors.Wheat;

        [Header("Ratios")]
        /// whether or not to draw any ratio
        public bool DrawRatios = true;
        /// the size of the projected ratios
        public float CameraSize = 5f;
        /// the opacity to apply to the dead zones
        public float UnsafeZonesOpacity = 0.2f;
        /// the list of ratios to draw
        public List<Ratio> Ratios;

        [MMInspectorButton("AutoSetup")]
        public bool AutoSetupButton;

        public virtual void AutoSetup()
        {
            Ratios.Clear();
            Ratios.Add(new Ratio(true, new Vector2(16, 9), MMColors.DeepSkyBlue));
            Ratios.Add(new Ratio(true, new Vector2(16, 10), MMColors.GreenYellow));
            Ratios.Add(new Ratio(true, new Vector2(4, 3), MMColors.HotPink));

        }
    }
}
