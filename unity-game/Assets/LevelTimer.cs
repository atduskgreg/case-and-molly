using UnityEngine;
using System.Collections;
using System.Linq;

public class LevelTimer : MonoBehaviour {
	public float timePerLevel = 60.0f;
	
	OVRGUI ovrGui;
	public OVRCameraController cameraController;
	public Font timeFont;
	public Color timeColor;
	
	public bool showOverlays = true;
	
	string roomNumber = "";
	
	public string[] instructionSteps;
	public string[] allowedRooms;
	
	int currentStep = 0;
	
	bool hideControls = false;
	public GameObject switchIndicator;
	
	
	// Use this for initialization
	void Start () {		
		ovrGui = new OVRGUI();
		ovrGui.SetCameraController(ref cameraController);
		ovrGui.SetFontReplace(timeFont);
	}
	
	void OnGUI(){
		if(showOverlays){
			Event e = Event.current;
			
			if(e.isKey && e.type == EventType.KeyDown && e.keyCode == KeyCode.DownArrow){
				currentStep++;
			}	
			if(e.isKey && e.type == EventType.KeyDown && e.keyCode == KeyCode.UpArrow){
				currentStep--;
			}
		
			if(currentStep < 0 || (currentStep > instructionSteps.Length-1)){
				currentStep = 0;
			}
		
			//print (currentStep);

       		if (e.keyCode == KeyCode.Return){
				
				if(allowedRooms.Contains(roomNumber)){
					Application.LoadLevel("SortingPuzzle");
				} else {
					roomNumber = "";
				}
				
				//hideControls = true;
				//print("room number submitted: " + roomNumber);
			}  
			
			if(!hideControls){
				GUI.SetNextControlName("RoomInput");
				roomNumber = GUI.TextField(new Rect(300, 350, 200, 20), roomNumber, 25);
				GUI.FocusControl("RoomInput");
				
				string instructionString = "";
				
				int finalStep = (currentStep + 3 < instructionSteps.Length) ? currentStep + 3 : instructionSteps.Length;
				
				for(int i = currentStep; i < finalStep; i++){
					
					instructionString += "\n";
					instructionString += instructionSteps[i];
				}
				
				ovrGui.StereoBox(150,200, 600, 200, ref instructionString, Color.white);
			} 
			
			if(showOverlays){
				switchIndicator.renderer.enabled = true;
			} else {
				switchIndicator.renderer.enabled = false;
			}
		
			string timeString;
			
			if(GetTimeRemaining() > 0){
				timeString = GetTimeRemaining().ToString("0.000");
			} else {
				timeString = "FAIL";
			}
			ovrGui.StereoBox (300,450, 100, 40, ref timeString, timeColor);
		}
	}
	
	public float GetTimeRemaining(){
		return Mathf.Clamp((timePerLevel - Time.timeSinceLevelLoad),0,60);
	}
	
	// Update is called once per frame
	void Update () {
		
	
	}
}
