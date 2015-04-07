using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour {
	private int[,] map;
	Material floor;
	Material wall;

	/* Johnson's paper has a parameter called r which 
	 * represents the percentage of rock cells */
	private float r = 0.5f;
	private int n = 8; /* number of iterations according to Johnson et al is 4 */
	private int T = 5; /* neighbourhood value according to Johnson et al */
	// Use this for initialization
	void Start () {

	}

	public void initMap(int s = 0) {
		floor = (Material) Resources.Load ("Floor", typeof(Material));
		wall  = (Material) Resources.Load ("Wall",  typeof(Material));

		int seedValue;
		if(s == 0) 
			seedValue = (int)System.DateTime.Now.Ticks;
		else seedValue = s;

		Debug.Log (seedValue);

		Random.seed = seedValue;
		map = new int[100,100];
		for(int i=0;i<100;i++) {
			for(int j=0;j<100;j++) {
				if(Random.value < r) {
					map[i,j] = 0;
				} else map[i,j] = 1;
			}
		}

		int[,] temp = new int[100, 100];
		for(int l=0;l<n;l++) {
			// LOOP N times 
			for(int i=0;i<100;i++) {
				for(int j=0;j<100;j++) {
					/* for this particular node, calculate T */
					if(getT(i, j) >= T) temp[i,j] = 1;
					else temp[i,j] = 0;
				}
			}
			// copy temp to map
			for(int i=0;i<100;i++) {
				for(int j=0;j<100;j++) {
					map[i,j] = temp[i,j];
				}
			}
		}
		// temp holds the final map. 
		// create a 102x102 map and copy 100x100 into this, so that
		// the map will be surrounded by walls. 
		map = new int[102, 102];
		for(int i=0;i<102;i++) {
			for(int j=0;j<102;j++) {
				map[i,j] = 1;
			}
		}
		for(int i=1;i<101;i++) {
			for(int j=1;j<101;j++) {
				map[i,j] = temp[i-1,j-1];
			}
		}

		// post processing
		// here's the deal: mark every region and find out the largest one. 
		// make all of the smaller ones walls. 

		// how to mark every region? start from the beginning and when you
		// come accross a floor tile, start bucket filling. 
		int reg = 2; // begin from ID=2 - 0 is floor, 1 is wall
		int maxArea = 0;
		int maxRegn = 2;
		for(int i=1;i<101;i++) {
			for(int j=1;j<101;j++) {
				if(map[i,j] == 0) {
					// calculate the area as you bucket fill 
					int area = bucketFill(i,j,reg);
					if(area > maxArea) {
						maxArea = area;
						maxRegn = reg;
					}
					reg++; // increment for the next region. 
				}
			}
		}

		/* REMOVED THIS */
		/* TODO: if the maxArea is less than 5000, then 
		 * find another way; like re-running the map 
		 * generation algorithm. */
		// here is another trick
		// if the maxArea is less than 5000 tiles, 
		// that means more than half of the region 
		// is walls. we want floors, so i switch the
		// map's floor and wall id's if the maxarea is
		// not big enough. 
		// int floorid = 0; 
		// int wallid  = 1;
		// if(maxArea < 5000) { // 100x100 ) 10.000
		//	floorid = 1; wallid = 0;
		// }

		// Regions are marked, the max area is saved in maxRegn.
		// Go once more through the tiles, remove the smaller regions. 
		for(int i=1;i<101;i++) {
			for(int j=1;j<101;j++) {
				// if tile is max region, make it 1, else if it is not 0 (floor), then
				// it is another region. make it a floor (0). 
				if(map[i,j] == maxRegn) map[i,j] = 0;
				else if(map[i,j] != 0) map[i,j] = 1;
			}
		}

		drawMap();
	}

	private int bucketFill(int x, int y, int r) {
		int toplam = 1; 
		map[x,y] = r;
		if(map[x-1,y] == 0) toplam += bucketFill (x-1, y, r);
		if(map[x+1,y] == 0) toplam += bucketFill (x+1, y, r);
		if(map[x,y-1] == 0) toplam += bucketFill (x, y-1, r);
		if(map[x,y+1] == 0) toplam += bucketFill (x, y+1, r);
		return toplam;
	}

	private void drawMap() {
		GameObject yer = GameObject.CreatePrimitive(PrimitiveType.Cube);
		yer.transform.position = new Vector3(50.5f, 0, 50.5f);
		yer.transform.localScale = new Vector3(102.0f, 0.01f, 102.0f);
		yer.GetComponent<Renderer>().material = floor;
		for(int i=0;i<102;i++) {
			for(int j=0;j<102;j++) {
				if(map[i,j] == 1) {
					GameObject kare = GameObject.CreatePrimitive(PrimitiveType.Cube);
					kare.transform.position = new Vector3(i, 0.5f, j);
					kare.GetComponent<Renderer>().material = wall;
					/* ABOUT rigidbodies AND Colliders:
					 * instead of adding rigidbodies, which makes the 
					 * engine run really slow, add only colliders and
					 * use OnCollisionEnter to kill or remove points from 
					 * the characters: http://docs.unity3d.com/ScriptReference/Collider.OnCollisionEnter.html 
					 * Of course, this OnCollisionEnter should be written for the characters, 
					 * not for all of the cubes here :) 
					 */
					/*kare.AddComponent(typeof(BoxCollider));
					kare.AddComponent(typeof(Rigidbody));
					kare.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;*/
					// kare.rigidbody.isKinematic = true;
				} 
			}
		}
	}

	private void lights(int x, int y) {
		GameObject light = new GameObject("mesale");
		light.AddComponent<Light>();
		light.GetComponent<Light>().color = Color.red;
		light.transform.position = new Vector3((float)x, 0.5f, (float)y);
	}

	private int getT(int x, int y) {
		int TT = 0;
		for(int i=b(x-1,100);i<=b(x+1,100);i++) {
			for(int j=b(y-1,100);j<=b(y+1,100);j++) {
				if(i==j) continue;
				if(map[i,j] == 1) TT++;
			}
		}
		return TT;
	}


	private int b(int v, int m=102) {
		if(v<0) return 0;
		else if(v>=m) return m-1;
		else return v;
	}

	public int[,] getMap() {
		return map;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
