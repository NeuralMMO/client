using UnityEngine;

public class Client : MonoBehaviour
{

   public UI ui;
   public Comms comms;
   public Environment environment;
   public PlayerManager playerManager;

   public bool first = true;
   float delta = 0.025f;
   float deltaTime;

   // Start is called before the first frame update
   void Start()
   {
      this.ui            = GameObject.Find("UI").GetComponent<UI>();
      this.comms         = GameObject.Find("WebSocket").GetComponent<Comms>();
      this.environment   = GameObject.Find("Environment").GetComponent<Environment>();
      this.playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
   }

   // Update is called once per frame
   void Update()
   {
      this.deltaTime += (Time.unscaledDeltaTime - deltaTime) * this.delta;

      //float timeDelta = Time.time - this.time;
      //this.time = Time.time;
      //Only available on server ticks
      //Should update update rate to 0.6
      var packet = this.comms.GetPacket();
      if (packet == null)
      {
         return;
      }

      if(this.first) {
         this.environment.UpdateTerrain(packet);
         this.first = false;
      }

      this.environment.UpdateMap(packet);
      this.playerManager.UpdatePlayers(packet);
      this.ui.UpdateFPS(this.deltaTime);
   }
}
