using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	public class MMLayers  
	{
		public static bool LayerInLayerMask(int layer, LayerMask layerMask)
		{
			if(((1 << layer) & layerMask) != 0)	
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
	}
}