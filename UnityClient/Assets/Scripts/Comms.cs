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
      this.w.SendString("START");
      yield break;
	}

	IEnumerator Start ()
	{
		while (true) {
			//m_IsConnected does not become false upon server crash
			//However, w.error will become non-null
			if (this.w == null || !this.w.m_IsConnected || w.error != null) {
				this.w = new WebSocket (new Uri ("ws://" + ip + ":" + port + "/ws"));
				yield return Connect(5f);
			}

			//Packet from server
			this.reply = w.RecvString();

			if (reply != null) {
				//The client is stateless; do not
				//queue up packets from the server
				if (this.thread != null) {
					this.thread.Abort();
				}

				//Async unpack server data
				this.thread = new Thread(Unpack);
				this.thread.Start();
				this.numPackets++;

				//Message back to server. Can use this to
				//implement client control in the future
				string msg = "Recieved packet " + numPackets.ToString() + " from Server";
				w.SendString(msg);
				Debug.Log(msg);
			}

			yield return 0;
		}
	}
}

