using System;
using UnityEngine;
using System.Collections;
using WebSocketSharp;

public class WebsocketSwitch : MonoBehaviour {
	
	WebSocket ws;
	public string host = "meat-toy.nodejitsu.com";
	public int port = 80;
	
	// Use this for initialization
	void Start () {
		//Security.PrefetchSocketPolicy(host, port);

	 	ws = new WebSocket("ws://"+host+":"+port);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)){
			print ("connecting");
					ws.Connect();

		} 
		if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			print ("gotcha");
			ws.Send(String.Format("write"));
		}
	}
}
