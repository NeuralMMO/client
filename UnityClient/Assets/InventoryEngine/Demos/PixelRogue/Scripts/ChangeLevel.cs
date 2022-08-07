using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{	
	/// <summary>
	/// Demo class to go from one level to another
	/// </summary>
	public class ChangeLevel : MonoBehaviour 
	{
		/// <summary>
		/// The name of the level to go to when entering the ChangeLevel zone
		/// </summary>
		[MMInformation("This demo component, when added to a BoxCollider2D, will change the scene to the one specified in the field below when the character enters the collider.", MMInformationAttribute.InformationType.Info,false)]
		public string Destination;

		/// <summary>
		/// When a character enters the ChangeLevel zone, we trigger a general save and then load the destination scene
		/// </summary>
		/// <param name="collider">Collider.</param>
		public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			if ((Destination != null) && (collider.gameObject.GetComponent<InventoryDemoCharacter>() != null))
			{
				MMGameEvent.Trigger("Save");
				SceneManager.LoadScene(Destination);
			}
		}
	}
}