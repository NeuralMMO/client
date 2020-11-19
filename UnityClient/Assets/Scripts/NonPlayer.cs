using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayer : Character 
{
   public void Awake()
   {
      this.skills = this.gameObject.AddComponent<NonPlayerSkills>();
   }

   public void Init(Dictionary<int, GameObject> players,
            Dictionary<int, GameObject> npcs, int iden, object packet) {
      GameObject prefab = Resources.Load("Prefabs/NonPlayerOverheads") as GameObject;
      this.overheads = Instantiate(prefab).GetComponent<Overheads>();

      base.Init(players, npcs, iden, packet);
      float mag = (float)(0.5 + 1.5*this.level/100f);
      this.transform.localScale = new Vector3(mag, mag, mag);
   }

   public void UpdatePlayer(Dictionary<int, GameObject> players,
            Dictionary<int, GameObject> npcs, object ent) {
      //Resources
      object resources = Unpack("resource", ent);
      this.resources.UpdateResources(resources);

      //Status
      object status = Unpack("status", ent);
      this.overheads.UpdateStatus(status);

      base.UpdatePlayer(players, npcs, ent);
   }
}

