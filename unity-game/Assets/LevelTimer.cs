using UnityEngine;
using System.Collections;

public class LevelTimer : MonoBehaviour {
	public float timePerLevel = 60.0f;
	
	//OVRGUI ovrGui;
	//public OVRCameraController cameraController;

	
	// Use this for initialization
	void Start () {
	
	}
	
	public float GetTimeRemaining(){
		return Mathf.Clamp((timePerLevel - Time.timeSinceLevelLoad),0,60);
	}
	
	// Update is called once per frame
	void Update () {
		
	
	}
}
