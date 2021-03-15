using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class PlayerSkills : SkillGroup 
{
   public Skill melee;
   public Skill range;
   public Skill mage;
   public Skill fishing;
   public Skill hunting;

   void Awake()
   {
      this.skills = new Dictionary<string, Skill>();

      this.melee        = this.AddSkill("melee");
      this.range        = this.AddSkill("range");
      this.mage         = this.AddSkill("mage");
      this.fishing      = this.AddSkill("fishing");
      this.hunting      = this.AddSkill("hunting");
   }
}
