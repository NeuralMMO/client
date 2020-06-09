using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class PlayerSkills : UnityModule 
{
   Dictionary<string, Skill> skills;

   public Skill constitution;
   public Skill melee;
   public Skill range;
   public Skill mage;
   public Skill defense;
   public Skill fishing;
   public Skill hunting;

   Skill AddSkill(string name) {
      Skill skill = this.gameObject.AddComponent<Skill>();
      this.skills.Add(name, skill);
      return skill;
   }

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

   public void UpdateSkills(object skills) {
      foreach (KeyValuePair<string, Skill> skill in this.skills) {
         object packet = Unpack(skill.Key, skills);
         skill.Value.UpdateSkill(packet);
      }
   }

   public void UpdateUI() {
      foreach (KeyValuePair<string, Skill> skill in this.skills) {
         GameObject UISkill = GameObject.Find("UI/Canvas/Panel/" + char.ToUpper(skill.Key[0]) + skill.Key.Substring(1));
         TextMeshProUGUI tm = UISkill.GetComponent<TextMeshProUGUI>();
         tm.text = char.ToUpper(skill.Key[0]) + skill.Key.Substring(1) + ": " + skill.Value.level;// + " (" + skill.Value.experience + "XP)";
      }
   }

}
