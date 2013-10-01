using UnityEngine;
using System.Collections;

public class InitGame : MonoBehaviour {	
	public Color backgroundColor;
	
	public int numCubes = 5;
	public int maxDistance = 20;
	public GameObject cube;
	GameObject[] cubes;
	
	int currentCube = -1;
	public GameObject ovrParent;
	
	float lerpStarted = 0.0f;
	public float lerpTime = 0.5f;
	float moveLength = 0.0f;
	Vector3 dest;
	Vector3 prevDest;
	
	float movePercent = 1.0f;
	
	public Camera cameraLeft;
	public Camera cameraRight;
	
	ArrayList userSort;
	public GameObject lineHead;
	
	void cleanupCameras(){
		
		cameraLeft.backgroundColor = backgroundColor;//new Color(0.1f, 0.1f, 0.1f);
		cameraRight.backgroundColor = backgroundColor;//new Color(0.1f, 0.1f, 0.1f);
		
		//renderer.material.mainTexture.wrapMode = TextureWrapMode.Clamp;
		
	}
	
	// Use this for initialization
	void Start () {
		cleanupCameras();
		
		cubes = new GameObject[numCubes];
		for(int i = 0; i < numCubes; i++){
			Vector3 position = Random.onUnitSphere * Random.Range(maxDistance/2, maxDistance);
			GameObject o = GameObject.Instantiate(cube, position, Quaternion.identity) as GameObject;
			

			BlinkySorty bs = o.GetComponent<BlinkySorty>();
			
			//bs.startColor.r = Random.value;
						
			bs.numBlinks = (int)Random.Range(1, 7);
			cubes[i] = o;			
		}	
		
		dest = ovrParent.transform.position;	
		
		userSort = new ArrayList();
	}
	
	void NextCube(){
			currentCube++;
			if(currentCube > numCubes-1){
				currentCube = 0;
			}
						
			prevDest = dest;
			dest = cubes[currentCube].gameObject.transform.position - (Vector3.back * 2);	
			moveLength = Vector3.Distance(ovrParent.transform.position, dest);
			
			lerpStarted = Time.time;
	}
	
	void OnGUI(){
		Event e = Event.current;
		if(e.isKey && e.keyCode == KeyCode.RightArrow && movePercent == 1){
			NextCube();
		}
		else if(e.isKey && Input.GetKeyDown(KeyCode.Space) && movePercent == 1){
			
			if(!userSort.Contains(cubes[currentCube])){
				userSort.Add(cubes[currentCube]);
			} else {
				userSort.Remove(cubes[currentCube]);
				userSort.Add(cubes[currentCube]);
			}
			
			int i = 0;
			foreach(GameObject item in userSort){
				item.GetComponent<BlinkySorty>().GoTo(lineHead.transform.position + (Vector3.left * 2 * i));
				i++;
			}
			
			//NextCube();
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		if(moveLength > 0){
			movePercent = (Time.time - lerpStarted)/lerpTime;
			movePercent = Mathf.Clamp(movePercent, 0, 1);
			ovrParent.transform.position = Vector3.Lerp(prevDest, dest, movePercent);
			ovrParent.transform.LookAt(cubes[currentCube].gameObject.transform.position);
		}
		
	}
}
