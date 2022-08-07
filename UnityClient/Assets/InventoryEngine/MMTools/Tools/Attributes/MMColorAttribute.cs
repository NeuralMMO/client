using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
    public class MMColorAttribute : PropertyAttribute
    {
        public Color color;

        public MMColorAttribute(float red = 1, float green = 0, float blue = 0)
        {
            this.color = new Color(red, green, blue, 1);
        }
    }
}
