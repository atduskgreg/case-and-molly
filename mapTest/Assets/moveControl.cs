using UnityEngine;
using System.Collections;

public class moveControl : MonoBehaviour {

	public float speed = 10.0f;
	public float gridDistance = 50.0f;
	public GameObject lead;

	public float flickThreshold = 0.2f;
	public float minSecsBetweenFlicks = 1.0f;
	

	float lastFlickAt = 0.00f;
	bool rightFlick = false;
	bool leftFlick = false;

	Vector3 prevW;

	Quaternion prevO;

	// Use this for initialization
	void Start () {
		lead.rigidbody.velocity = new Vector3(0,0,speed);
	}

	void  OnCollisionEnter ( Collision myCollision  ){
		Application.LoadLevel(Application.loadedLevel);
		lastFlickAt = Time.time;
	}
	
	// Update is called once per frame
	void Update () {

//		Vector3 currW = new Vector3();
//		OVRDevice.GetAngularVelocity(0, ref currW.x, ref currW.y, ref currW.z);

		Quaternion currO = new Quaternion();
		OVRDevice.GetOrientation(0, ref currO);

//		print ("x: " + currO.x + " y: "+ currO.y + " z: " + currO.z + " w: " + currO.w);

//		print ("angular velocity: " + "x: " + currW.x + " y: " + currW.y + " z: " + currW.z);
		rightFlick = false;
		leftFlick = false;

		//print("t: " + (Time.time - lastFlickAt) + " currW: " + currO.z);

//		print ("dt: " + (Time.time - lastFlickAt) + " min: " + minSecsBetweenFlicks + " cO: " + currO.z + " pO: " + prevO.z );
		if((Time.time - lastFlickAt) > minSecsBetweenFlicks){
//			rightFlick = (currW.x > 1.0f) && (prevW.x <= 1.0f);
//		 	leftFlick = (currW.x < -1.0f) && (prevW.x >= -1.0f);
			rightFlick = (currO.z < -flickThreshold) && (prevO.z >= -flickThreshold);
			leftFlick = (currO.z > flickThreshold) && (prevO.z <= flickThreshold);


		}
		if(leftFlick || rightFlick){
//			print ("x: " + currW.x + " left: " + leftFlick + " right: " + rightFlick);
			print ("HIT l: " + leftFlick + " r: " + rightFlick);
			lastFlickAt = Time.time;

		}

		prevO = currO;
//		prevW = currW;

		if(Input.GetKeyDown(KeyCode.Escape)){
			Application.LoadLevel(Application.loadedLevel);
		}

		Vector3 p = lead.transform.position;

		bool clickedThisTurn = false;

		if (Input.GetKeyDown(KeyCode.RightArrow) || rightFlick){
			p.x += gridDistance;
			clickedThisTurn= true;
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow) || leftFlick){
			p.x -= gridDistance;
			clickedThisTurn = true;
		}


		if(clickedThisTurn){
			print("clickedThisTurn: " + p.x );
			lead.transform.position = lead.transform.position = new Vector3(p.x, p.y, p.z);
		}

	}
}
