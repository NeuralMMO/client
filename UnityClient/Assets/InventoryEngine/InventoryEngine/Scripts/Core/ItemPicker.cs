using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{	
	/// <summary>
	/// Add this component to an object so it can be picked and added to an inventory
	/// </summary>
	public class ItemPicker : MonoBehaviour 
	{
		/// the item that should be picked 
		[MMInformation("Add this component to a Trigger box collider 2D and it'll make it pickable, and will add the specified item to its target inventory. Just drag a previously created item into the slot below. For more about how to create items, have a look at the documentation. Here you can also specify how many of that item should be picked when picking the object.",MMInformationAttribute.InformationType.Info,false)]
		public InventoryItem Item ;
		[Header("Pick Quantity")]
		/// the quantity of that item that should be added to the inventory when picked
		public int Quantity = 1;
		[Header("Conditions")]
		/// if you set this to true, a character will be able to pick this item even if its inventory is full
		public bool PickableIfInventoryIsFull = false;
		/// if you set this to true, a character will pick as much from this picker as possible, leaving the rest for later
		public bool PickAsMuchQuantityAsPossible = false;
		/// if you set this to true, the object will be disabled when picked
		public bool DisableObjectWhenDepleted = false;

		protected int _pickedQuantity = 0;

		protected Inventory _targetInventory;

		/// <summary>
		/// On Start we initialize our item picker
		/// </summary>
		protected virtual void Start()
		{
			Initialization ();
		}

		/// <summary>
		/// On Init we look for our target inventory
		/// </summary>
		protected virtual void Initialization()
		{
			FindTargetInventory (Item.TargetInventoryName);
		}
        
        /// <summary>
        /// Triggered when something collides with the picker
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter(Collider collider)
        {
            // if what's colliding with the picker ain't a characterBehavior, we do nothing and exit
            if (!collider.CompareTag("Player"))
            {
                return;
            }

            Pick(Item.TargetInventoryName);
        }

        /// <summary>
        /// Triggered when something collides with the picker
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
            // if what's colliding with the picker ain't a characterBehavior, we do nothing and exit
            if (!collider.CompareTag("Player"))
			{
				return;
			}

			Pick(Item.TargetInventoryName);
		}		

		/// <summary>
		/// Picks this item and adds it to its target inventory
		/// </summary>
		public virtual void Pick()
		{
			Pick(Item.TargetInventoryName);
		}

		/// <summary>
		/// Picks this item and adds it to the target inventory specified as a parameter
		/// </summary>
		/// <param name="targetInventoryName">Target inventory name.</param>
		public virtual void Pick(string targetInventoryName)
		{
			FindTargetInventory (targetInventoryName);
			if (_targetInventory==null)
			{
				return;
			}

			if (!Pickable()) 
			{
				PickFail ();
				return;
			}

			DetermineMaxQuantity ();
			if (!Application.isPlaying)
			{
				_targetInventory.AddItem(Item, 1);
			}				
			else
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, Item.TargetInventoryName, Item, _pickedQuantity, 0);
			}				
			if (Item.Pick())
            {
                Quantity = Quantity - _pickedQuantity;
                PickSuccess();
                DisableObjectIfNeeded();
            }			
		}

		/// <summary>
		/// Describes what happens when the object is successfully picked
		/// </summary>
		protected virtual void PickSuccess()
		{
			
		}

		/// <summary>
		/// Describes what happens when the object fails to get picked (inventory full, usually)
		/// </summary>
		protected virtual void PickFail()
		{

		}

		/// <summary>
		/// Disables the object if needed.
		/// </summary>
		protected virtual void DisableObjectIfNeeded()
		{
			// we desactivate the gameobject
			if (DisableObjectWhenDepleted && Quantity <= 0)
			{
				gameObject.SetActive(false);	
			}
		}

		/// <summary>
		/// Determines the max quantity of item that can be picked from this
		/// </summary>
		protected virtual void DetermineMaxQuantity()
		{
			_pickedQuantity = _targetInventory.NumberOfStackableSlots (Item.ItemID, Item.MaximumStack);
			if (Quantity < _pickedQuantity)
			{
				_pickedQuantity = Quantity;
			}
		}

		/// <summary>
		/// Returns true if this item can be picked, false otherwise
		/// </summary>
		public virtual bool Pickable()
		{
			if (!PickableIfInventoryIsFull && _targetInventory.NumberOfFreeSlots == 0)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Finds the target inventory based on its name
		/// </summary>
		/// <param name="targetInventoryName">Target inventory name.</param>
		public virtual void FindTargetInventory(string targetInventoryName)
		{
			_targetInventory = null;
			if (targetInventoryName==null)
			{
				return;
			}
			foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
			{				
				if (inventory.name==targetInventoryName)
				{
					_targetInventory = inventory;
				}
			}
		}
	}
}