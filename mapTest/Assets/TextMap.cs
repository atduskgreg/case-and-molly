using UnityEngine;
using System.Collections;
using System;

public class TextMap : MonoBehaviour {

	public TextAsset textMap;
    public float zSize = 7.5f;
    public float xSize = 10.0f;
    public float ySize = 10.0f;


	string map = "|  |\n|  |\n|  |\n|_ |\n | |";
//	public GameObject wall;

	// Use this for initialization
	void Start () {
		string[] lines = textMap.text.Split(Environment.NewLine.ToCharArray());

        float x = 0.0f;
        float z = 0.0f;
		for(int i = 0; i < lines.Length; i++){
			char[] chars = lines[i].ToCharArray();
            bool anyForwardWalls = false;
            x = 0;

			for(int j = 0; j < chars.Length; j++){

                print(i + " " + j);
				if (chars[j] == ' ') {
					print( "space" );
                    x += xSize;
				}

				if (chars[j] == '|') {
                    anyForwardWalls = true;
					print( "forward wall" );
					Quaternion q = new Quaternion ();
                    // this is a hack, only correct if path is in middle of world.
                    // need to detect "inside" of walls (state based on counting forward walls per row)
                    if (j <= chars.Length/2) { 
						q.SetFromToRotation (Vector3.up, Vector3.right);
					} else {
						q.SetFromToRotation (Vector3.up, Vector3.left);
					}

					int p = j;
					if (j == (chars.Length - 1)) {
					    p = j - 1;
                    }   


                    GameObject w = (GameObject)Instantiate(Resources.Load("Wall"), new Vector3(x,5, z), q);
				}

                if (chars[j] == '_') {
					print( "blocking wall" );
					Quaternion q = new Quaternion ();
//                    q.SetFromToRotation (Vector3.up, Vector3.back);
                    q.eulerAngles = new Vector3 (0, 90, 270);

                    GameObject w = (GameObject)Instantiate(Resources.Load("Wall"), new Vector3(x + xSize/2,5, z + zSize/2), q);
                    w.transform.localScale = new Vector3(1,1,1);
                    Vector3 v = new Vector3 (0, 0, 0);
//                    w.transform.rotation;
                    x += xSize;

				}	

			}
            if (anyForwardWalls) {
                z += zSize;
            }

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
