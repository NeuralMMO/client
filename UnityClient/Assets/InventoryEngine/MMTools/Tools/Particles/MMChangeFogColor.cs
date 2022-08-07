using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	[ExecuteAlways]
	/// <summary>
	/// Adds this class to a UnityStandardAssets.ImageEffects.GlobalFog to change its color
	/// Why this is not native, I don't know.
	/// </summary>
	public class MMChangeFogColor : MonoBehaviour 
	{
		/// Adds this class to a UnityStandardAssets.ImageEffects.GlobalFog to change its color
		[MMInformation("Adds this class to a UnityStandardAssets.ImageEffects.GlobalFog to change its color", MMInformationAttribute.InformationType.Info,false)]
		public Color FogColor;

		/// <summary>
		/// Sets the fog's color to the one set in the inspector
		/// </summary>
		protected virtual void SetupFogColor () 
		{
			RenderSettings.fogColor = FogColor;
	        RenderSettings.fog = true;
		}

		/// <summary>
		/// On Start(), we set the fog's color
		/// </summary>
		protected virtual void Start()
		{
			SetupFogColor();
		}

		/// <summary>
		/// Whenever there's a change in the camera's inspector, we change the fog's color
		/// </summary>
		protected virtual void OnValidate()
		{
			SetupFogColor();
		}
	}
}