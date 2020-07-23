using System;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class Overheads: UnityModule
{
   public TextMeshPro tm;
   public TextContainer tc;
   public MeshRenderer mr;

   public string text;
   public Color color;
   public int fontSz = 60;
   public float heightOffset = 1.2f;
   public Camera camera;

   public Canvas canvas;
   public float offset = 1f;

   public GameObject prefab;

   public TMP_Text playerName;
   public TMP_Text damage;
   public TMP_Text freeze;
   public TMP_Text immune;
   public Vector3 worldPos;
   public Vector3 damageOrig;

   private UIDepth depthScript;
   private MeshRenderer selfRenderer;

   public Player player;
   public PlayerResources resources;

   void Awake()
   {
       this.camera    = Camera.main;
       this.resources = this.GetComponentInChildren<PlayerResources>();
       this.canvas    = GameObject.Find("Overlay").GetComponent<Canvas>();

       this.canvas.GetComponent<ScreenSpaceCanvas>().AddToCanvas(this.gameObject);
       this.transform.position = new Vector3(-100, -100, 0); //Render off screen
       this.transform.SetParent(this.canvas.transform);

       //Name & Damage
       TMP_Text[] text  = this.GetComponentsInChildren<TMP_Text>();
       this.playerName  = text[0];
       this.damage      = text[1];
       this.freeze      = text[2];
       this.immune      = text[3];
       this.damageOrig  = this.damage.transform.localPosition;
       this.damage.text = "Damage";
 
       //Bars
       this.depthScript  = this.GetComponent<UIDepth>();
       //this.selfRenderer = GetComponent<Renderer>();
   }

   public void Update() {
      Vector3 orig = this.damage.transform.localPosition;
      Vector3 targ = orig + new Vector3(0, 2.0f, 0);
      this.damage.transform.localPosition = Vector3.Lerp(orig, targ, Client.tickFrac);
      this.damage.alpha = 2*(1 - Client.tickFrac);
   }

   //Every tick
   public void UpdateDamage(int dmg) {
      if (dmg == 0) {
         this.damage.text = "";
      } else {
         this.damage.text = dmg.ToString();
      }
      this.damage.transform.localPosition = this.damageOrig;
   }

   public void UpdateStatus(object status) {
      int immuneTicks = Convert.ToInt32(UnpackList(new List<string>{"immune"}, status));
      if (immuneTicks > 0) {
         this.immune.text = immuneTicks.ToString();
         this.immune.gameObject.SetActive(true);
      } else {
         this.immune.gameObject.SetActive(false);
      }

      int freeze = Convert.ToInt32(UnpackList(new List<string>{"freeze"}, status));
      if (freeze > 0) {
         this.freeze.text = freeze.ToString();
         this.freeze.gameObject.SetActive(true);
      } else {
         this.freeze.gameObject.SetActive(false);
      }
   }

   //Every frame
   public void UpdateOverheads(Player player)
   {
      if (this.selfRenderer == null) {
         this.selfRenderer = player.GetComponent<MeshRenderer>();
      }

      if (this.selfRenderer.isVisible) {
          this.gameObject.SetActive(true);
      } else {
          this.gameObject.SetActive(false);
      }

      Vector3 pos = player.transform.position;

      this.playerName.color = color;
      this.playerName.text  = "<color=#00bbbb>(Lvl " + player.level + ") </color>" + name;

      worldPos          = new Vector3(pos.x, pos.y + 1f, pos.z);
      Vector3 screenPos = this.camera.WorldToScreenPoint(worldPos);

      this.transform.position = screenPos;
      this.depthScript.depth  = -(worldPos - Camera.main.transform.position).magnitude;
   }
}
