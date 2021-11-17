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
   public Skill herbalism;
   public Skill prospecting;
   public Skill carving;
   public Skill alchemy;
   
   void Awake()
   {
      this.skills = new Dictionary<string, Skill>();

      this.melee        = this.AddSkill("melee");
      this.range        = this.AddSkill("range");
      this.mage         = this.AddSkill("mage");
      this.fishing      = this.AddSkill("fishing");
      this.herbalism    = this.AddSkill("herbalism");
      this.prospecting  = this.AddSkill("prospecting");
      this.carving      = this.AddSkill("carving");
      this.alchemy      = this.AddSkill("alchemy");
   }
}
