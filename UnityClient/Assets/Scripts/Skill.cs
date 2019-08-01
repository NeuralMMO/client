using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;

using TMPro;

public class Skill : UnityModule 
{
   public int level;
   public int experience;

   public void Awake() {
       //this.tm = this.GetComponent<TextMeshProUGUI>();
   }

   // Update is called once per frame
   public void UpdateSkill(object skill)
   {
      this.level      = Convert.ToInt32(Unpack("level", skill));
      this.experience = Convert.ToInt32(Unpack("exp", skill));
   }
}
