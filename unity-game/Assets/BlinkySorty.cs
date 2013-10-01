using UnityEngine;
using System.Collections;

public class BlinkySorty : MonoBehaviour {
	
	public Color startColor;
	public Color darkColor;
	public float blinkDuration = 0.4f;
	public int numBlinks = 2;

	int blinkCount = 0;
	float prevLerp = 0;
	bool isBlinking = false;
	float blinkStartTime = 0;
	
	Vector3 goTo;
	bool shouldGoTo = false;
	float moveStarted;
	public float moveTime = 1.0f;
	Vector3 startedFrom;
	// Use this for initialization
	void Start () {
		renderer.material.color = Color.black;
	}
	
	void OnGUI(){
		Event e = Event.current;
		if(e.isMouse && Input.GetMouseButtonDown(0)){
			isBlinking = true;
			blinkCount = 0;
			blinkStartTime = Time.time;
		}
	}
	
	public void GoTo(Vector3 v){
		goTo = v;
		shouldGoTo = true;
		moveStarted = Time.time;
		startedFrom = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
		if(isBlinking && blinkCount < numBlinks){
		
			//float lerp = Mathf.PingPong(Time.time - blinkStartTime, blinkDuration)/blinkDuration;
			float lerp = Mathf.Sin((Time.time - blinkStartTime) * 10);
			renderer.material.color = Color.Lerp(darkColor, startColor, lerp);
		
			if(prevLerp < 0.5 && lerp > 0.5){
				blinkCount++;
			}
		
			prevLerp = lerp;
		} else {
			//float lerp = Mathf.PingPong(Time.time - blinkStartTime, blinkDuration)/blinkDuration;
			float lerp = Mathf.Sin((Time.time - blinkStartTime) * 4.5f);
			renderer.material.color = Color.Lerp(renderer.material.color, darkColor, lerp);
		}
		
		if(shouldGoTo){
			float movePercent = (Time.time - moveStarted)/moveTime;
			movePercent = Mathf.Clamp (movePercent, 0,1);
			
			transform.position = Vector3.Lerp(startedFrom, goTo, movePercent);
			
			/*
			 * movePercent = (Time.time - lerpStarted)/lerpTime;
			movePercent = Mathf.Clamp(movePercent, 0, 1);
			ovrParent.transform.position = Vector3.Lerp(prevDest, dest, movePercent);
			ovrParent.transform.LookAt(cubes[currentCube].gameObject.transform.position);
			 * 
			 */
			
		}
		
	}
}
