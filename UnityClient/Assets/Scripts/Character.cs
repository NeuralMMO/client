using MonoBehaviorExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MoreMountains.InventoryEngine;


public class Character: UnityModule 
{

   public int id;
   public int r = 0;
   public int c = 0;

   public Vector3    attackPos;
   public Quaternion attackRot;

   public GameObject target;
   public GameObject attack;
   public Vector3 forward;
   public Vector3 up;

   public int rOld = 0;
   public int cOld = 0;
   public bool alive = true;


   public string name;
   public int level = 1;
   public int item_level = 0;

   const string ITEM_DESCRIPTION_TEMPLATE = "Melee:\t{0, 3} Attack / {1, -3} Defense\nRange:\t{2, 3} Attack / {3, -3} Defense\nMage:\t\t{4, 3} Attack / {5, -3} Defense\nRestore:\t{6, 3} Resource / {7, -3} Health";

   float start;
   Vector3 orig;

   public SkillGroup skills;
   public ResourceGroup resources;
   public Overheads overheads;
   public NNInventory inventory;
   public object packet;

   //Load the OBJ shader and materials
   public void NNObj(Color ball, Color rod_bottom, Color rod_top)
   {
      MeshRenderer nn = this.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
      nn.materials[0].SetColor("_Color", ball);
      nn.materials[1].SetColor("_Color", rod_bottom);
      nn.materials[2].SetColor("_Color", rod_top);
   }

   //Create the overhead UI
   public void Overheads(string name, Color color)
   {
      this.overheads.name = name;
      this.resources = this.overheads.resources;
      this.overheads.color = color;
      this.overheads.player = this;
   }

   public void Init(Dictionary<int, GameObject> players,
         Dictionary<int, GameObject> npcs, int iden, object packet) {
      this.orig = this.transform.position;
      this.start = Time.time;
      this.id = iden;

      object basePacket = Unpack("base", packet);
      string name = (string)Unpack("name", basePacket);

      Dictionary<string, object> equipment = UnpackList(new List<string> { "inventory", "equipment"}, packet) as Dictionary<string, object>;

      //foreach(KeyValuePair<string, object> entry in equip)
      //{
      //   Debug.Log(entry);
      //}

      Color ball        = hexToColor((string)Unpack("color", basePacket));
      //Color rod_bottom  = hexToColor((string)UnpackList(new List<string> { "bottom", "color" }, equipment));
      //Color rod_top     = hexToColor((string)UnpackList(new List<string> { "top", "color" }, equipment));

      Color rod_bottom = hexToColor("000e0e");
      Color rod_top    = hexToColor("000e0e");

      //OBJ model and overheads
      this.NNObj(ball, rod_bottom, rod_top);
      this.Overheads(name, ball);

      GameObject inventoryPrefab = Resources.Load("Prefabs/Inventory") as GameObject;
      this.inventory             = Instantiate(inventoryPrefab).GetComponent<NNInventory>();

      this.UpdatePlayer(players, npcs, packet);
      this.UpdatePos(false);
   }

   public void UpdateUI()
   {
      this.skills.UpdateUI();
      this.resources.UpdateUI();

      GameObject UIName = GameObject.Find("UI/Canvas/Panel/Name");
      TextMeshProUGUI uiName = UIName.GetComponent<TextMeshProUGUI>();

      uiName.color = this.overheads.playerName.color;
      uiName.text = this.overheads.playerName.text;
   }

   public static void UpdateStaticUI()
   {
      PlayerResources.UpdateDeadUI();
   }

   void Update()
   {
      if (!this.alive) {
         this.DeathAnimation();
      } 
      this.UpdatePos(true);
      this.UpdateAttack();
   }

   public void UpdatePos(bool smooth)
   {
      Vector3 orig = new Vector3(this.rOld, 0, this.cOld);
      Vector3 targ = new Vector3(this.r, 0, this.c);
      if (smooth)
      {
         this.transform.position = Vector3.Lerp(orig, targ, Client.tickFrac);
      }
      else
      {
         this.transform.position = targ;
      }

      //Turn to target instead of move direction
      if (this.target != null)
      {
         targ = this.target.transform.position;
      }
      this.transform.forward = Vector3.RotateTowards(this.forward, orig - targ, (float)Math.PI * Client.tickFrac, 0f);
   }

   public void DeathAnimation()
   {
      this.transform.GetChild(0).transform.up = Vector3.RotateTowards(this.up, this.transform.right, (float)Math.PI/2f * Client.tickFrac, 0f);
   }


   public void UpdateAttack()
   {
      if (this.attack == null || this.target == null)
      {
         return;
      }

      //this.attack.transform.position = Vector3.Lerp(this.transform.position, this.target.transform.position, Client.tickFrac) + 3 * Vector3.up / 4;
      //this.attack.transform.position = Vector3.Lerp(this.attackPos, this.target.transform.position, Client.tickFrac);

      //this.attack.transform.rotation = Quaternion.LookRotation(this.target.transform.position - this.attack.transform.position);
      //this.attack.transform.rotation = Quaternion.RotateTowards(this.attackRot, this.attackTarg, this.attackDelta * Client.tickFrac);

      //this.attackTarg   = Quaternion.LookRotation(this.target.transform.position + (this.target.transform.localScale.x * 3 * Vector3.up / 4) - this.attackPos);
      //this.attackRot    = this.attackTarg;
      this.attack.transform.rotation = this.AttackRotation();
   }

