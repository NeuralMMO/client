using System;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class Player : Character
{

   public void Awake()
   {
      this.skills = this.gameObject.AddComponent<PlayerSkills>();
   }

   public void Init(Dictionary<int, GameObject> players,
            Dictionary<int, GameObject> npcs, int iden, object packet) {
      GameObject prefab = Resources.Load("Prefabs/Overheads") as GameObject;
      this.overheads    = Instantiate(prefab).GetComponent<PlayerOverheads>();
      base.Init(players, npcs, iden, packet);
   }


   public void UpdatePlayer(Dictionary<int, GameObject> players,
         Dictionary<int, GameObject> npcs, object ent) {
      //Resources
      object resources = Unpack("resource", ent);
      this.resources.UpdateResources(resources);

      object status = Unpack("status", ent);
      ((PlayerOverheads) this.overheads).UpdateStatus(status);

      base.UpdatePlayer(players, npcs, ent);
   }
}
