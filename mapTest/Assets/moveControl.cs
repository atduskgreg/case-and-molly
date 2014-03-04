using UnityEngine;
using System.Collections;

public class moveControl : MonoBehaviour {

	public float speed = 10.0f;
	public float gridDistance = 50.0f;
	public GameObject lead;
	public GameObject exit;

	public float flickThreshold = 0.2f;
	public float minSecsBetweenFlicks = 1.0f;
	

	float lastFlickAt = 0.00f;
	bool rightFlick = false;
	bool leftFlick = false;
  public Vector2 guiPosition = new Vector2(128,128);

  public Texture tiltWarning;
  public Texture wallWarning;

    float levelStartedAt = 0.0f;

  public GameObject soundManager;

  static bool crashed = false;

	Vector3 prevW;

	Quaternion prevO;

	// Use this for initialization
	void Start () {
		lead.rigidbody.velocity = new Vector3(0,0,speed);

    if (crashed){
//      soundManager.GetComponents<AudioSource>()[1].Play ();
      //soundManager.GetComponents<AudioSource>()[1].Play ();

      moveControl.crashed = false;
     }

    levelStartedAt = Time.time;

	}

	void  OnCollisionEnter ( Collision myCollision  ){

		Application.LoadLevel(Application.loadedLevel);
		lastFlickAt = Time.time;
    moveControl.crashed = true;


	}

  void DrawGUIWarning(){
//    GUI.color = Color.green;
//    GUI.Label(new Rect(128,128, 100, 20), GetElapsedTime().ToString("f2"), timerStyle); 
    GUI.DrawTexture(new Rect(0,0,512,512), tiltWarning, ScaleMode.ScaleToFit, true, 0.0f);
  }

  void OnGUI(){
    if ((Time.time - levelStartedAt) > 1.5f && (Time.time - levelStartedAt) < 3.0f){
      GUI.BeginGroup(new Rect(guiPosition.x, guiPosition.y, 512, 512));
      GUI.DrawTexture(new Rect(0,0,512,512), tiltWarning, ScaleMode.ScaleToFit, true, 0.0f);
      GUI.EndGroup();


      GUI.BeginGroup(new Rect(Screen.width / 2 + guiPosition.x - 100, guiPosition.y, 512, 512));
      GUI.DrawTexture(new Rect(0,0,512,512), tiltWarning, ScaleMode.ScaleToFit, true, 0.0f);
      GUI.EndGroup();
      } 
    else if ((Time.time - levelStartedAt) > 3.5f && (Time.time - levelStartedAt) < 4.5f){
      GUI.BeginGroup(new Rect(guiPosition.x, guiPosition.y, 512, 512));
      GUI.DrawTexture(new Rect(0,0,512,512), wallWarning, ScaleMode.ScaleToFit, true, 0.0f);
      GUI.EndGroup();


      GUI.BeginGroup(new Rect(Screen.width / 2 + guiPosition.x - 100, guiPosition.y, 512, 512));
      GUI.DrawTexture(new Rect(0,0,512,512), wallWarning, ScaleMode.ScaleToFit, true, 0.0f);
      GUI.EndGroup();
    }
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




       rightFlick = (currO.z < -flickThreshold);// && (prevO.z >= -flickThreshold);
      leftFlick = (currO.z > flickThreshold);// && (prevO.z <= flickThreshold);


		}
		if(leftFlick || rightFlick){
//			print ("x: " + currW.x + " left: " + leftFlick + " right: " + rightFlick);
			print ("HIT l: " + leftFlick + " r: " + rightFlick);
			lastFlickAt = Time.time;
		}

		prevO = currO;
//		prevW = currW;

//		print (gameObject.transform.position.z);
		if (gameObject.transform.position.z >= exit.transform.position.z) {
			Application.LoadLevel ("mollyLevel");
//			
		}

		if(Input.GetKeyDown(KeyCode.Escape)){
			Application.LoadLevel(Application.loadedLevel);
		}

		Vector3 p = lead.transform.position;

		bool clickedThisTurn = false;

		if (Input.GetKeyDown(KeyCode.RightArrow) || rightFlick){
			p.x += gridDistance;
			clickedThisTurn= true;
						soundManager.GetComponents<AudioSource>()[1].Play ();

		}

		if (Input.GetKeyDown(KeyCode.LeftArrow) || leftFlick){
			p.x -= gridDistance;
			clickedThisTurn = true;
      	soundManager.GetComponents<AudioSource>()[1].Play ();

		}


		if(clickedThisTurn){
			print("clickedThisTurn: " + p.x );
			lead.transform.position = new Vector3(p.x, p.y, p.z);
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			print ("here");
			lead.rigidbody.velocity = new Vector3(0,0,0);
		}

	}
}
