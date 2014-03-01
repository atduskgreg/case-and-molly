using UnityEngine;
using System.Collections;

public class XboxController : MonoBehaviour {

		public float ForceStrength = 3.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
				gameObject.transform.position = new Vector3 (gameObject.transform.position.x + Input.GetAxis ("Horizontal"),
						gameObject.transform.position.y + Input.GetAxis ("Vertical"),
						gameObject.transform.position.z);
				  
//				print("a:" + Input.GetButton("joystick button 16") + " b " + Input.GetButton("joystick button 17") + " x: " + Input.GetButton("joystick button 18") + " y: " + Input.GetButton("joystick button 19"));
//				print ("a:" + Input.GetButton ("Fire1") + " b: " + Input.GetButton ("Fire2") + " x: " + Input.GetButton ("Fire3") + " y: " + Input.GetButton ("Jump"));


				Vector3 forceDir = new Vector3 ();
				if (Input.GetButton ("Fire1")) {
						forceDir.x = 1;
				}

				if (Input.GetButton ("Fire2")) {
						forceDir.x = -1;
				}

				if (Input.GetButton ("Fire3")) {
						forceDir.z = 1;
				}

				if (Input.GetButton ("Jump")) {
						forceDir.z = -1;
				}

				print(forceDir.magnitude);

				if (forceDir.magnitude > 0) {
						gameObject.rigidbody.AddExplosionForce (ForceStrength, forceDir, 0.0f, 0.1f, ForceMode.Impulse);

				}


//				print(Input.GetAxis ("Horizontal") + "x" + Input.GetAxis ("Vertical"));
	}
}
