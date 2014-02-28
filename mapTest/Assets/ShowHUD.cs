using UnityEngine;
using System.Collections;
using System;
using System.Text;
using WebSocketSharp;
using SimpleJSON;
using System.Collections.Generic;

public class ShowHUD : MonoBehaviour {

	public Texture mapImage;
	public double  top_lat;
	public float top_lng;

	public double point_lat;
	public double point_lng;

	OVRGUI ovrGui;
	public OVRCameraController cameraController;

	public double debugMoveAmt = 0.00001d;

	public float pointSize = 10.0f;

	public GUIStyle guiStyle;
		public GUIStyle timerStyle;

	public Texture ret;
	public Texture bigTile;


	int zoom = 17;

	WebSocket ws;
	public string host = "case-and-molly-server.herokuapp.com";

	Vector2 tileNum;
	Vector2 nextWayPoint;

	public List<Texture> mapTiles; // 0 1 2
								   //   3

	public Vector2 guiPosition;
	public int mapSize = 256;

		float mapAlpha = 1.0f;
	Vector2 lerpTo;
	bool needsLerp = false;
	float lerpStarted = 0.0f;

	String currentMapTile = "39654x48478";
	bool needsNewMapTile = false;
	// Use this for initialization

	int pointX;
	int pointY;
	int topX;
	int topY;

	Vector2 prevPosition;

		bool mollyClickedHere = false;

	void Start () {
		ovrGui = new OVRGUI();
		ovrGui.SetCameraController(ref cameraController);

		Texture t = mapTiles[0];
		mapImage = t;

		ws = new WebSocket("ws://"+host);
		ws.OnOpen += (sender, e) => {
			print ("socket open");
			ws.Send ("join");
		};
		ws.OnMessage += (sender, e) => {
//			print(e.Data);
			var d = JSON.Parse(e.Data);


			List<string> keys = new List<string>(); 
			foreach(string key in d.Keys)
			{
				keys.Add(key);
			}

			if(keys.Contains("molly") && (d["molly"].AsInt == 1)){
				mollyClickedHere = true;
			} else{
				mollyClickedHere = false;
			}
								

			if(keys.Contains("location")){
				tileNum = GetTileNumber(d["location"]["lat"].AsDouble,d["location"]["lng"].AsDouble, zoom);

				lerpTo.x = d["location"]["lat"].AsFloat;
				lerpTo.y = d["location"]["lng"].AsFloat;

//				print ("Loc: " + point_lat + "," + point_lng);

//				print("Tile number: "  + (int)tileNum.x  +"x" + (int)tileNum.y);
				string newTexName = tileNum.x + "x" + tileNum.y;
				if(!String.Equals(currentMapTile, newTexName)){
					currentMapTile = newTexName;
					needsNewMapTile = true;
					needsLerp = true;
				}
			}

		};

		ws.OnError += (sender, e) => {
			print ("error: " + e);
		};

		ws.Connect();
	}

	public void SendStartSignal(){
		ws.Send("{\"start\": \"1\"}");
	}
			

	void OnGUI(){
		if(needsNewMapTile == true){
//			print ("changing map tile");
			lerpStarted = Time.time;

			foreach(Texture tex in mapTiles){
				if(tex.name == currentMapTile){
					mapImage = tex;
				}
			}
			needsNewMapTile = false;
		}



		int levelOfDetail = 17;

		topX = 0;
		topY = 0;//39654x48478

		if(needsLerp){
//			print ("needs lerp: " + (Time.time - lerpStarted));
			point_lat = (double)Mathf.Lerp((float)point_lat, lerpTo.x, Time.time - lerpStarted);
			point_lng = (double)Mathf.Lerp((float)point_lng, lerpTo.y, Time.time - lerpStarted);
		}

		if(point_lat == lerpTo.x && point_lng == lerpTo.y){
//			print ("end lerp");
			needsLerp = false;
		}

				//39653x48478
		Vector2 tileTop = LatLngForTileNumber(39653, 48478, zoom);
		Microsoft.MapPoint.TileSystem.LatLongToPixelXY( tileTop.x,  tileTop.y,  levelOfDetail, out topX, out topY);
		pointX = 0;
		pointY = 0;
		Microsoft.MapPoint.TileSystem.LatLongToPixelXY( point_lat,  point_lng,  levelOfDetail, out pointX, out pointY);

//		print (pointX + "," + pointY + " " + topX + "," + topY + " (" + (pointX - topX) + "," + (pointY - topY) + ")");

		pointX = pointX - topX;
		pointY = pointY - topY;


		GUI.BeginGroup(new Rect(guiPosition.x,guiPosition.y,mapSize,mapSize));
		RenderGUIGuts ();
		GUI.EndGroup();


		GUI.BeginGroup(new Rect(Screen.width/2 + guiPosition.x - 100, guiPosition.y,mapSize,mapSize));
		RenderGUIGuts ();
		GUI.EndGroup();

	}

