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

		floor = (Material) Resources.Load ("Floor", typeof(Material));
		wall  = (Material) Resources.Load ("Wall",  typeof(Material));

		Random.seed = (int)System.DateTime.Now.Ticks;
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
		drawMap();
	}

	private void drawMap() {
		GameObject yer = GameObject.CreatePrimitive(PrimitiveType.Cube);
		yer.transform.position = new Vector3(50.5f, 0, 50.5f);
		yer.transform.localScale = new Vector3(102.0f, 0.01f, 102.0f);
		yer.renderer.material = floor;
		for(int i=0;i<102;i++) {
			for(int j=0;j<102;j++) {
				if(map[i,j] == 1) {
					GameObject kare = GameObject.CreatePrimitive(PrimitiveType.Cube);
					kare.transform.position = new Vector3(i, 0.5f, j);
					kare.renderer.material = wall;
				} 
			}
		}
	}

	private void lights(int x, int y) {
		GameObject light = new GameObject("mesale");
		light.AddComponent<Light>();
		light.light.color = Color.red;
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
	
	// Update is called once per frame
	void Update () {
	
	}
}
