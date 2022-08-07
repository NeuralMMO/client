using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "BaseItem", menuName = "MoreMountains/InventoryEngine/BaseItem", order = 0)]
	[Serializable]
	/// <summary>
	/// Base item class, to use when your object doesn't do anything special
	/// </summary>
	public class BaseItem : InventoryItem 
	{
				
	}
}