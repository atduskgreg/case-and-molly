using UnityEngine;
using System.Collections;
using System;

public class WaypointManager : MonoBehaviour {

	public Vector2[] visibleWayPoints;
	public Vector2[] hiddenDestinations;
	public int currentWayPoint = 0;
	ShowHUD hudScript;

	// Use this for initialization
	void Start () {
		hudScript = gameObject.GetComponent<ShowHUD>();
		hudScript.SetNextWaypoint (visibleWayPoints[0]);
	
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

	// Update is called once per frame
	void Update () {
//		Vector2 currentLoc = new Vector2((float)hudScript.point_lat, (float)hudScript.point_lng);
				double d = GetDistance (hudScript.point_lat, hudScript.point_lng, visibleWayPoints [0].x, visibleWayPoints [0].y);
				print (d);
				if (d < 600) {
						hudScript.SetMapAlpha (Mathf.Pow((float)d/600, 4));
				}
		}
}
