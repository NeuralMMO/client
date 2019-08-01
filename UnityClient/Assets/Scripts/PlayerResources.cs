using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class PlayerResources: UnityModule 
{

   public Resource health;
   public Resource food;
   public Resource water;

   Dictionary<string, Resource> resources;

   Resource AddResource(string name) {
      this.gameObject.GetComponentInChildren<Resource>();
      string text = char.ToUpper(name[0]) + name.Substring(1);
      Resource resource = this.transform.Find(text).GetComponent<Resource>();
      this.resources.Add(name, resource);
      return resource;
   }

   void Awake()
   {
      this.resources =  new Dictionary<string, Resource>();

      this.health = this.AddResource("health");
      this.food   = this.AddResource("food");
      this.water  = this.AddResource("water");
   }

   public void UpdateResources(object resources) {
      foreach (KeyValuePair<string, Resource> resource in this.resources) {
         object packet = Unpack(resource.Key, resources);
         resource.Value.UpdateResource(packet);
      }
   }

   public void UpdateUI() {
      foreach (KeyValuePair<string, Resource> resource in this.resources) {
         GameObject UIResource = GameObject.Find("UI/Canvas/Panel/" + char.ToUpper(resource.Key[0]) + resource.Key.Substring(1));
         TextMeshProUGUI tm = UIResource.GetComponent<TextMeshProUGUI>();
         tm.text = resource.Key + ": " + resource.Value.value;
      }
   }

   public static void UpdateDeadUI() {
      GameObject UIResource = GameObject.Find("UI/Canvas/Panel/Health");
      TextMeshProUGUI tm = UIResource.GetComponent<TextMeshProUGUI>();
      tm.text = "Health: Dead";
   }

}
