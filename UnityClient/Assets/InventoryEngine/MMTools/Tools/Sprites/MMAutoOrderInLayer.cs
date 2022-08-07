using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	[RequireComponent(typeof(SpriteRenderer))]
	/// <summary>
	/// Add this component to an object to have it pick a new order in layer on start, useful to have unique sorting layer numbers
	/// </summary>
	public class MMAutoOrderInLayer : MonoBehaviour 
	{
		static int CurrentMaxCharacterOrderInLayer = 0;

		[Header("Global Counter")]
		[MMInformation("Add this component to an object with a sprite renderer, and it'll give it a new order in layer based on the settings defined here. First is the global counter increment, or how much you'd like to increment the layer order between two objects on that same layer.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the number by which to increment each new object's order in layer
		public int GlobalCounterIncrement = 5;

		[Header("Parent")]
		[MMInformation("You can also decide to determine the new layer order based on the parent sprite's order (it'll have to be on the same layer).",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// if this is true, the new order in layer value will be based on the highest order value found on a parent with a similar sorting layer
		public bool BasedOnParentOrder = false;
		/// if BasedOnParentOrder is true, the new value will be the parent's order value + this value
		public int ParentIncrement = 1;

		[Header("Children")]
		[MMInformation("And here you can decide to apply your new layer order to all children.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// if this is true, the new order value will be passed to all children with a similar sorting layer
		public bool ApplyNewOrderToChildren = false;
		/// the value by which the new order value should be incremented to pass it to children
		public int ChildrenIncrement = 0;

		protected SpriteRenderer _spriteRenderer;

		/// <summary>
		/// On Start, we get our sprite renderer and determine the new order in layer
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
			AutomateLayerOrder();
		}

		/// <summary>
		/// Gets the sprite renderer component and stores it
		/// </summary>
		protected virtual void Initialization()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
		}

		/// <summary>
		/// Picks a new order in layer based on the inspector's settings
		/// </summary>
		protected virtual void AutomateLayerOrder()
		{
			int newOrder = 0;

			// if there's no sprite renderer on this object, we do nothing and exit
			if (_spriteRenderer == null)
			{
				return;
			}

			// if we're supposed to base our new order in layer value on the parent's value
			if (BasedOnParentOrder)
			{
				int maxLayerOrder = 0;
				Component[] spriteRenderers = GetComponentsInParent( typeof(SpriteRenderer) );

				// we look for all sprite renderers in parent objects
		        if( spriteRenderers != null )
		        {
					foreach( SpriteRenderer spriteRenderer in spriteRenderers )
		            {
		            	// if we find a parent with a sprite renderer, on the same sorting layer and with a higher sorting value than previously found
						if ( (spriteRenderer.sortingLayerID == _spriteRenderer.sortingLayerID)
							&& (spriteRenderer.sortingOrder > maxLayerOrder))
						{							
							// we store the new value
							maxLayerOrder = spriteRenderer.sortingOrder;							
						}
		            }	
		            // we set our new value to the highest value found, plus our increment
		            newOrder = maxLayerOrder + ParentIncrement;                
		        }
			}
			else
			{
				// if we're not based on parent, we base our pick on the current max order in layer
				newOrder = CurrentMaxCharacterOrderInLayer + GlobalCounterIncrement;
				// we increment the global order index
				CurrentMaxCharacterOrderInLayer += GlobalCounterIncrement;
			}

			// we apply our new order value
			_spriteRenderer.sortingOrder = newOrder;

			// if we need to apply that new value to all children, we do it
			if (ApplyNewOrderToChildren)
			{
				Component[] childrenSpriteRenderers = GetComponentsInChildren( typeof(SpriteRenderer) );
				if( childrenSpriteRenderers != null )
		        {
					foreach( SpriteRenderer childSpriteRenderer in childrenSpriteRenderers )
		            {
						if (childSpriteRenderer.sortingLayerID == _spriteRenderer.sortingLayerID)
						{
							childSpriteRenderer.sortingOrder = newOrder + ChildrenIncrement;
						}
		            }	              
		        }
			}
		}
	}
}