   public void UpdateInventory()
   {
      object ent = this.packet;
      {
           
      }
      BaseItem itemObj;
      this.inventory.items.EmptyInventory();
      List<object> items = (List<object>) UnpackList(new List<string> {"inventory", "items"}, ent);
      foreach (object e in items)
      {
         Dictionary<string, object> itm = (Dictionary<string, object>) e;

         string name          = Unpack("item", itm) as string;
         

         int level            = Convert.ToInt32(Unpack("level", itm));
         int quantity         = Convert.ToInt32(Unpack("quantity", itm));
         int melee_attack     = Convert.ToInt32(Unpack("melee_attack", itm));
         int range_attack     = Convert.ToInt32(Unpack("range_attack", itm));
         int mage_attack      = Convert.ToInt32(Unpack("mage_attack", itm));
         int melee_defense    = Convert.ToInt32(Unpack("melee_defense", itm));
         int range_defense    = Convert.ToInt32(Unpack("range_defense", itm));
         int mage_defense     = Convert.ToInt32(Unpack("mage_defense", itm));

         int health_restore   = Convert.ToInt32(Unpack("health_restore", itm));
         int resource_restore = Convert.ToInt32(Unpack("resource_restore", itm));

         itemObj             = Instantiate(Resources.Load("Prefabs/Items/" + name) as BaseItem);;

         itemObj.Quantity    = 1;
         if (quantity == 1) {
            itemObj.ItemName = String.Format(itemObj.ItemName, level, "");
         } else {
            itemObj.ItemName = String.Format(itemObj.ItemName, level, " x " + quantity);            
         }
         itemObj.Description = String.Format(ITEM_DESCRIPTION_TEMPLATE,
               melee_attack, melee_defense, range_attack, range_defense,
               mage_attack, mage_defense, resource_restore, health_restore);
         this.inventory.items.AddItem(itemObj, 1);
      }

      Dictionary<string, object> equipment = UnpackList(new List<string> {"inventory", "equipment"}, ent) as Dictionary<string, object>;
      object item;

      //Equipment
      List<string> item_types = new List<string> {"ammunition", "hat", "top", "bottom", "held"};
      List<Inventory> item_inv   = new List<Inventory> {this.inventory.ammunition, this.inventory.hat, this.inventory.top, this.inventory.bottom, this.inventory.held};
      for (int idx=0; idx<5; idx++)
      {
         Inventory inv = item_inv[idx];
         inv.EmptyInventory();

         string item_type = item_types[idx];
         if (equipment.ContainsKey(item_type)){
            item        = Unpack(item_type, equipment);
            int level   = Convert.ToInt32(Unpack("level", item));
            string name = Unpack("item", item) as string;
            itemObj     = Resources.Load("Prefabs/Items/" + name) as BaseItem;

            inv.AddItem(itemObj, 1);
         }         
      }
   }

   public void UpdatePlayer(Dictionary<int, GameObject> players,
         Dictionary<int, GameObject> npcs, object ent) {
      this.packet  = ent;
      this.orig    = this.transform.position;
      this.forward = this.transform.forward;
      this.up      = this.transform.up;
      this.start   = Time.time;

      this.alive = Convert.ToBoolean(Unpack("alive", ent));

      //Position
      object entBase  = Unpack("base", ent);
      this.level      = Convert.ToInt32(Unpack("level", entBase));
      this.item_level = Convert.ToInt32(Unpack("item_level", entBase));

      this.rOld = this.r;
      this.cOld = this.c;

      this.r = Convert.ToInt32(UnpackList(new List<string> { "r" }, entBase));
      this.c = Convert.ToInt32(UnpackList(new List<string> { "c" }, entBase));

      //Skills
      object skills = Unpack("skills", ent);
      this.skills.UpdateSkills(skills);

      //Attack
      if (this.attack != null)
      {
         Destroy(this.attack);
         this.target = null;
      }

      Dictionary<string, object> hist = Unpack("history", ent) as Dictionary<string, object>;
      int damage = Convert.ToInt32(UnpackList(new List<string> { "damage" }, hist));

      this.overheads.UpdateDamage(damage);
      this.overheads.UpdateOverheads(this);

      //Handle attacks
      if (!hist.ContainsKey("attack"))
      {
         return;
      }

      object attk = Unpack("attack", hist);
      string style = Unpack("style", attk) as string;
      object targ = Unpack("target", attk);
      int targs = Convert.ToInt32(targ);

      //Handle targets
      if (players.ContainsKey(targs))
      {
         this.target = players[targs];
      } else if (npcs.ContainsKey(targs)) {
         this.target = npcs[targs];
      } else {
         return;
      }

      GameObject prefab = Resources.Load("Prefabs/" + style) as GameObject;

      this.attackPos    = this.transform.position + (
            this.transform.localScale.x * 6 * Vector3.up / 4);
      this.attack       = GameObject.Instantiate(
            prefab, this.attackPos, this.AttackRotation());

   }

   
   public Quaternion AttackRotation() {
      return Quaternion.LookRotation(
            this.target.transform.position + (
            this.target.transform.localScale.x * 3 * Vector3.up / 4) - this.attackPos);
   }

   public void Delete()
   {
      GameObject.Destroy(this.overheads.gameObject);
      GameObject.Destroy(this.overheads);

      if (this.attack != null)
      {
         GameObject.Destroy(this.attack);
      }
   }

   //Random function off Unity forums
   public static Color hexToColor(string hex)
   {
      hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
      hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
      byte a = 255;//assume fully visible unless specified in hex
      byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
      byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
      byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
      //Only use alpha if the string has enough characters
      if (hex.Length == 8)
      {
         a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
      }
      return new Color32(r, g, b, a);
   }
}
