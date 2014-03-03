using UnityEngine;
using System.Collections;

public class FadeWithDistance : MonoBehaviour {
		public GameObject mainObject;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
//				print (Mathf.Clamp ((float)mainObject.GetComponent<WaypointManager>().DistanceToNextWaypoint (), 0, 600)/600.0f);
					
				float f = (600-Mathf.Clamp((float)mainObject.GetComponent<WaypointManager>().DistanceToNextWaypoint (), 0, 600))/600.0f;
				print ("f: " + f);
				gameObject.GetComponents<AudioSource>()[0].volume = 0.005f + 0.33f*f;
	}
}