	void RenderGUIGuts(){
//		var svMat = GUI.matrix; // save current matrix
//		// substitute matrix - only scale is altered from standard
//		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(0.5f, 0.5f, 1.0f));

				int xOffset = 256 - pointX;
				int yOffset = 256 - pointY;

				Color before = GUI.color;
				GUI.color = new Vector4(before.r, before.g, before.b, mapAlpha);
				GUI.DrawTexture(new Rect(xOffset,yOffset,512,512), bigTile, ScaleMode.ScaleToFit, true, 0.0f);
				GUI.color = before;
//		GUI.DrawTexture(new Rect(-256 + xOffset,-256 +yOffset,256,256), mapTiles[0], ScaleMode.ScaleToFit, true, 0.0f);
//		GUI.DrawTexture(new Rect(xOffset,-256 +yOffset,256,256), mapTiles[1], ScaleMode.ScaleToFit, true, 0.0f);
//		GUI.DrawTexture(new Rect(xOffset,yOffset,256,256), mapTiles[2], ScaleMode.ScaleToFit, true, 0.0f);
//		GUI.DrawTexture(new Rect(256 + xOffset,-256 +yOffset,256,256), mapTiles[3], ScaleMode.ScaleToFit, true, 0.0f);
//		GUI.DrawTexture(new Rect(512 + xOffset,-256 +yOffset,256,256), mapTiles[4], ScaleMode.ScaleToFit, true, 0.0f);

//				Vector2 currPosition = new Vector2 (pointX, pointY);
//				Vector2 moveDir = currPosition - prevPosition;

//				Matrix4x4 matrixBackup = GUI.matrix;
//				float a1 = Vector2.Angle (new Vector2 (0, 1), currPosition);
//				float a2 = Vector2.Angle (new Vector2 (0, 1), prevPosition);
//				if (a1 != a2) {
//						print ("gotcha!");
//				} else {
//						print ("a1: " + a1 + " a2: " + a2 + " diff: " + (a1 - a2));
//				}
////				print ("curr: " + currPosition + " prev: " + prevPosition + " move: " + moveDir + " a: " + a);
//				GUIUtility.RotateAroundPivot(a1, new Vector2(256,256));
				//GUI.DrawTexture(rect, texture);
//				GUI.DrawTexture (new Rect(0, 0, 512, 512), ret, ScaleMode.ScaleToFit, true, 0.0f);



				GUI.Button(new Rect(256 - pointSize/2, 256 - pointSize/2, pointSize, pointSize), "", guiStyle);

		int wpX = 0;
		int wpY = 0;
		Microsoft.MapPoint.TileSystem.LatLongToPixelXY( nextWayPoint.x,  nextWayPoint.y,  17, out wpX, out wpY);

		wpX = wpX - topX;
		wpY = wpY - topY;


		int wpOffsetX = wpX - pointX;
		int wpOffsetY = wpY - pointY;


		Color beforeColor = GUI.color;

		GUI.color = Color.red;
		GUI.Button(new Rect(256 + wpOffsetX - pointSize/2, 256 + wpOffsetY - pointSize/2, pointSize, pointSize), "", guiStyle);

		GUI.color = beforeColor;
		GUI.Label(new Rect(128,128, 100, 20), GetElapsedTime().ToString("f2"), timerStyle);
//				GUI.matrix = matrixBackup;


//		GUI.matrix = svMat; // restore matrix
	}

	float GetElapsedTime(){
		return gameObject.GetComponent<WaypointManager> ().GetElapsedGameTime ();
	}


	public void SetMapAlpha(float a){
		mapAlpha = a;
	}

	public void SetNextWaypoint(Vector2 p){
		nextWayPoint.x = p.x;
		nextWayPoint.y = p.y;

	}

