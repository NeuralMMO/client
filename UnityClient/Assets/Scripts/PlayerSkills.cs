using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class PlayerSkills : SkillGroup 
{
   public Skill constitution;
   public Skill melee;
   public Skill range;
   public Skill mage;
   public Skill defense;
   public Skill fishing;
   public Skill hunting;

   void Awake()
   {
      this.skills = new Dictionary<string, Skill>();

      this.constitution = this.AddSkill("constitution");
      this.melee        = this.AddSkill("melee");
      this.range        = this.AddSkill("range");
      this.mage         = this.AddSkill("mage");
      this.defense      = this.AddSkill("defense");
      this.fishing      = this.AddSkill("fishing");
      this.hunting      = this.AddSkill("hunting");
   }
}
