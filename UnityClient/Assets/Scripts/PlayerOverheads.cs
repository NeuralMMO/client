using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerOverheads :Overheads 
{

   //public TMP_Text wilderness;
   protected override void Awake()
   {
      base.Awake();
      this.resources = this.GetComponentInChildren<PlayerResources>();
      //this.wilderness  = this.TMPComponents[3];
   }

   public void UpdateStatus(object status)
   {
      base.UpdateStatus(status);

      /*
      int wildernessLevel = Convert.ToInt32(UnpackList(new List<string> { "wilderness" }, status));
      if (wildernessLevel >= 0)
      {
         this.wilderness.text = wildernessLevel.ToString();
         this.wilderness.gameObject.SetActive(true);
      }
      else
      {
         this.wilderness.gameObject.SetActive(false);
      }
      */
   }
}
