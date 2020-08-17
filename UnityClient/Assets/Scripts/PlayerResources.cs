using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class PlayerResources: ResourceGroup 
{

   public Resource health;
   public Resource food;
   public Resource water;

   void Awake()
   {
      this.resources =  new Dictionary<string, Resource>();

      this.health = this.AddResource("health");
      this.food   = this.AddResource("food");
      this.water  = this.AddResource("water");
   }
}
