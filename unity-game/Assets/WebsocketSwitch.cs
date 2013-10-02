using System;
using UnityEngine;
using System.Collections;
using WebSocketSharp;

public class WebsocketSwitch : MonoBehaviour {
	
	WebSocket ws;
	public string host = "meat-toy.nodejitsu.com";
	public int port = 80;
	
	LevelTimer levelTimer;
	
	// Use this for initialization
	void Start () {
	 	ws = new WebSocket("ws://"+host+":"+port);
		levelTimer = GetComponent<LevelTimer>();
		print ("connecting...");
		ws.Connect();

	}
	
	// Update is called once per frame
	void Update () {
		print (levelTimer.GetTimeRemaining());
		ws.Send(levelTimer.GetTimeRemaining().ToString("0.000"));
		if(Input.GetKeyDown(KeyCode.Space)){
			ws.Send("switch");
		}
	}
}
