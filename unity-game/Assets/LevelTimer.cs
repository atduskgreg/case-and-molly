using UnityEngine;
using System.Collections;

public class LevelTimer : MonoBehaviour {
	public float timePerLevel = 60.0f;
	
	//OVRGUI ovrGui;
	//public OVRCameraController cameraController;
	
	string roomNumber = "";
	
	// Use this for initialization
	void Start () {		
		
	}
	
	void OnGUI(){
		Event e = Event.current;

        if (e.keyCode == KeyCode.Return){
			print("room number submitted: " + roomNumber);

		} else {
			GUI.SetNextControlName("RoomInput");
			roomNumber = GUI.TextField(new Rect(300, 350, 200, 20), roomNumber, 25);
			GUI.FocusControl("RoomInput");
		}
	}
	
	public float GetTimeRemaining(){
		return Mathf.Clamp((timePerLevel - Time.timeSinceLevelLoad),0,60);
	}
	
	// Update is called once per frame
	void Update () {
		
	
	}
}
