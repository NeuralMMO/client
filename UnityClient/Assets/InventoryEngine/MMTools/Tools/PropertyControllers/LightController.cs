using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A class used to control the intensity of a light
    /// </summary>
    public class LightController : MonoBehaviour
    {
        [Header("Binding")]
        [MMInformation("Use this component to control the properties of one or more lights at runtime. Plays well with a FloatController. " +
            "This component will try to auto set the TargetLight if there's a Light component on this object.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// the light to control 
        public Light TargetLight;
        /// the lights to control
        public List<Light> TargetLights;

        [Header("Light Settings")]
        /// the new intensity
        public float Intensity = 1f;
        /// the multiplier to apply
        public float Multiplier = 1f;
        /// the new range
        public float Range = 1f;

        [Header("Color")]
        /// the new color
        public Color LightColor;
        
        /// <summary>
        /// On Start, we initialize our light
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Grabs the light, sets initial range and color
        /// </summary>
        protected virtual void Initialization()
        {
            if (TargetLight == null)
            {
                TargetLight = this.gameObject.GetComponent<Light>();
            }

            if (TargetLight != null)
            {
                TargetLight.range = Range;
                TargetLight.color = LightColor;
            }

            if (TargetLights.Count > 0)
            {
                foreach (Light light in TargetLights)
                {
                    if (light != null)
                    {
                        light.range = Range;
                        light.color = LightColor;
                    }
                }
            }
        }

        /// <summary>
        /// On Update we apply our light settings
        /// </summary>
        protected virtual void Update()
        {
            ApplyLightSettings();           
        }

        /// <summary>
        /// Applys the new intensity, range and color to the light
        /// </summary>
        protected virtual void ApplyLightSettings()
        {
            if (TargetLight != null)
            {
                TargetLight.intensity = Intensity * Multiplier;
            }

            if (TargetLights.Count > 0)
            {
                foreach (Light light in TargetLights)
                {
                    if (light != null)
                    {
                        light.intensity = Intensity * Multiplier;
                    }
                }
            }
        }
    }
}
