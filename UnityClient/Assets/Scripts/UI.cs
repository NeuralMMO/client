using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MonoBehaviorExtension;
using System;

public class UI: UnityModule {
   Transform canvas;
   Character character;

   GameObject fps;
   TMP_Text wilderness;
   GameObject menu;

   void SetText(GameObject obj, string text) {
      obj.GetComponent<TextMeshProUGUI>().text = text;
   }

   void Awake()
   {
      this.canvas     = this.Get("Canvas");
      this.menu       = GetObject(this.canvas, "RightClickMenu");
      this.wilderness = GetObject(this.canvas, "Wilderness").GetComponent<TMP_Text>();

      this.fps    = GetObject(this.canvas, "Panel/FPS");
   }

   void Update()
   {
      Character character = this.menu.GetComponent<RightClickMenu>().character;
      this.UpdateOverlay(character);
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
      this.character = playerObj.GetComponent<Player>();
      if (character == null)
      {
         character = playerObj.GetComponent<NonPlayer>();
      }

      if (character != null) {
         this.menu.GetComponent<RightClickMenu>().UpdateSelf(this, character);
      }
   }

   public void UpdateUI(Dictionary<string, object> packet, float time) {
      string fps = "FPS: " + (1f / time).ToString("0.0");
      this.SetText(this.fps, fps);

      int wildernessLevel  = Convert.ToInt32(UnpackList(new List<string> { "wilderness" }, packet));
      if (wildernessLevel == -1)
      {
         this.wilderness.text = "Safe";
      } else
      {

         this.wilderness.text = wildernessLevel.ToString();
      }
   }

   public void UpdateOverlay(Character character) {
      Character.UpdateStaticUI();
 
      if (character == null) {
         return;
      }

      character.UpdateUI();

   }
}
