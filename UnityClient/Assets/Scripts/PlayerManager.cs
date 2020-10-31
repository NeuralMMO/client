using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;

public class EntityGroup
{
   public Dictionary<int, GameObject> entities;
   public HashSet<int> IDs;
   public HashSet<int> newIDs;
      
   public EntityGroup()
   {
      this.entities = new Dictionary<int, GameObject>();
      this.IDs = new HashSet<int>();
      this.newIDs = new HashSet<int>();
   }

}

public class PlayerManager : UnityModule
{
   public EntityGroup players;
   public EntityGroup npcs;

   public GameObject anchor;
   public Camera camera;

   Consts consts;
   GameObject prefab;
   GameObject root;
   GameObject cameraAnchor;

   void Start() {
      this.camera = Camera.main;

      this.anchor       = GameObject.Find("Client/CameraAnchor");
      this.prefab       = Resources.Load("Prefabs/Player") as GameObject; 
      this.root         = GameObject.Find("Client/Environment/Players");
      this.cameraAnchor = GameObject.Find("CameraAnchor");

      this.players = new EntityGroup();
      this.npcs    = new EntityGroup();
   }

   public void UpdateEntities(Dictionary<string, object> packet) {
      Dictionary<string, object> p = Unpack("player", packet) as Dictionary<string, object>;
      this.UpdateGroup(this.players, p, true);

      Dictionary<string, object> n = Unpack("npc", packet) as Dictionary<string, object>;
      this.UpdateGroup(this.npcs, n, false);
  }

   public void UpdateGroup(EntityGroup group, Dictionary<string, object> entities, bool isPlayer)
   {
      //Initialize entities
      foreach (KeyValuePair<string, object> ent in entities) {
         int id = Convert.ToInt32(ent.Key);
         if (group.entities.ContainsKey(id)) {
            continue;
         }

         GameObject entityObject = GameObject.Instantiate(this.prefab) as GameObject;
         entityObject.transform.SetParent(root.transform, true);
         if (isPlayer)
         {
            Player entity = entityObject.AddComponent<Player>();
            entity.Init(group.entities, id, ent.Value);
         } else
         {
            NonPlayer entity = entityObject.AddComponent<NonPlayer>();
            entity.Init(group.entities, id, ent.Value);
         }

         group.entities.Add(id, entityObject);
         group.newIDs.Add(id);
      }

      //Step entities
      foreach (KeyValuePair<string, object> ent in entities) {
         int id = Convert.ToInt32(ent.Key);
         group.IDs.Add(id);
         if (isPlayer) {
            Player entity = group.entities[id].GetComponent<Player>();
            entity.UpdatePlayer(group.entities, ent.Value);
         } else
         {
            NonPlayer entity = group.entities[id].GetComponent<NonPlayer>();
            entity.UpdatePlayer(group.entities, ent.Value);
 
         }

      }

      foreach (int id in group.entities.Keys.ToList()) {
         if (group.IDs.Contains(id)) {
            continue;
         }

         if (this.anchor.transform.parent == group.entities[id].transform) {
            this.anchor.transform.parent = null;
         }

         if (isPlayer)
         {
            group.entities[id].GetComponent<Player>().Delete();
         } else
         {
            group.entities[id].GetComponent<NonPlayer>().Delete();
         }
         GameObject.Destroy(group.entities[id]);
         group.entities.Remove(id);
      }

      group.IDs.Clear();
      group.newIDs.Clear();
   }

   bool inRenderDist(Player player)
   {
         int r = (int) Math.Floor(this.cameraAnchor.transform.position.x / Consts.CHUNK_SIZE()) * Consts.CHUNK_SIZE();
         int c = (int) Math.Floor(this.cameraAnchor.transform.position.z / Consts.CHUNK_SIZE()) * Consts.CHUNK_SIZE();
         if(player.r < r - Consts.TILE_RADIUS() || player.r > r + Consts.TILE_RADIUS())
         {
            return false;
         }
         if(player.c < c - Consts.TILE_RADIUS() || player.c > c + Consts.TILE_RADIUS())
         {
            return false;
         }
         return true;
   }

}
