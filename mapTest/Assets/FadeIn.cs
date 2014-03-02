using UnityEngine;
using System.Collections;

public class FadeIn : MonoBehaviour {

  public float fadeTime = 3.0f;
  float startTime = 0.0f;


	// Use this for initialization
	void Start () {
    startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
    float t = Mathf.Clamp(Time.time - startTime, 0, fadeTime) / fadeTime;
//    print(Mathf.Clamp(Time.time - startTime, 0, 1500));
    renderer.material.SetColor("_Color", new Color(t, t, t, 1));
	}
}
