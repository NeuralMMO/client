using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{	
	public class MMObjectBounds : MonoBehaviour
	{
		public enum WaysToDetermineBounds { Collider, Collider2D, Renderer, Undefined }

        [Header("Bounds")]
        public WaysToDetermineBounds BoundsBasedOn;  


		public Vector3 Size { get; set; }

		/// <summary>
		/// When this component is added we define its bounds.
		/// </summary>
		protected virtual void Reset() 
		{
			DefineBoundsChoice();
   		}

		/// <summary>
		/// Tries to determine automatically what the bounds should be based on.
		/// In this order, it'll keep the last found of these : Collider2D, Collider or Renderer.
		/// If none of these is found, it'll be set as Undefined.
		/// </summary>
		protected virtual void DefineBoundsChoice()
   		{
   			BoundsBasedOn = WaysToDetermineBounds.Undefined;
			if (GetComponent<Renderer>()!=null)
			{
				BoundsBasedOn = WaysToDetermineBounds.Renderer;
			}
			if (GetComponent<Collider>()!=null)
			{
				BoundsBasedOn = WaysToDetermineBounds.Collider;
			}
			if (GetComponent<Collider2D>()!=null)
			{
				BoundsBasedOn = WaysToDetermineBounds.Collider2D;
			}
   		}

   		/// <summary>
   		/// Returns the bounds of the object, based on what has been defined
   		/// </summary>
   		public virtual Bounds GetBounds()
		{
			if (BoundsBasedOn==WaysToDetermineBounds.Renderer)
			{
				if (GetComponent<Renderer>()==null)
				{
					throw new Exception("The PoolableObject "+gameObject.name+" is set as having Renderer based bounds but no Renderer component can be found.");
				}
				return GetComponent<Renderer>().bounds;
			}

			if (BoundsBasedOn==WaysToDetermineBounds.Collider)
			{
				if (GetComponent<Collider>()==null)
				{
					throw new Exception("The PoolableObject "+gameObject.name+" is set as having Collider based bounds but no Collider component can be found.");
				}
				return GetComponent<Collider>().bounds;				
			}

			if (BoundsBasedOn==WaysToDetermineBounds.Collider2D)
			{
				if (GetComponent<Collider2D>()==null)
				{
					throw new Exception("The PoolableObject "+gameObject.name+" is set as having Collider2D based bounds but no Collider2D component can be found.");
				}
				return GetComponent<Collider2D>().bounds;				
			}

			return new Bounds(Vector3.zero,Vector3.zero);
   		}



	}
}
