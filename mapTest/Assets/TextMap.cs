using UnityEngine;
using System.Collections;
using System;

public class TextMap : MonoBehaviour {

		//public TextAsset textMap;
	public float zSize = 30.0f;
	public float xSize = 20.0f;
    public float ySize = 10.0f;
	public GameObject exit;
	public TextAsset[] maps;

  public GameObject top;
  public GameObject floor;
	
	static int currentMap = -1;


	int WidthOfLine(string line){
		int wUnits = 0;
		char[] chars = line.ToCharArray ();
		for (int i = 0; i < chars.Length; i++) {
				if (chars [i] == ' ' || chars [i] == '_') {
						wUnits++;
				}
		}
		return wUnits;
	}

  int LengthOfMap(string map){
    string[] lines = map.Split(Environment.NewLine.ToCharArray());
    int result = 0;
    for(int i = 0; i < lines.Length; i++){
      if(lines[i].IndexOf('|') != -1){
        result++;
      }
    }
    return result;
  }

		public static void NextMap(){
				TextMap.currentMap++;
		}

	void Start () {
    // support starting on caseLevel for debugging
    if (TextMap.currentMap == -1){
      TextMap.currentMap = 0;
    }


    int l = LengthOfMap(maps[TextMap.currentMap].text);
    print("l: " + l);
    top.transform.localScale = new Vector3(200, 1, 3.5f * l);
    floor.transform.localScale = new Vector3(200, 1, 4.5f * l);

    string[] lines = maps[TextMap.currentMap].text.Split(Environment.NewLine.ToCharArray());

		float x = 0.0f;//-WidthOfLine(lines[0])/2.0f;
        float z = 0.0f;
		for(int i = 0; i < lines.Length; i++){
			char[] chars = lines[i].ToCharArray();
            bool anyForwardWalls = false;
			bool rightFacing = true;

			int lineWidth = WidthOfLine (lines [0]);
			x = -(lineWidth * xSize)/2.0f + xSize/2.0f;
        int numWalls = 0;

			for(int j = 0; j < chars.Length; j++){


				if (chars[j] == ' ') {
//					print( "space" );
          x += xSize;
				}

				if (chars[j] == '|') {
          anyForwardWalls = true;
                numWalls++;
//					print( "forward wall" );
					Quaternion q = new Quaternion ();
                   
					if (rightFacing) { 
						q.SetFromToRotation (Vector3.up, Vector3.right);
					} else {
						q.SetFromToRotation (Vector3.up, Vector3.left);
					}

          rightFacing = !rightFacing;


//          if (numWalls > 1 && rightFacing){
//            x += xSize;
//          }			  
//
          if (numWalls == 3){
            x += xSize * 2;
          }
					GameObject w = (GameObject)Instantiate(Resources.Load("Wall"), new Vector3(x,ySize*5, z), q);

					w.transform.localScale = new Vector3(ySize,1,zSize/10.0f);


         
				}

        if (chars[j] == '_') {
//					print( "blocking wall" );
					Quaternion q = new Quaternion ();
          q.eulerAngles = new Vector3 (0, 90, 270);

					GameObject w = (GameObject)Instantiate(Resources.Load("Wall"), new Vector3(x + xSize/2,ySize*5, z + zSize/2), q);
					w.transform.localScale = new Vector3(ySize,1,xSize/10.0f);

          x += xSize;
				}	

			if (chars [j] == 'x') {
          		exit.transform.position = new Vector3(x+7, 3, z+25);
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
