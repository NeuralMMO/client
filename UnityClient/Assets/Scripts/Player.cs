using System;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

using MoreMountains.InventoryEngine;


public class TestItem: InventoryItem {}

public class Player : Character
{
   public NNInventory inventory;

   public void Awake()
   {
      this.skills    = this.gameObject.AddComponent<PlayerSkills>();
   }

   public void Init(Dictionary<int, GameObject> players,
            Dictionary<int, GameObject> npcs, int iden, object packet) {
      GameObject overheadsPrefab = Resources.Load("Prefabs/Overheads") as GameObject;
      this.overheads             = Instantiate(overheadsPrefab).GetComponent<PlayerOverheads>();
      base.Init(players, npcs, iden, packet);

      GameObject inventoryPrefab = Resources.Load("Prefabs/Inventory") as GameObject;
      this.inventory             = Instantiate(inventoryPrefab).GetComponent<NNInventory>();
   }


   public void UpdatePlayer(Dictionary<int, GameObject> players,
         Dictionary<int, GameObject> npcs, object ent) {
      //Resources
      object resources = Unpack("resource", ent);
      this.resources.UpdateResources(resources);

      object status = Unpack("status", ent);
      ((PlayerOverheads) this.overheads).UpdateStatus(status);

      //Items
      List<object> items;
      BaseItem itemObj;
      int level;
      string name;

      //Bug Note: odd stack overflows with stackable items

      //Ammunition
      this.inventory.ammunition.EmptyInventory();

      Dictionary<string, object> ammunition = UnpackList(new List<string> { "inventory", "ammunition"}, ent) as Dictionary<string, object>;
      foreach(KeyValuePair<string, object> e in ammunition)
      {
         Dictionary<string, object> item = (Dictionary<string, object>) e.Value;
         level   = Convert.ToInt32(Unpack("level", item));
         name    = Unpack("item", item) as string;
         itemObj = Resources.Load("Prefabs/" + name) as BaseItem;
         this.inventory.ammunition.AddItem(itemObj, 1);
      }

      //Consumables
      this.inventory.consumables.EmptyInventory();
      items = (List<object>) UnpackList(new List<string> {"inventory", "consumables"}, ent);
      foreach (object e in items)
      {
         Dictionary<string, object> item = (Dictionary<string, object>) e;
         level   = Convert.ToInt32(Unpack("level", item));
         name    = Unpack("item", item) as string;
         itemObj = Resources.Load("Prefabs/" + name) as BaseItem;
         this.inventory.consumables.AddItem(itemObj, 1);
      }

      //Loot
      this.inventory.loot.EmptyInventory();
      items = (List<object>) UnpackList(new List<string> {"inventory", "loot"}, ent);
      foreach (object e in items)
      {
         Dictionary<string, object> item = (Dictionary<string, object>) e;
         level   = Convert.ToInt32(Unpack("level", item));
         name    = Unpack("item", item) as string;
         itemObj = Resources.Load("Prefabs/" + name) as BaseItem;
         this.inventory.loot.AddItem(itemObj, 1);
      }

      object equipment;
      equipment = UnpackList(new List<string> {"inventory", "equipment", "hat"}, ent);
      level     = Convert.ToInt32(Unpack("level", equipment));
      itemObj   = Resources.Load("Prefabs/Hat") as BaseItem;
      this.inventory.hat.EmptyInventory();
      this.inventory.hat.AddItem(itemObj, 1);

      equipment = UnpackList(new List<string> {"inventory", "equipment", "top"}, ent);
      level     = Convert.ToInt32(Unpack("level", equipment));
      itemObj   = Resources.Load("Prefabs/Top") as BaseItem;
      this.inventory.top.EmptyInventory();
      this.inventory.top.AddItem(itemObj, 1);

      equipment = UnpackList(new List<string> {"inventory", "equipment", "bottom"}, ent);
      level     = Convert.ToInt32(Unpack("level", equipment));
      itemObj   = Resources.Load("Prefabs/Bottom") as BaseItem;
      this.inventory.bottom.EmptyInventory();
      this.inventory.bottom.AddItem(itemObj, 1);

      equipment = UnpackList(new List<string> {"inventory", "equipment", "weapon"}, ent);
      level     = Convert.ToInt32(Unpack("level", equipment));
      itemObj   = Resources.Load("Prefabs/Weapon") as BaseItem;
      this.inventory.weapon.EmptyInventory();
      this.inventory.weapon.AddItem(itemObj, 1);

      base.UpdatePlayer(players, npcs, ent);
   }
}
