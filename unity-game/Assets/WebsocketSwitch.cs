using System;
using UnityEngine;
using System.Collections;
using WebSocketSharp;

public class WebsocketSwitch : MonoBehaviour {
	
	WebSocket ws;
	public string host = "meat-toy.nodejitsu.com";
	public int port = 80;
	public bool sendClick = true;
	public Color switchColor = Color.red;
	bool switchValue = false;
	public GameObject switchIndicator;

	LevelTimer levelTimer;
	
	// Use this for initialization
	void Start () {
	 	ws = new WebSocket("ws://"+host+":"+port);
		levelTimer = GetComponent<LevelTimer>();
		print ("connecting...");
		ws.Connect();
		switchIndicator.renderer.material.color = switchColor;

	}
	
	void OnGUI(){
		
	}
	
	// Update is called once per frame
	void Update () {
		ws.Send(levelTimer.GetTimeRemaining().ToString("0.000"));
		if(Input.GetMouseButtonDown(0) && sendClick){
			ws.Send("switch");
			switchValue = !switchValue;
			if(switchValue){
				switchColor = Color.green;
			} else {
				switchColor = Color.red;
			}
			
			switchIndicator.renderer.material.color = switchColor;
		}
	}
}
