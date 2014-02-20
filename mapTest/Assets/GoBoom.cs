using UnityEngine;
using System.Collections;

public class GoBoom : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
				if (Input.GetMouseButtonDown (0)) {
						ExploderObject exploder = GameObject.Find("exploder").GetComponent<ExploderObject>();
						exploder.gameObject.SetActive(true);
						exploder.transform.position = transform.position;
						exploder.Explode();
				}
	}
}
