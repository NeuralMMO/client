using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class Overheads : UnityModule
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
   GameObject cameraAnchor;

   public TMP_Text[] TMPComponents;
   public TMP_Text playerName;
   public TMP_Text damage;
   public TMP_Text freeze;
   public Vector3 damageOrig;

   private MeshRenderer selfRenderer;

   public Character player;
   public ResourceGroup resources;

   public float depth;
   private CanvasGroup canvasGroup;
   public Vector3 foo = new Vector3();

   protected virtual void Awake()
   {
       this.camera       = Camera.main;
       this.cameraAnchor = GameObject.Find("CameraAnchor");
       this.resources    = this.GetComponentInChildren<NonPlayerResources>();
       this.canvas       = GameObject.Find("Overlay").GetComponent<Canvas>();

       this.canvasGroup = this.GetComponent<CanvasGroup>();
       //this.canvas.GetComponent<ScreenSpaceCanvas>().AddToCanvas(this);
       this.transform.position = new Vector3(-100, -100, 0); //Render off screen
       this.transform.SetParent(this.canvas.transform);

       //Name & Damage
       TMP_Text[] text  = this.GetComponentsInChildren<TMP_Text>();
       this.TMPComponents = text;
       this.playerName  = text[0];
       this.damage      = text[1];
       this.freeze      = text[2];
       this.damageOrig  = this.damage.transform.localPosition;
       this.damage.text = "Damage";
   }
   public void Update() {
      Vector3 playerPos = player.transform.position;
      Vector3 worldPos  = new Vector3(playerPos.x, playerPos.y + this.player.transform.localScale.x, playerPos.z);
      Vector3 anchor    = this.cameraAnchor.transform.position;
      Vector3 cameraPos = this.camera.transform.position;

      this.depth        = - new Vector3(worldPos.x - anchor.x, cameraPos.y, worldPos.z - anchor.z).magnitude;
      this.canvasGroup.alpha = 1 - Mathf.Clamp((-this.depth - 0) / 32 - 1, 0, 1);

      if (this.canvasGroup.alpha == 0) {
         return;
      }

      Vector3 screenPos = this.camera.WorldToScreenPoint(worldPos);
      this.transform.position = new Vector3(screenPos.x, screenPos.y, 0);
      this.foo = screenPos;

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
      int freeze = Convert.ToInt32(UnpackList(new List<string>{"freeze"}, status));
      if (freeze > 0) {
         this.freeze.text = freeze.ToString();
         this.freeze.gameObject.SetActive(true);
      } else {
         this.freeze.gameObject.SetActive(false);
      }
   }

   //Every frame
   public void UpdateOverheads(Character player)
   {
      if (this.selfRenderer == null) {
         this.selfRenderer = player.GetComponent<MeshRenderer>();
      }

      this.playerName.color = color;
      this.playerName.text  = "<color=#00bbbb>(Lvl " + player.level + ") </color>" + name;

  }
}

