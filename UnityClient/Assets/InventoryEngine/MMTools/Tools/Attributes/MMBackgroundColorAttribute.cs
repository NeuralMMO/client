using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
    public enum MMBackgroundAttributeColor
    {
        Red,
        Pink,
        Orange,
        Yellow,
        Green,
        Blue,
        Violet,
        White
    }

    public class MMBackgroundColorAttribute : PropertyAttribute
    {
        public MMBackgroundAttributeColor Color;

        public MMBackgroundColorAttribute(MMBackgroundAttributeColor color = MMBackgroundAttributeColor.Yellow)
        {
            this.Color = color;
        }
    }
}
