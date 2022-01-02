using MonoBehaviorExtension;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillGroup : UnityModule 
{
   public Dictionary<string, Skill> skills;

   public Skill AddSkill(string name)
   {
      Skill skill = this.gameObject.AddComponent<Skill>();
      this.skills.Add(name, skill);
      return skill;
   }

   public void UpdateSkills(object skills)
   {
      foreach (KeyValuePair<string, Skill> skill in this.skills)
      {
         object packet = Unpack(skill.Key, skills);
         skill.Value.UpdateSkill(packet);
      }
   }

   public void UpdateUI()
   {
      foreach (KeyValuePair<string, Skill> skill in this.skills)
      {
         GameObject UISkill = GameObject.Find("UI/Canvas/Panel/" + char.ToUpper(skill.Key[0]) + skill.Key.Substring(1));
         TextMeshProUGUI tm = UISkill.GetComponent<TextMeshProUGUI>();
         tm.text = char.ToUpper(skill.Key[0]) + skill.Key.Substring(1) + ": " + skill.Value.level;
      }
   }
}
