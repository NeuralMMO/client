using System;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class Player : UnityModule {
   public int id;
   public int r = 0;
   public int c = 0;

   public GameObject target;
   public GameObject attack;
   //public ResourceBars bars;

   public int rOld = 0;
   public int cOld = 0;

   public string name;
   public int level = 1;

   float start;
   Vector3 orig;

   public PlayerSkills skills;
   public PlayerResources resources;

   GameObject prefab;
   public Overheads overheads;

   //Load the OBJ shader and materials
   public Color NNObj(Color color) {
      MeshRenderer nn = this.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
      nn.materials[0].SetColor("_Color", color);
      return color;
   }

   //Create the overhead UI
   public void Overheads(string name, Color color) {
      this.prefab    = Resources.Load("Prefabs/Overheads") as GameObject;
      this.overheads = Instantiate(this.prefab).GetComponent<Overheads>();
      this.resources = this.overheads.resources;

      this.overheads.name   = name;
      this.overheads.color  = color; 
      this.overheads.player = this;
   }

   public void Init(Dictionary<int, GameObject> players, int iden, object packet) {
      this.skills             = this.gameObject.AddComponent<PlayerSkills>();
      this.orig               = this.transform.position;
      this.start              = Time.time;
      this.id                 = iden;

      object basePacket = Unpack("base", packet);
      string name       = (string) Unpack("name", basePacket);
      Color  color      = hexToColor((string) Unpack("color", basePacket));

      //OBJ model and overheads
      this.NNObj(color);
      this.Overheads(name, color);

      this.UpdatePlayer(players, packet);
      this.UpdatePos(false);
   }

   public void UpdateUI(){
      this.skills.UpdateUI();
      this.resources.UpdateUI();

      GameObject UIName      = GameObject.Find("UI/Canvas/Panel/Name");
      TextMeshProUGUI uiName = UIName.GetComponent<TextMeshProUGUI>();

      uiName.color = this.overheads.playerName.color;
      uiName.text  = this.overheads.playerName.text;
   }

   public static void UpdateStaticUI() {
      PlayerResources.UpdateDeadUI();
   }

   void Update(){
      this.UpdatePos(true);
      this.UpdateAttack();
      this.overheads.UpdateOverheads(this);
   }
 
   public void UpdatePos(bool smooth) {
      Vector3 orig = new Vector3(this.rOld, 0, this.cOld);
      Vector3 targ = new Vector3(this.r, 0, this.c);
      if (smooth) {
        this.transform.position = Vector3.Lerp(orig, targ, Client.tickFrac);
      } else {
        this.transform.position = targ;
      }
   }

   public void UpdateAttack() {
      if (this.attack == null || this.target == null) {
         return;
      }

      Vector3 orig = new Vector3(0, 0, 0);
      Vector3 targ = this.target.transform.position - this.transform.position + 3*Vector3.up/4; 
      this.attack.transform.localPosition = Vector3.Lerp(orig, targ, Client.tickFrac);
   }

   public void UpdatePlayer(Dictionary<int, GameObject> players, object ent) {
      this.orig  = this.transform.position;
      this.start = Time.time;

      //Position
      object entBase = Unpack("base", ent);
      this.rOld = this.r;
      this.cOld = this.c;

      this.r  = Convert.ToInt32(UnpackList(new List<string>{"r", "val"}, entBase));
      this.c  = Convert.ToInt32(UnpackList(new List<string>{"c", "val"}, entBase));

      //Resources
      object resources = Unpack("resource", ent);
      this.resources.UpdateResources(resources);

      //Skills
      object skills = Unpack("skills", ent);
      this.skills.UpdateSkills(skills);
      this.level = Convert.ToInt32(Unpack("level", skills));

      //Status
      object status = Unpack("status", ent);
      this.overheads.UpdateStatus(status);


      //Attack
      if (this.attack != null) {
         Destroy(this.attack);
         this.target = null;
      }

      Dictionary<string, object> hist = Unpack("history", ent) as Dictionary<string, object>;
      int damage = Convert.ToInt32(UnpackList(new List<string>{"damage", "val"}, hist));

      this.overheads.UpdateDamage(damage);
      
      //Handle attacks
      if (! hist.ContainsKey("attack")) {
         return;
      }

      Debug.Log("Attack!");

      object attk   = Unpack("attack", hist);
      string style  = Unpack("style", attk) as string;
      object targ   = Unpack("target", attk);
      int targs     = Convert.ToInt32(targ);

      //Handle targets
      if (! players.ContainsKey(targs)) {
        return;
      }

      this.target = players[targs];
      UnityEngine.Object prefab = Resources.Load("Prefabs/" + style + "Attack") as GameObject; 
      this.attack = GameObject.Instantiate(prefab) as GameObject;
      this.attack.transform.SetParent(this.transform);
      this.attack.transform.localPosition = 3*Vector3.up/4;
   }

   public void Delete() {
      GameObject.Destroy(this.overheads.gameObject);
      GameObject.Destroy(this.overheads);

      if (this.attack != null) {
         GameObject.Destroy(this.attack);
      }
   }

   //Random function off Unity forums
   public static Color hexToColor(string hex)
   {
      hex = hex.Replace ("0x", "");//in case the string is formatted 0xFFFFFF
      hex = hex.Replace ("#", "");//in case the string is formatted #FFFFFF
      byte a = 255;//assume fully visible unless specified in hex
      byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
      byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
      byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
      //Only use alpha if the string has enough characters
      if(hex.Length == 8){
            a = byte.Parse(hex.Substring(6,2), System.Globalization.NumberStyles.HexNumber);
      }
      return new Color32(r,g,b,a);
   }
 
}