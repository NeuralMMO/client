using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MonoBehaviorExtension;

public class PlayerManager : UnityModule
{
   private Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

   public GameObject anchor;
   public Camera camera;

   Consts consts;
   GameObject prefab;
   GameObject root;
   GameObject cameraAnchor;

   HashSet<int> idens;
   HashSet<int> newEnts;

   void Start() {
      this.camera = Camera.main;

      this.anchor       = GameObject.Find("Client/CameraAnchor");
      this.prefab       = Resources.Load("Prefabs/Player") as GameObject; 
      this.root         = GameObject.Find("Client/Environment/Players");
      this.cameraAnchor = GameObject.Find("CameraAnchor");

      this.idens   = new HashSet<int>();
      this.newEnts = new HashSet<int>();
   }

   public void UpdatePlayers(Dictionary<string, object> packet) {
      Dictionary<string, object> ents = Unpack("ent", packet) as Dictionary<string, object>;

      this.Init(ents);
      this.Step(ents);
      this.Cull(ents);

      this.idens.Clear();
      this.newEnts.Clear();
   }

   bool inRenderDist(Player player)
   {
         int r = (int) Math.Floor(this.cameraAnchor.transform.position.x / Consts.CHUNK_SIZE) * Consts.CHUNK_SIZE;
         int c = (int) Math.Floor(this.cameraAnchor.transform.position.z / Consts.CHUNK_SIZE) * Consts.CHUNK_SIZE;
         if(player.r < r - Consts.TILE_RADIUS || player.r > r + Consts.TILE_RADIUS)
         {
            return false;
         }
         if(player.c < c - Consts.TILE_RADIUS || player.c > c + Consts.TILE_RADIUS)
         {
            return false;
         }
         return true;
   }

   void Init(Dictionary<string, object> ents){
      foreach (KeyValuePair<string, object> ent in ents) {
         int id = Convert.ToInt32(ent.Key);
         if (players.ContainsKey(id)) {

            continue;
         }

         GameObject playerObj   = GameObject.Instantiate(this.prefab) as GameObject;
         Player playerComponent = playerObj.AddComponent<Player>();

         playerObj.transform.SetParent(root.transform, true);
         Player player = playerObj.GetComponent<Player>();
         player.Init(this.players, id, ent.Value);

         if (!inRenderDist(player))
         {
            Destroy(player);
            Destroy(playerObj);
         }

         players.Add(id, playerObj);
         newEnts.Add(id);
      }
   }

   void Step(Dictionary<string, object> ents){
      foreach (KeyValuePair<string, object> ent in ents) {
         int id = Convert.ToInt32(ent.Key);
         Player player = players[id].GetComponent<Player>();
         if (inRenderDist(player))
         {
            idens.Add(id);
         }

         player.UpdatePlayer(this.players, ent.Value);
      }
   }

   void Cull(Dictionary<string, object> ents){
      foreach (int id in this.players.Keys.ToList()) {
         if (idens.Contains(id)) {
            continue;
         }

         if (this.anchor.transform.parent == players[id].transform) {
            this.anchor.transform.parent = null;
         }

         players[id].GetComponent<Player>().Delete();
         GameObject.Destroy(players[id]);
         players.Remove(id);
      }
   }

}