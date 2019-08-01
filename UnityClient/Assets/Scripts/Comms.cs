using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using MiniJSON;
//using SimpleJSON;

public class Comms: MonoBehaviour
{
	public int NetworkSpeed = 5;
	public string ip = "localhost";
	//public string ip = "192.168.1.18";
	public string port = "8080";
    public Dictionary<string, object> packet;

	private int x = 0;
	private int currentTime;
	Thread thread;
	string reply;

	void Unpack() {
        this.packet = MiniJSON.Json.Deserialize(this.reply) as Dictionary<string,object>;
	}


    public Dictionary<string, object> GetPacket() {
		Dictionary<string, object> pkt = this.packet;
		this.packet = null;
		return pkt; 
	}

	IEnumerator Start ()
	{
		WebSocket w = new WebSocket (new Uri ("ws://" + ip + ":" + port + "/ws"));
		//WebSocket w = new WebSocket (new Uri ("ws://" + ip + ":" + port + "/ws"));
		yield return StartCoroutine (w.Connect ());
		w.SendString ("START");

		while (true) {
			if ((int)((Time.time % 60) * NetworkSpeed) >= x) {
				w.SendString ("Open for: " + x.ToString ());
				x++;
			}

			this.reply = w.RecvString ();

			if (reply != null) {
				this.thread = new Thread(Unpack);
				this.thread.Start();
				Debug.Log (reply);
			}
			if (w.error != null) {
				Debug.LogError ("Error: " + w.error);
				break;
			}
			yield return 0;
		}
		w.Close ();
	}
}

