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

   GameObject prefab;
   GameObject root;

   HashSet<int> idens;
   HashSet<int> newEnts;

   void Start() {
      this.camera = Camera.main;

      this.anchor = GameObject.Find("Client/CameraAnchor");
      this.prefab = Resources.Load("Prefabs/Player") as GameObject; 
      this.root   = GameObject.Find("Client/Environment/Players");

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

   void Init(Dictionary<string, object> ents){
      foreach (KeyValuePair<string, object> ent in ents) {
         int id = Convert.ToInt32(ent.Key);
         if (players.ContainsKey(id)) {
            continue;
         }

         GameObject player      = GameObject.Instantiate(this.prefab) as GameObject;
         Player playerComponent = player.AddComponent<Player>();

         player.transform.SetParent(root.transform, true);
         player.GetComponent<Player>().Init(this.players, id, ent.Value);

         players.Add(id, player);
         newEnts.Add(id);
      }
   }

   void Step(Dictionary<string, object> ents){
      foreach (KeyValuePair<string, object> ent in ents) {
         int id = Convert.ToInt32(ent.Key);
         idens.Add(id);

         players[id].GetComponent<Player>().UpdatePlayer(this.players, ent.Value);
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