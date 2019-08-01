using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MonoBehaviorExtension;

public class UI: UnityModule {
   Transform canvas;
   Player player;

   GameObject fps;

   GameObject menu;

   void SetText(GameObject obj, string text) {
      obj.GetComponent<TextMeshProUGUI>().text = text;
   }

   void Awake()
   {
      this.canvas = this.Get("Canvas");
      this.menu   = GetObject(this.canvas, "RightClickMenu");

      this.fps    = GetObject(this.canvas, "Panel/FPS");
   }

   void Update()
   {
      Player player = this.menu.GetComponent<RightClickMenu>().player;
      this.UpdateOverlay(player);
      this.UpdateRightClickMenu();
   }

   public void UpdateRightClickMenu() {
      if (!Input.GetMouseButtonDown(0)) {
         return;
      }

      Debug.Log("Click");
      RaycastHit hit;
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (!Physics.Raycast(ray, out hit, 128)) {
         return;
      }

      //Assumes nothing else has collision geometry
      GameObject playerObj = hit.transform.gameObject; 
      this.player = playerObj.GetComponent<Player>();

      if (player != null) {
         this.menu.GetComponent<RightClickMenu>().UpdateSelf(this, player);
      }
   }

   public void UpdateFPS(float time) {
      string fps = "FPS: " + (1f / time).ToString("0.0");
      this.SetText(this.fps, fps);
   }

   public void UpdateOverlay(Player player) {
      Player.UpdateStaticUI();
 
      if (player == null) {
         return;
      }

      player.UpdateUI();

   }
}
