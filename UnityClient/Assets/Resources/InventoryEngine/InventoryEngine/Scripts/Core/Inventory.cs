using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.InventoryEngine
{
    [Serializable]
    /// <summary>
    /// Base inventory class. 
    /// Will handle storing items, saving and loading its content, adding items to it, removing items, equipping them, etc.
    /// </summary>
    public class Inventory : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
    {
        /// The different possible inventory types, main are regular, equipment will have special behaviours (use them for slots where you put the equipped weapon/armor/etc).
        public enum InventoryTypes { Main, Equipment }

        [Header("Debug")]
        /// If true, will draw the contents of the inventory in its inspector
        [MMInformation("The Inventory component is like the database and controller part of your inventory. It won't show anything on screen, you'll need also an InventoryDisplay for that. Here you can decide whether or not you want to output a debug content in the inspector (useful for debugging).", MMInformationAttribute.InformationType.Info, false)]
        public bool DrawContentInInspector = false;

        /// the complete list of inventory items in this inventory
        [MMInformation("This is a realtime view of your Inventory's contents. Don't modify this list via the inspector, it's visible for control purposes only.", MMInformationAttribute.InformationType.Info, false)]
        public InventoryItem[] Content;

        [Header("Inventory Type")]
        /// whether this inventory is a main inventory or equipment one
        [MMInformation("Here you can define your inventory's type. Main are 'regular' inventories. Equipment inventories will be bound to a certain item class and have dedicated options.", MMInformationAttribute.InformationType.Info, false)]
        public InventoryTypes InventoryType = InventoryTypes.Main;

        [Header("Target Transform")]
        [MMInformation("The TargetTransform is any transform in your scene at which objects dropped from the inventory will spawn.", MMInformationAttribute.InformationType.Info, false)]
        /// the transform at which objects will be spawned when dropped
        public Transform TargetTransform;

        [Header("Persistency")]
        [MMInformation("Here you can define whether or not this inventory should respond to Load and Save events. If you don't want to have your inventory saved to disk, set this to false. You can also have it reset on start, to make sure it's always empty at the start of this level.", MMInformationAttribute.InformationType.Info, false)]
        /// whether this inventory will be saved and loaded
        public bool Persistent = true;
        /// whether or not this inventory should be reset on start
        public bool ResetThisInventorySaveOnStart = false;

        /// the owner of the inventory (for games where you have multiple characters)
        public GameObject Owner { get; set; }

        /// The number of free slots in this inventory
        public int NumberOfFreeSlots { get { return Content.Length - NumberOfFilledSlots; } }

        /// The number of filled slots 
        public int NumberOfFilledSlots
        {
            get
            {
                int numberOfFilledSlots = 0;
                for (int i = 0; i < Content.Length; i++)
                {
                    if (!InventoryItem.IsNull(Content[i]))
                    {
                        numberOfFilledSlots++;
                    }
                }
                return numberOfFilledSlots;
            }
        }

        public int NumberOfStackableSlots(string searchedName, int maxStackSize)
        {
            int numberOfStackableSlots = 0;
            int i = 0;

            while (i < Content.Length)
            {
                if (InventoryItem.IsNull(Content[i]))
                {
                    numberOfStackableSlots += maxStackSize;
                }
                else
                {
                    if (Content[i].ItemID == searchedName)
                    {
                        numberOfStackableSlots += maxStackSize - Content[i].Quantity;
                    }
                }
                i++;
            }

            return numberOfStackableSlots;
        }

        public const string _resourceItemPath = "Items/";
        protected const string _saveFolderName = "InventoryEngine/";
        protected const string _saveFileExtension = ".inventory";

        /// <summary>
        /// Sets the owner of this inventory, useful to apply the effect of an item for example.
        /// </summary>
        /// <param name="newOwner">New owner.</param>
        public virtual void SetOwner(GameObject newOwner)
        {
            Owner = newOwner;
        }

        /// <summary>
        /// Tries to add an item of the specified type. Note that this is name based.
        /// </summary>
        /// <returns><c>true</c>, if item was added, <c>false</c> if it couldn't be added (item null, inventory full).</returns>
        /// <param name="itemToAdd">Item to add.</param>
        public virtual bool AddItem(InventoryItem itemToAdd, int quantity)
        {
            // if the item to add is null, we do nothing and exit
            if (itemToAdd == null)
            {
                Debug.LogWarning(this.name + " : The item you want to add to the inventory is null");
                return false;
            }

            List<int> list = InventoryContains(itemToAdd.ItemID);
            // if there's at least one item like this already in the inventory and it's stackable
            if (list.Count > 0 && itemToAdd.MaximumStack > 1)
            {
                // we store items that match the one we want to add
                for (int i = 0; i < list.Count; i++)
                {
                    // if there's still room in one of these items of this kind in the inventory, we add to it
                    if (Content[list[i]].Quantity < itemToAdd.MaximumStack)
                    {
                        // we increase the quantity of our item
                        Content[list[i]].Quantity += quantity;
                        // if this exceeds the maximum stack
                        if (Content[list[i]].Quantity > Content[list[i]].MaximumStack)
                        {
                            InventoryItem restToAdd = itemToAdd;
                            int restToAddQuantity = Content[list[i]].Quantity - Content[list[i]].MaximumStack;
                            // we clamp the quantity and add the rest as a new item
                            Content[list[i]].Quantity = Content[list[i]].MaximumStack;
                            AddItem(restToAdd, restToAddQuantity);
                        }
                        MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                        return true;
                    }
                }
            }
            // if we've reached the max size of our inventory, we don't add the item
            if (NumberOfFilledSlots >= Content.Length)
            {
                return false;
            }
            while (quantity > 0)
            {
                if (quantity > itemToAdd.MaximumStack)
                {
                    AddItem(itemToAdd, itemToAdd.MaximumStack);
                    quantity -= itemToAdd.MaximumStack;
                }
                else
                {
                    AddItemToArray(itemToAdd, quantity);
                    quantity = 0;
                }
            }
            // if we're still here, we add the item in the first available slot
            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
            return true;
        }

        /// <summary>
        /// Tries to move the item at the first parameter slot to the second slot
        /// </summary>
        /// <returns><c>true</c>, if item was moved, <c>false</c> otherwise.</returns>
        /// <param name="startIndex">Start index.</param>
        /// <param name="endIndex">End index.</param>
        public virtual bool MoveItem(int startIndex, int endIndex)
        {
            bool swap = false;
            // if what we're trying to move is null, this means we're trying to move an empty slot
            if (InventoryItem.IsNull(Content[startIndex]))
            {
                Debug.LogWarning("InventoryEngine : you're trying to move an empty slot.");
                return false;
            }
            // if both objects are swappable, we'll swap them
            if (Content[startIndex].CanSwapObject)
            {
                if (!InventoryItem.IsNull(Content[endIndex]))
                {
                    if (Content[endIndex].CanSwapObject)
                    {
                        swap = true;
                    }
                }
            }
            // if the target slot is empty
            if (InventoryItem.IsNull(Content[endIndex]))
            {
                // we create a copy of our item to the destination
                Content[endIndex] = Content[startIndex].Copy();
                // we remove the original
                RemoveItemFromArray(startIndex);
                // we mention that the content has changed and the inventory probably needs a redraw if there's a GUI attached to it
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                return true;
            }
            else
            {
                // if we can swap objects, we'll try and do it, otherwise we return false as the slot we target is not null
                if (swap)
                {
                    // we swap our items
                    InventoryItem tempItem = Content[endIndex].Copy();
                    Content[endIndex] = Content[startIndex].Copy();
                    Content[startIndex] = tempItem;
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes the specified item from the inventory.
        /// </summary>
        /// <returns><c>true</c>, if item was removed, <c>false</c> otherwise.</returns>
        /// <param name="itemToRemove">Item to remove.</param>
        public virtual bool RemoveItem(int i, int quantity)
        {
            Content[i].Quantity -= quantity;
            if (Content[i].Quantity <= 0)
            {
                bool suppressionSuccessful = RemoveItemFromArray(i);
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                return suppressionSuccessful;
            }
            else
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                return true;
            }
        }

        /// <summary>
        /// Destroys the item stored at index i
        /// </summary>
        /// <returns><c>true</c>, if item was destroyed, <c>false</c> otherwise.</returns>
        /// <param name="i">The index.</param>
        public virtual bool DestroyItem(int i)
        {
            Content[i] = null;

            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
            return true;
        }

        /// <summary>
        /// Empties the current state of the inventory.
        /// </summary>
        public virtual void EmptyInventory()
        {
            Content = new InventoryItem[Content.Length];

            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
        }

        /// <summary>
        /// Adds the item to content array.
        /// </summary>
        /// <returns><c>true</c>, if item to array was added, <c>false</c> otherwise.</returns>
        /// <param name="itemToAdd">Item to add.</param>
        /// <param name="quantity">Quantity.</param>
        protected virtual bool AddItemToArray(InventoryItem itemToAdd, int quantity)
        {
            if (NumberOfFreeSlots == 0)
            {
                return false;
            }
            int i = 0;
            while (i < Content.Length)
            {
                if (InventoryItem.IsNull(Content[i]))
                {
                    Content[i] = itemToAdd.Copy();
                    Content[i].Quantity = quantity;
                    return true;
                }
                i++;
            }
            return false;
        }

        /// <summary>
        /// Removes the item at index i from the array.
        /// </summary>
        /// <returns><c>true</c>, if item from array was removed, <c>false</c> otherwise.</returns>
        /// <param name="i">The index.</param>
        protected virtual bool RemoveItemFromArray(int i)
        {
            if (i < Content.Length)
            {
                Content[i].ItemID = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resizes the array to the specified new size
        /// </summary>
        /// <param name="newSize">New size.</param>
        public virtual void ResizeArray(int newSize)
        {
            InventoryItem[] temp = new InventoryItem[newSize];
            for (int i = 0; i < Mathf.Min(newSize, Content.Length); i++)
            {
                temp[i] = Content[i];
            }
            Content = temp;
        }

        /// <summary>
        /// Returns the total quantity of items matching the specified name
        /// </summary>
        /// <returns>The quantity.</returns>
        /// <param name="searchedItem">Searched item.</param>
        public virtual int GetQuantity(string searchedName)
        {
            List<int> list = InventoryContains(searchedName);
            int total = 0;
            foreach (int i in list)
            {
                total += Content[i].Quantity;
            }
            return total;
        }

        /// <summary>
        /// Returns a list of all the items in the inventory that match the specified name
        /// </summary>
        /// <returns>A list of item matching the search criteria.</returns>
        /// <param name="searchedType">The searched type.</param>
        public virtual List<int> InventoryContains(string searchedName)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < Content.Length; i++)
            {
                if (!InventoryItem.IsNull(Content[i]))
                {
                    if (Content[i].ItemID == searchedName)
                    {
                        list.Add(i);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Returns a list of all the items in the inventory that match the specified class
        /// </summary>
        /// <returns>A list of item matching the search criteria.</returns>
        /// <param name="searchedType">The searched type.</param>
        public virtual List<int> InventoryContains(MoreMountains.InventoryEngine.ItemClasses searchedClass)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < Content.Length; i++)
            {
                if (InventoryItem.IsNull(Content[i]))
                {
                    continue;
                }
                if (Content[i].ItemClass == searchedClass)
                {
                    list.Add(i);
                }
            }
            return list;
        }

        /// <summary>
        /// Saves the inventory to a file
        /// </summary>
        public virtual void SaveInventory()
        {
            SerializedInventory serializedInventory = new SerializedInventory();
            FillSerializedInventory(serializedInventory);
            MMSaveLoadManager.Save(serializedInventory, gameObject.name + _saveFileExtension, _saveFolderName);
        }

        /// <summary>
        /// Tries to load the inventory if a file is present
        /// </summary>
        public virtual void LoadSavedInventory()
        {
            SerializedInventory serializedInventory = (SerializedInventory)MMSaveLoadManager.Load(typeof(SerializedInventory), gameObject.name + _saveFileExtension, _saveFolderName);
            ExtractSerializedInventory(serializedInventory);
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryLoaded, null, this.name, null, 0, 0);
        }

        /// <summary>
        /// Fills the serialized inventory for storage
        /// </summary>
        /// <param name="serializedInventory">Serialized inventory.</param>
        protected virtual void FillSerializedInventory(SerializedInventory serializedInventory)
        {
            serializedInventory.InventoryType = InventoryType;
            serializedInventory.DrawContentInInspector = DrawContentInInspector;
            serializedInventory.ContentType = new string[Content.Length];
            serializedInventory.ContentQuantity = new int[Content.Length];
            for (int i = 0; i < Content.Length; i++)
            {
                if (!InventoryItem.IsNull(Content[i]))
                {
                    serializedInventory.ContentType[i] = Content[i].ItemID;
                    serializedInventory.ContentQuantity[i] = Content[i].Quantity;
                }
                else
                {
                    serializedInventory.ContentType[i] = null;
                    serializedInventory.ContentQuantity[i] = 0;
                }
            }
        }

        /// <summary>
        /// Extracts the serialized inventory from a file content
        /// </summary>
        /// <param name="serializedInventory">Serialized inventory.</param>
        protected virtual void ExtractSerializedInventory(SerializedInventory serializedInventory)
        {
            if (serializedInventory == null)
            {
                return;
            }

            InventoryType = serializedInventory.InventoryType;
            DrawContentInInspector = serializedInventory.DrawContentInInspector;
            Content = new InventoryItem[serializedInventory.ContentType.Length];
            for (int i = 0; i < serializedInventory.ContentType.Length; i++)
            {
                if ((serializedInventory.ContentType[i] != null) && (serializedInventory.ContentType[i] != ""))
                {
                    Content[i] = Resources.Load<InventoryItem>(_resourceItemPath + serializedInventory.ContentType[i]).Copy();
                    Content[i].Quantity = serializedInventory.ContentQuantity[i];
                }
                else
                {
                    Content[i] = null;
                }
            }
        }

        /// <summary>
        /// Destroys any save file 
        /// </summary>
        public virtual void ResetSavedInventory()
        {
            MMSaveLoadManager.DeleteSave(gameObject.name + _saveFileExtension, _saveFolderName);
            Debug.LogFormat("save file deleted");
        }

        /// <summary>
        /// Triggers the use and potential consumption of the item passed in parameter. You can also specify the item's slot (optional) and index.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="slot">Slot.</param>
        /// <param name="index">Index.</param>
        public virtual bool UseItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return false;
            }
            if (!item.IsUsable)
            {
                return false;
            }
            if (item.Use())
            {
                // remove 1 from quantity
                RemoveItem(index, 1);
                MMInventoryEvent.Trigger(MMInventoryEventType.ItemUsed, slot, this.name, item, 0, index);
            }

            return true;
        }

        public virtual bool UseItem(string itemName)
        {
            List<int> list = InventoryContains(itemName);
            if (list.Count > 0)
            {
                UseItem(Content[list[list.Count - 1]], list[list.Count - 1], null);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Equips the item at the specified slot 
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        /// <param name="slot">Slot.</param>
        public virtual void EquipItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryType == Inventory.InventoryTypes.Main)
            {
                InventoryItem oldItem = null;
                if (InventoryItem.IsNull(item))
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                    return;
                }
                // if the object is not equipable, we do nothing and exit
                if (!item.IsEquippable)
                {
                    return;
                }
                // if a target equipment inventory is not set, we do nothing and exit
                if (item.TargetEquipmentInventory == null)
                {
                    Debug.LogWarning("InventoryEngine Warning : " + Content[index].ItemName + "'s target equipment inventory couldn't be found.");
                    return;
                }
                // if the object can't be moved, we play an error sound and exit
                if (!item.CanMoveObject)
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                    return;
                }
                // call the equip method of the item

                if (!item.Equip())
                {
                    return;
                }
                // if this is a mono slot inventory, we prepare to swap
                if (item.TargetEquipmentInventory.Content.Length == 1)
                {
                    if (!InventoryItem.IsNull(item.TargetEquipmentInventory.Content[0]))
                    {
                        if (
                            (item.CanSwapObject)
                            && (item.TargetEquipmentInventory.Content[0].CanMoveObject)
                            && (item.TargetEquipmentInventory.Content[0].CanSwapObject)
                        )
                        {
                            // we store the item in the equipment inventory
                            oldItem = item.TargetEquipmentInventory.Content[0].Copy();
                            item.TargetEquipmentInventory.EmptyInventory();
                        }
                    }
                }
                // we add one to the target equipment inventory
                item.TargetEquipmentInventory.AddItem(item.Copy(), item.Quantity);
                // remove 1 from quantity
                RemoveItem(index, item.Quantity);
                if (oldItem != null)
                {
                    oldItem.Swap();
                    AddItem(oldItem, oldItem.Quantity);
                }
                MMInventoryEvent.Trigger(MMInventoryEventType.ItemEquipped, slot, this.name, item, item.Quantity, index);
            }
        }

        /// <summary>
        /// Drops the item, removing it from the inventory and potentially spawning an item on the ground near the character
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        /// <param name="slot">Slot.</param>
        public virtual void DropItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return;
            }
            item.SpawnPrefab();
            if (item.UnEquip())
            {
                DestroyItem(index);
            }

        }

        public virtual void DestroyItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return;
            }
            DestroyItem(index);
        }

        public virtual void UnEquipItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            // if there's no item at this slot, we trigger an error
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return;
            }
            // if we're not in an equipment inventory, we trigger an error
            if (InventoryType != InventoryTypes.Equipment)
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return;
            }
            // we trigger the unequip effect of the item
            if (!item.UnEquip())
            {
                return;
            }
            MMInventoryEvent.Trigger(MMInventoryEventType.ItemUnEquipped, slot, this.name, item, item.Quantity, index);

            // if there's a target inventory, we'll try to add the item back to it
            if (item.TargetInventory != null)
            {
                // if we managed to add the item
                if (item.TargetInventory.AddItem(item, item.Quantity))
                {
                    DestroyItem(index);
                }
                else
                {
                    // if we couldn't (inventory full for example), we drop it to the ground
                    MMInventoryEvent.Trigger(MMInventoryEventType.Drop, slot, this.name, item, item.Quantity, index);
                }
            }
        }

        /// <summary>
        /// Catches inventory events and acts on them
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            // if this event doesn't concern our inventory display, we do nothing and exit
            if (inventoryEvent.TargetInventoryName != this.name)
            {
                return;
            }
            switch (inventoryEvent.InventoryEventType)
            {
                case MMInventoryEventType.Pick:
                    AddItem(inventoryEvent.EventItem, inventoryEvent.Quantity);
                    break;

                case MMInventoryEventType.UseRequest:
                    UseItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.EquipRequest:
                    EquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.UnEquipRequest:
                    UnEquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.Destroy:
                    DestroyItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.Drop:
                    DropItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;
            }
        }

        /// <summary>
        /// When we catch an MMGameEvent, we do stuff based on its name
        /// </summary>
        /// <param name="gameEvent">Game event.</param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if ((gameEvent.EventName == "Save") && Persistent)
            {
                SaveInventory();
            }
            if ((gameEvent.EventName == "Load") && Persistent)
            {
                if (ResetThisInventorySaveOnStart)
                {
                    ResetSavedInventory();
                }
                LoadSavedInventory();
            }
        }

        /// <summary>
        /// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<MMInventoryEvent>();
        }

        /// <summary>
        /// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
        /// </summary>
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<MMInventoryEvent>();
        }
    }
}