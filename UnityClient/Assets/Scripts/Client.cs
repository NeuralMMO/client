using UnityEngine;

public class Client : MonoBehaviour
{
   public static float tickFrac;
   public static float tickRate = 0.6f;

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
         Debug.Log("yeet");
      this.ui            = GameObject.Find("UI").GetComponent<UI>();
      this.comms         = GameObject.Find("WebSocket").GetComponent<Comms>();
      this.environment   = GameObject.Find("Environment").GetComponent<Environment>();
      this.playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
   }

   // Update is called once per frame
   void Update()
   {
      this.deltaTime += (Time.unscaledDeltaTime - deltaTime) * this.delta;

      tickFrac = Mathf.Clamp(tickFrac + Time.deltaTime / tickRate, 0, 1);

      //float timeDelta = Time.time - this.time;
      //this.time = Time.time;
      //Only available on server ticks
      //Should update update rate to 0.6
      var packet = this.comms.GetPacket();
      if (packet == null)
      {
         return;
      }

      tickFrac = 0;
      if(this.comms.newConnection) {
         this.comms.newConnection = false;
         this.environment.initTerrain(packet);
      }

      this.environment.UpdateMap(packet);
      this.playerManager.UpdatePlayers(packet);
      this.ui.UpdateFPS(this.deltaTime);
   }
}
