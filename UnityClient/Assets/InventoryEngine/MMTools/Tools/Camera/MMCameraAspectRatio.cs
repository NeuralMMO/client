using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	[RequireComponent(typeof(Camera))]
	/// <summary>
	/// Forces an aspect ratio on a camera
	/// </summary>
	public class MMCameraAspectRatio : MonoBehaviour 
	{
		/// the desired aspect ratio
		public Vector2 AspectRatio = Vector2.zero;

		protected Camera _camera;

		/// <summary>
		/// On Start, we run the Initialization method
		/// </summary>
		protected virtual void Start () 
		{
			Initialization ();
		}

		/// <summary>
		/// Checks the presence of a camera component, and applies the aspect ratio to it
		/// </summary>
		protected virtual void Initialization()
		{
			if (AspectRatio == Vector2.zero)
			{
				return;
			}

			_camera = this.gameObject.GetComponent<Camera> ();
			if (_camera == null)
			{
				return;
			}

			float newAspectRatio = AspectRatio.x / AspectRatio.y;
			float currentWindowAspectRatio = (float)Screen.width / (float)Screen.height;
			float newScaleHeight = currentWindowAspectRatio / newAspectRatio;

			if (newScaleHeight >= 1.0f)
			{  
				float scalewidth = 1.0f / newScaleHeight;
				Rect rectangle = _camera.rect;
				rectangle.width = scalewidth;
				rectangle.height = 1.0f;
				rectangle.x = (1.0f - scalewidth) / 2.0f;
				rectangle.y = 0;
				_camera.rect = rectangle;
			}
			else 
			{
				Rect rectangle = _camera.rect;
				rectangle.width = 1.0f;
				rectangle.height = newScaleHeight;
				rectangle.x = 0;
				rectangle.y = (1.0f - newScaleHeight) / 2.0f;
				_camera.rect = rectangle;
			}
		}
		
	}
}
