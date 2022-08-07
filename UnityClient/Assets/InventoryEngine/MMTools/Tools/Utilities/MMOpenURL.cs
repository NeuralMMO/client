using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to open a URL specified in its inspector
	/// </summary>
	public class MMOpenURL : MonoBehaviour 
	{
		/// the URL to open when calling OpenURL()
		public string DestinationURL;

		/// <summary>
		/// Opens the URL specified in the DestinationURL field
		/// </summary>
		public virtual void OpenURL()
		{
			Application.OpenURL(DestinationURL);
		}		
	}
}
