using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MoreMountains.InventoryEngine
{	
	/// <summary>
	/// This class handles the display of the items in an inventory and will trigger the various things you can do with an item (equip, use, etc.)
	/// </summary>
	public class InventorySlot : Button 
	{
		[MMInformation("Inventory slots are used inside an InventoryDisplay to present the content of each inventory slot. It's best to not touch these directly but rather make changes from the InventoryDisplay's inspector.",MMInformationAttribute.InformationType.Info,false)]
		/// the sprite used as a background for the slot while an item is being moved
		public Sprite MovedSprite;
		/// the inventory display this slot belongs to
		public InventoryDisplay ParentInventoryDisplay;
		/// the slot's index (its position in the inventory array)
		public int Index;
		/// whether or not this slot is currently enabled and can be interacted with
		public bool SlotEnabled=true;

		protected const float _disabledAlpha = 0.5f;
		protected const float _enabledAlpha = 1.0f;

		/// <summary>
		/// On Start, we start listening to click events on that slot
		/// </summary>
		protected override void Start()
		{
			base.Start();
			this.onClick.AddListener(SlotClicked);
		}

		/// <summary>
		/// If there's an item in this slot, draws its icon inside.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="index">Index.</param>
		public virtual void DrawIcon(InventoryItem item, int index)
		{
			if (ParentInventoryDisplay!=null)
			{				
				if (!InventoryItem.IsNull(item))
				{
					GameObject itemIcon = new GameObject("Icon", typeof(RectTransform));
					itemIcon.transform.SetParent(this.transform);
					UnityEngine.UI.Image itemIconImage = itemIcon.AddComponent<Image>();
					itemIconImage.sprite = item.Icon;
					RectTransform itemRectTransform = itemIcon.GetComponent<RectTransform>();
					itemRectTransform.localPosition=Vector3.zero;
					itemRectTransform.localScale=Vector3.one;
					MMGUI.SetSize(itemRectTransform, ParentInventoryDisplay.IconSize);

					// if there's more than one of this item in this slot, we draw the associated quantity
					if (item.Quantity>1)
					{
						GameObject textObject = new GameObject("Slot "+index+" Quantity", typeof(RectTransform));
						textObject.transform.SetParent(this.transform);
						Text textComponent = textObject.AddComponent<Text>();
						textComponent.text=item.Quantity.ToString();
						textComponent.font=ParentInventoryDisplay.QtyFont;
						textComponent.fontSize=ParentInventoryDisplay.QtyFontSize;
						textComponent.color=ParentInventoryDisplay.QtyColor;
						textComponent.alignment=ParentInventoryDisplay.QtyAlignment;
						RectTransform textObjectRectTransform = textObject.GetComponent<RectTransform>();
						textObjectRectTransform.localPosition=Vector3.zero;
						textObjectRectTransform.localScale=Vector3.one;
						MMGUI.SetSize(textObjectRectTransform, (ParentInventoryDisplay.SlotSize - Vector2.one * ParentInventoryDisplay.QtyPadding)); 
					}
				}
			}
		}

		/// <summary>
		/// When that slot gets selected (via a mouse over or a touch), triggers an event for other classes to act on
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			if (ParentInventoryDisplay!=null)
			{
				InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
				MMInventoryEvent.Trigger(MMInventoryEventType.Select, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index);
			}
		}

		/// <summary>
		/// When that slot gets clicked, triggers an event for other classes to act on
		/// </summary>
		public virtual void SlotClicked () 
		{
			if (ParentInventoryDisplay!=null)
			{
				InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
				MMInventoryEvent.Trigger(MMInventoryEventType.Click, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index);
				// if we're currently moving an object
				if (ParentInventoryDisplay.CurrentlyBeingMovedItemIndex!=-1)
				{
					Move();
				}
			}
		}

		/// <summary>
		/// Selects the item in this slot for a movement, or moves the currently selected one to that slot
		/// This will also swap both objects if possible
		/// </summary>
		public virtual void Move()
		{
			if (!SlotEnabled) { return; }
			// if we're not already moving an object
			if (ParentInventoryDisplay.CurrentlyBeingMovedItemIndex==-1)
			{
				// if the slot we're on is empty, we do nothing
				if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index);
					return;
				}
				if (ParentInventoryDisplay.TargetInventory.Content[Index].CanMoveObject)
				{
					// we change the background image
					GetComponent<Image>().sprite = ParentInventoryDisplay.MovedSlotImage;
					ParentInventoryDisplay.CurrentlyBeingMovedItemIndex=Index;
				}
			}
			// if we ARE moving an object
			else
			{
				// we move the object to a new slot. 
				if (!ParentInventoryDisplay.TargetInventory.MoveItem(ParentInventoryDisplay.CurrentlyBeingMovedItemIndex,Index))
				{
					// if the move couldn't be made (non empty destination slot for example), we play a sound
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index);
				}
				else
				{
					// if the move could be made, we reset our currentlyBeingMoved pointer
					ParentInventoryDisplay.CurrentlyBeingMovedItemIndex=-1;
					MMInventoryEvent.Trigger(MMInventoryEventType.Move, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
				}
			}
		}

		/// <summary>
		/// Consume one unity of the item in this slot, triggering a sound and whatever behaviour has been defined for this item being used
		/// </summary>
		public virtual void Use()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.UseRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
		}

		/// <summary>
		/// Equip this item if possible.
		/// </summary>
		public virtual void Equip()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
		}

		/// <summary>
		/// Unequip this item if possible.
		/// </summary>
		public virtual void UnEquip()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
		}

		/// <summary>
		/// Drops this item.
		/// </summary>
		public virtual void Drop()
		{
			if (!SlotEnabled) { return; }
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index);
				return;
			}
            if (ParentInventoryDisplay.TargetInventory.Content[Index].Drop())
            {
                ParentInventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
                MMInventoryEvent.Trigger(MMInventoryEventType.Drop, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
            }            
		}

		/// <summary>
		/// Disables the slot.
		/// </summary>
		public virtual void DisableSlot()
		{
			this.interactable=false;
			SlotEnabled=false;
			GetComponent<CanvasGroup> ().alpha = _disabledAlpha;
		}

		/// <summary>
		/// Enables the slot.
		/// </summary>
		public virtual void EnableSlot()
		{
			this.interactable=true;
			SlotEnabled=true;
			GetComponent<CanvasGroup> ().alpha = _enabledAlpha;
		}

		/// <summary>
		/// Returns true if the item at this slot can be equipped, false otherwise
		/// </summary>
		public virtual bool Equippable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsEquippable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Returns true if the item at this slot can be used, false otherwise
		/// </summary>
		public virtual bool Usable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsUsable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}		
	}
}