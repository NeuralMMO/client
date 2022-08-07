using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
	/// <summary>
	/// An example of a game manager, the only significant part being how we trigger in a single place the load of all inventories, in the Start method.
	/// </summary>
	public class InventoryDemoGameManager : MMSingleton<InventoryDemoGameManager> 
	{
		public InventoryDemoCharacter Player { get; protected set; }

		protected override void Awake () 
		{
			base.Awake ();
			Player = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryDemoCharacter>()	;
		}

		/// <summary>
		/// On start, we trigger our load event, which will be caught by inventories so they try to load saved content
		/// </summary>
		protected virtual void Start()
		{
            MMGameEvent.Trigger("Load");
		}
	}
}