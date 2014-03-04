using UnityEngine;
using System.Collections;

public class ShowEnding : MonoBehaviour {

	public Texture endingImage;
		public GUIStyle timerStyle;

		float finalTime;
	// Use this for initialization
	void Start () {
				finalTime = WaypointManager.GetElapsedGameTime();
	}
	
	void GUIGuts(){
				Color before = GUI.color;
				GUI.color = new Color(0,1,0, Mathf.Sin(Time.time*15)*0.3f + 0.7f);

				GUI.Label(new Rect(12,32, 100, 20), "Run time:\n" + finalTime.ToString("f2"), timerStyle);

				GUI.color = before;
		GUI.DrawTexture(new Rect(0,0,512,512), endingImage, ScaleMode.ScaleToFit, true, 0.0f);
	}
	
	void OnGUI () {
		GUI.BeginGroup(new Rect(128, 128, 512, 512));
		GUIGuts ();
		GUI.EndGroup();
		
		
		GUI.BeginGroup(new Rect(Screen.width / 2 + 128 - 100, 128, 512, 512));
		GUIGuts ();
		GUI.EndGroup();
	}
}
