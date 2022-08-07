using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this class to a radial image and it'll allow you to control its fill amount
	/// </summary>
	public class MMRadialProgressBar : MonoBehaviour 
	{
		/// the start fill amount value 
		public float StartValue = 1f;
		/// the end goad fill amount value
		public float EndValue = 0f;
		/// the distance to the start or end value at which the class should start lerping
		public float Tolerance = 0.01f;
        /// optional - the ID of the player associated to this bar
        public string PlayerID;

        protected Image _radialImage;
		protected float _newPercent;

		/// <summary>
		/// On awake we grab our Image component
		/// </summary>
		protected virtual void Awake()
		{
			_radialImage = GetComponent<Image>();
		}

		/// <summary>
		/// Call this method to update the fill amount based on a currentValue between minValue and maxValue
		/// </summary>
		/// <param name="currentValue">Current value.</param>
		/// <param name="minValue">Minimum value.</param>
		/// <param name="maxValue">Max value.</param>
		public virtual void UpdateBar(float currentValue,float minValue,float maxValue)
		{
			_newPercent = MMMaths.Remap(currentValue,minValue,maxValue,StartValue,EndValue);
			if (_radialImage == null) { return; }
			_radialImage.fillAmount = _newPercent;
			if (_radialImage.fillAmount > 1 - Tolerance)
			{
				_radialImage.fillAmount = 1;
			}
			if (_radialImage.fillAmount < Tolerance)
			{
				_radialImage.fillAmount = 0;
			}

		}
	}
}