	// Update is called once per frame
	void Update () {
		if (mollyClickedHere && gameObject.GetComponent<WaypointManager>().IsWithinVictoryDistance()) {
			GoToCase();
		}

		bool posChanged = false;
		if ( Input.GetKey(KeyCode.UpArrow) ){
			point_lat += debugMoveAmt;
						posChanged = true;
		}
		if ( Input.GetKey(KeyCode.DownArrow) ){
			point_lat -= debugMoveAmt;
						posChanged = true;


		}
		if ( Input.GetKey(KeyCode.RightArrow) ){
			point_lng += debugMoveAmt;
						posChanged = true;



		}
		if ( Input.GetKey(KeyCode.LeftArrow) ){
			point_lng -= debugMoveAmt;
						posChanged = true;

		}

//				if (posChanged) {
//						int pX = 0;
//						int pY = 0;
//						Microsoft.MapPoint.TileSystem.LatLongToPixelXY (point_lat, point_lng, 17, out pX, out pY);
//						prevPosition.x = pX - topX;
//						prevPosition.y = pY - topY;
//
//				}

//		if(Input.GetKey(KeyCode.Space)){
//			needsLerp = true;
//			lerpStarted = Time.time;
//		}
		
		if (Input.GetKeyDown(KeyCode.N)) {
			ws.Send ("{\"case\": \"0\"}");
		}
		if (Input.GetKeyDown(KeyCode.Y)) {
			ws.Send ("{\"case\": \"1\"}");
		}

		if (Input.GetKeyDown (KeyCode.C)) {
			print ("here?");
			GoToCase();
		}
	}

	void GoToCase() {
		print("going to case");
				TextMap.NextMap();

		Application.LoadLevel ("caseLevel1");
//		AsyncOperation async = Application.LoadLevelAsync("caseLevel1");
//		yield return async;
		print ("level loaded");
	}

	Vector2 GetTileNumber(double lat_deg, double lng_deg, int zoom){
		double lat_rad = lat_deg/180.0f * Math.PI;
		double n = Math.Pow(2.0f , zoom);
		int x = (int)((lng_deg + 180.0f) / 360.0f * n);
		int y = (int)((1.0 - Math.Log(Math.Tan(lat_rad) + (1 / Math.Cos(lat_rad))) / Math.PI) / 2.0f * n);
		return new Vector2(x,y);
	}
	Vector2 LatLngForTileNumber(int xTile, int yTile, int zoom){
		double n = Math.Pow(2.0f, zoom);
		double lng_deg = xTile / n * 360.0f - 180.0f;
		double lat_rad = Math.Atan(Math.Sinh(Math.PI * (1-2*yTile/n)));
		double lat_deg = 180.0f * (lat_rad / Math.PI);
		return (new Vector2((float)lat_deg, (float)lng_deg));
	}
}


namespace Microsoft.MapPoint
{
	static class TileSystem
	{
		private const double EarthRadius = 6378137;
		private const double MinLatitude = -85.05112878;
		private const double MaxLatitude = 85.05112878;
		private const double MinLongitude = -180;
		private const double MaxLongitude = 180;
		
		
		/// <summary>
		/// Clips a number to the specified minimum and maximum values.
		/// </summary>
		/// <param name="n">The number to clip.</param>
		/// <param name="minValue">Minimum allowable value.</param>
		/// <param name="maxValue">Maximum allowable value.</param>
		/// <returns>The clipped value.</returns>
		private static double Clip(double n, double minValue, double maxValue)
		{
			return Math.Min(Math.Max(n, minValue), maxValue);
		}
		
		
		
		/// <summary>
		/// Determines the map width and height (in pixels) at a specified level
		/// of detail.
		/// </summary>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <returns>The map width and height in pixels.</returns>
		public static uint MapSize(int levelOfDetail)
		{
			return (uint) 256 << levelOfDetail;
		}
		
		
		
		/// <summary>
		/// Determines the ground resolution (in meters per pixel) at a specified
		/// latitude and level of detail.
		/// </summary>
		/// <param name="latitude">Latitude (in degrees) at which to measure the
		/// ground resolution.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <returns>The ground resolution, in meters per pixel.</returns>
		public static double GroundResolution(double latitude, int levelOfDetail)
		{
			latitude = Clip(latitude, MinLatitude, MaxLatitude);
			return Math.Cos(latitude * Math.PI / 180) * 2 * Math.PI * EarthRadius / MapSize(levelOfDetail);
		}
		
		
		
		/// <summary>
		/// Determines the map scale at a specified latitude, level of detail,
		/// and screen resolution.
		/// </summary>
		/// <param name="latitude">Latitude (in degrees) at which to measure the
		/// map scale.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <param name="screenDpi">Resolution of the screen, in dots per inch.</param>
		/// <returns>The map scale, expressed as the denominator N of the ratio 1 : N.</returns>
		public static double MapScale(double latitude, int levelOfDetail, int screenDpi)
		{
			return GroundResolution(latitude, levelOfDetail) * screenDpi / 0.0254;
		}
		
		
		
