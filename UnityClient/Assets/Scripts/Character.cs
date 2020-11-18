using MonoBehaviorExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

   float start;
   Vector3 orig;

   public SkillGroup skills;
   public ResourceGroup resources;
   public Overheads overheads;

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

   public void Init(Dictionary<int, GameObject> players, int iden, object packet)
   {
      this.orig = this.transform.position;
      this.start = Time.time;
      this.id = iden;

      object basePacket = Unpack("base", packet);
      string name = (string)Unpack("name", basePacket);

      Color ball        = hexToColor((string)Unpack("color", basePacket));
      Color rod_bottom  = hexToColor((string)UnpackList(new List<string> { "loadout", "platelegs", "color" }, packet));
      Color rod_top     = hexToColor((string)UnpackList(new List<string> { "loadout", "chestplate", "color" }, packet));

      //OBJ model and overheads
      this.NNObj(ball, rod_bottom, rod_top);
      this.Overheads(name, ball);

      this.UpdatePlayer(players, packet);
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
      if (this.alive) {
         this.UpdatePos(true);
      } else {
         this.DeathAnimation();
      }
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

   public void UpdatePlayer(Dictionary<int, GameObject> players, object ent)
   {
      this.orig    = this.transform.position;
      this.forward = this.transform.forward;
      this.up      = this.transform.up;
      this.start   = Time.time;

      this.alive = Convert.ToBoolean(Unpack("alive", ent));

      //Position
      object entBase = Unpack("base", ent);
      this.rOld = this.r;
      this.cOld = this.c;

      this.r = Convert.ToInt32(UnpackList(new List<string> { "r" }, entBase));
      this.c = Convert.ToInt32(UnpackList(new List<string> { "c" }, entBase));

      //Skills
      object skills = Unpack("skills", ent);
      this.skills.UpdateSkills(skills);
      this.level = Convert.ToInt32(Unpack("level", skills));

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
      if (!players.ContainsKey(targs))
      {
         return;
      }

      this.target = players[targs];

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
