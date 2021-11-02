using System;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;
using TMPro;

public class Player : Character
{
   

   public void Awake()
   {
      this.skills    = this.gameObject.AddComponent<PlayerSkills>();
   }

   public void Init(Dictionary<int, GameObject> players,
            Dictionary<int, GameObject> npcs, int iden, object packet) {
      GameObject overheadsPrefab = Resources.Load("Prefabs/Overheads") as GameObject;
      this.overheads             = Instantiate(overheadsPrefab).GetComponent<PlayerOverheads>();
      base.Init(players, npcs, iden, packet);
   }


   public void UpdatePlayer(Dictionary<int, GameObject> players,
         Dictionary<int, GameObject> npcs, object ent) {
      //Resources
      object resources = Unpack("resource", ent);
      this.resources.UpdateResources(resources);

      object status = Unpack("status", ent);
      ((PlayerOverheads) this.overheads).UpdateStatus(status);

      //Bug Note: odd stack overflows with stackable items
      //Items
      base.UpdatePlayer(players, npcs, ent);
   }
}
