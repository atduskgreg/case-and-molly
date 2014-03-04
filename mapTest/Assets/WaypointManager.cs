using UnityEngine;
using System.Collections;
using System;

public class WaypointManager : MonoBehaviour {

	public Vector2[] visibleWayPoints;
	ShowHUD hudScript;
	public float victoryDistance = 5.0f; // feet

	static public int currentWayPoint = -1;
	static float gameStartTime = 0.0f;
	static bool gameStarted = false;

	// Use this for initialization
	void Start (){
		hudScript = gameObject.GetComponent<ShowHUD> ();
		WaypointManager.currentWayPoint++;
		hudScript.SetNextWaypoint (visibleWayPoints [WaypointManager.currentWayPoint]);	
	}

	double GetDistance(double lat1, double lng1, double lat2, double lng2) {
		var R = 6371; // Radius of the earth in km
		var dLat = deg2rad(lat2-lat1);  // deg2rad below
		var dLon = deg2rad(lng2-lng1); 
		var a = 
			Math.Sin(dLat/2) * Math.Sin(dLat/2) +
			Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * 
			Math.Sin(dLon/2) * Math.Sin(dLon/2)
			; 
		var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a)); 
		var d = R * c; // Distance in km
		return kmToFt(d);
	}

	double deg2rad(double deg) {
		return deg * (Math.PI / 180);
	}

	double kmToFt(double km){
		return (km * 3280.84);
	}


	public bool AtFinalWaypoint(){
			return (currentWayPoint == visibleWayPoints.Length-1);
	}

	public bool IsWithinVictoryDistance(){
		return (DistanceToNextWaypoint() <= victoryDistance);
	}
	
	public double DistanceToNextWaypoint(){
		return GetDistance(hudScript.point_lat, hudScript.point_lng, visibleWayPoints[WaypointManager.currentWayPoint].x, visibleWayPoints[WaypointManager.currentWayPoint].y);
	}

	static public float GetElapsedGameTime(){
		if (WaypointManager.gameStarted) {
			return (Time.time - WaypointManager.gameStartTime);
		} else {
			return 0.0f;
		}
	}

	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.Space) && !WaypointManager.gameStarted){
			WaypointManager.gameStartTime = Time.time;	
			WaypointManager.gameStarted = true;
						print ("sending start");
			hudScript.SendStartSignal();
		}


//		print (GetElapsedGameTime());

		if (DistanceToNextWaypoint() < 600) {
			hudScript.SetMapAlpha (Mathf.Pow((float)DistanceToNextWaypoint()/600, 4));
		}
						
	}
}
