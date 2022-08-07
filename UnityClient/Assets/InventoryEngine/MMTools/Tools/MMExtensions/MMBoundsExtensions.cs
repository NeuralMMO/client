using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Bounds helpers
	/// </summary>
	public class MMBoundsExtensions : MonoBehaviour 
	{
        /// <summary>
        /// Gets collider bounds for an object (from Collider2D)
        /// </summary>
        /// <param name="theObject"></param>
        /// <returns></returns>
		public static Bounds GetColliderBounds(GameObject theObject)
	    {
	    	Bounds returnBounds;

			// if the object has a collider at root level, we base our calculations on that
			if (theObject.GetComponent<Collider>()!=null)
	    	{
				returnBounds = theObject.GetComponent<Collider>().bounds;
				return returnBounds;
	    	}

			// if the object has a collider2D at root level, we base our calculations on that
			if (theObject.GetComponent<Collider2D>()!=null) 
			{
				returnBounds = theObject.GetComponent<Collider2D>().bounds;
				return returnBounds;
			}

			// if the object contains at least one Collider we'll add all its children's Colliders bounds
			if (theObject.GetComponentInChildren<Collider>()!=null)
			{
				Bounds totalBounds = theObject.GetComponentInChildren<Collider>().bounds;
				Collider[] colliders = theObject.GetComponentsInChildren<Collider>();
				foreach (Collider col in colliders) 
				{
					totalBounds.Encapsulate(col.bounds);
				}
				returnBounds = totalBounds;
				return returnBounds;
			}

			// if the object contains at least one Collider2D we'll add all its children's Collider2Ds bounds
			if (theObject.GetComponentInChildren<Collider2D>()!=null)
			{
				Bounds totalBounds = theObject.GetComponentInChildren<Collider2D>().bounds;
				Collider2D[] colliders = theObject.GetComponentsInChildren<Collider2D>();
				foreach (Collider2D col in colliders) 
				{
					totalBounds.Encapsulate(col.bounds);
				}
				returnBounds = totalBounds;
				return returnBounds;
			}

			returnBounds = new Bounds(Vector3.zero, Vector3.zero);
			return returnBounds;
		}

        /// <summary>
        /// Gets bounds of a renderer
        /// </summary>
        /// <param name="theObject"></param>
        /// <returns></returns>
		public static Bounds GetRendererBounds(GameObject theObject)
	    {
	    	Bounds returnBounds;

			// if the object has a renderer at root level, we base our calculations on that
			if (theObject.GetComponent<Renderer>()!=null)
	    	{
				returnBounds = theObject.GetComponent<Renderer>().bounds;
				return returnBounds;
	    	}

			// if the object contains at least one renderer we'll add all its children's renderer bounds
			if (theObject.GetComponentInChildren<Renderer>()!=null)
			{
				Bounds totalBounds = theObject.GetComponentInChildren<Renderer>().bounds;
				Renderer[] renderers = theObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in renderers) 
				{
					totalBounds.Encapsulate(renderer.bounds);
				}
				returnBounds = totalBounds;
				return returnBounds;
			}

			returnBounds = new Bounds(Vector3.zero, Vector3.zero);
			return returnBounds;
		}
	}
}
