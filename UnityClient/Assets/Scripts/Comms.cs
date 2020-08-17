using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class Comms: MonoBehaviour
{
	public WebSocket w;
	public int NetworkSpeed = 5;
	public string ip = "localhost";
	public string port = "8080";
   public Dictionary<string, object> packet;

	GameObject cameraAnchor;
	Console console;
	int numPackets = 0;
	Thread thread;
	string reply;
	public bool newConnection;

	void Unpack() {
        this.packet = MiniJSON.Json.Deserialize(this.reply) as Dictionary<string,object>;
	}


   public Dictionary<string, object> GetPacket() {
		Dictionary<string, object> pkt = this.packet;
		this.packet = null;
		return pkt; 
	}

	IEnumerator Connect(float frequency)
	{
		while (!this.w.m_IsConnected)
		{
			Debug.Log("Trying to connect to server");
			IEnumerator connect = this.w.Connect();
			StartCoroutine(connect);
			float timeout = frequency;
			while (timeout > 0 && !this.w.m_IsConnected)
			{
				timeout -= Time.deltaTime;
				yield return null;
			}
			if (timeout <= 0)
			{
				Debug.Log("Failed to connect");
				StopCoroutine(connect);
				continue;
			}
		}
      this.newConnection = true;
      Debug.Log("Connected");
      //this.w.SendString("START");
      yield break;
	}

	IEnumerator Start ()
	{
      this.cameraAnchor = GameObject.Find("CameraAnchor");
      this.console      = GameObject.Find("Console").GetComponent<Console>();
		while (true) {
			//m_IsConnected does not become false upon server crash
			//However, w.error will become non-null
			if (this.w == null || !this.w.m_IsConnected || w.error != null) {
				this.w = new WebSocket (new Uri ("ws://" + ip + ":" + port + "/ws"));
				yield return Connect(15f);
			}

			//Packet from server
			this.reply = w.RecvString();

			if (reply != null && (this.thread == null || !this.thread.IsAlive)) {
				//The client is stateless; do not
				//queue up packets from the server

				//Async unpack server data
				this.thread = new Thread(Unpack);
				this.thread.Start();
				this.numPackets++;

				//Message camera pos back to server
				int r = (int)Math.Floor(this.cameraAnchor.transform.position.x);
				int c = (int)Math.Floor(this.cameraAnchor.transform.position.z);
				string cmd = this.console.consumeCommand();
				string msg = "Recieved packet " + numPackets.ToString() + " from Server;" + r.ToString() + " " + c.ToString() + " " + cmd;
				w.SendString(msg);
				Debug.Log(msg);
			}

			yield return 0;
		}
	}
}

