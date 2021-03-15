using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class NonPlayerSkills : SkillGroup
{
   public Skill melee;
   public Skill range;
   public Skill mage;

   void Awake()
   {
      this.skills = new Dictionary<string, Skill>();

      this.melee = this.AddSkill("melee");
      this.range = this.AddSkill("range");
      this.mage = this.AddSkill("mage");
   }
}