		/// <summary>
		/// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
		/// into pixel XY coordinates at a specified level of detail.
		/// </summary>
		/// <param name="latitude">Latitude of the point, in degrees.</param>
		/// <param name="longitude">Longitude of the point, in degrees.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <param name="pixelX">Output parameter receiving the X coordinate in pixels.</param>
		/// <param name="pixelY">Output parameter receiving the Y coordinate in pixels.</param>
		public static void LatLongToPixelXY(double latitude, double longitude, int levelOfDetail, out int pixelX, out int pixelY)
		{
			latitude = Clip(latitude, MinLatitude, MaxLatitude);
			longitude = Clip(longitude, MinLongitude, MaxLongitude);
			
			double x = (longitude + 180) / 360; 
			double sinLatitude = Math.Sin(latitude * Math.PI / 180);
			double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);
			
			uint mapSize = MapSize(levelOfDetail);
			pixelX = (int) Clip(x * mapSize + 0.5, 0, mapSize - 1);
			pixelY = (int) Clip(y * mapSize + 0.5, 0, mapSize - 1);
		}
		
		
		
		/// <summary>
		/// Converts a pixel from pixel XY coordinates at a specified level of detail
		/// into latitude/longitude WGS-84 coordinates (in degrees).
		/// </summary>
		/// <param name="pixelX">X coordinate of the point, in pixels.</param>
		/// <param name="pixelY">Y coordinates of the point, in pixels.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <param name="latitude">Output parameter receiving the latitude in degrees.</param>
		/// <param name="longitude">Output parameter receiving the longitude in degrees.</param>
		public static void PixelXYToLatLong(int pixelX, int pixelY, int levelOfDetail, out double latitude, out double longitude)
		{
			double mapSize = MapSize(levelOfDetail);
			double x = (Clip(pixelX, 0, mapSize - 1) / mapSize) - 0.5;
			double y = 0.5 - (Clip(pixelY, 0, mapSize - 1) / mapSize);
			
			latitude = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
			longitude = 360 * x;
		}
		
		
		
		/// <summary>
		/// Converts pixel XY coordinates into tile XY coordinates of the tile containing
		/// the specified pixel.
		/// </summary>
		/// <param name="pixelX">Pixel X coordinate.</param>
		/// <param name="pixelY">Pixel Y coordinate.</param>
		/// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
		/// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
		public static void PixelXYToTileXY(int pixelX, int pixelY, out int tileX, out int tileY)
		{
			tileX = pixelX / 256;
			tileY = pixelY / 256;
		}
		
		
		
		/// <summary>
		/// Converts tile XY coordinates into pixel XY coordinates of the upper-left pixel
		/// of the specified tile.
		/// </summary>
		/// <param name="tileX">Tile X coordinate.</param>
		/// <param name="tileY">Tile Y coordinate.</param>
		/// <param name="pixelX">Output parameter receiving the pixel X coordinate.</param>
		/// <param name="pixelY">Output parameter receiving the pixel Y coordinate.</param>
		public static void TileXYToPixelXY(int tileX, int tileY, out int pixelX, out int pixelY)
		{
			pixelX = tileX * 256;
			pixelY = tileY * 256;
		}
		
		
		
		/// <summary>
		/// Converts tile XY coordinates into a QuadKey at a specified level of detail.
		/// </summary>
		/// <param name="tileX">Tile X coordinate.</param>
		/// <param name="tileY">Tile Y coordinate.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <returns>A string containing the QuadKey.</returns>
		public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
		{
			StringBuilder quadKey = new StringBuilder();
			for (int i = levelOfDetail; i > 0; i--)
			{
				char digit = '0';
				int mask = 1 << (i - 1);
				if ((tileX & mask) != 0)
				{
					digit++;
				}
				if ((tileY & mask) != 0)
				{
					digit++;
					digit++;
				}
				quadKey.Append(digit);
			}
			return quadKey.ToString();
		}
		
		
		
		/// <summary>
		/// Converts a QuadKey into tile XY coordinates.
		/// </summary>
		/// <param name="quadKey">QuadKey of the tile.</param>
		/// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
		/// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
		/// <param name="levelOfDetail">Output parameter receiving the level of detail.</param>
		public static void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int levelOfDetail)
		{
			tileX = tileY = 0;
			levelOfDetail = quadKey.Length;
			for (int i = levelOfDetail; i > 0; i--)
			{
				int mask = 1 << (i - 1);
				switch (quadKey[levelOfDetail - i])
				{
				case '0':
					break;
					
				case '1':
					tileX |= mask;
					break;
					
				case '2':
					tileY |= mask;
					break;
					
				case '3':
					tileX |= mask;
					tileY |= mask;
					break;
					
				default:
					throw new ArgumentException("Invalid QuadKey digit sequence.");
				}
			}
		}
	}
}
