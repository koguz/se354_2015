using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapCell {
	public int wallCount;
	public int type;
	public MapCell() {
		type = 1;
		wallCount = 0;
	}
}

public class RegionConnector {
	public int x;
	public int y;
	public int r1;
	public int r2;

	public RegionConnector(int px, int py, int p1, int p2) {
		x = px; y = py;
		updateRegions(p1, p2);
	}

	public void updateRegions(int p1, int p2) {
		r1 = Mathf.Min(p1, p2);
		r2 = Mathf.Max(p1, p2);
	}
}

public class Room {
	public int x;
	public int y;

	public int w;
	public int h;

	public Room(int size) {
		int min = size/10;
		int max = size/5;
		float av = 0.0f;
		do {
			w = Random.Range(min, max);
			h = Random.Range(min, max);
			av = (float) w / (float) h;
		} while(av > 1.6 || av < 0.6);
		getRandomSpot(size);
	}

	public void getRandomSpot(int size) {
		x = Random.Range (1, size-(w+1));
		y = Random.Range (1, size-(h+1));
	}
}

public class RoomsWithHalls : MonoBehaviour {
	private MapCell[,] map;
	Material floor;
	Material wall;
	private List<Room> rooms;
	private const int maxTries = 400;
	// Use this for initialization
	void Start () {
		floor = (Material) Resources.Load ("Floor", typeof(Material));
		wall  = (Material) Resources.Load ("Wall",  typeof(Material));
		Random.seed = (int)System.DateTime.Now.Ticks;
		map = new MapCell[100,100];
		for(int i=0;i<100;i++) 
			for(int j=0;j<100;j++)
				map[i,j] = new MapCell(); // all tiles are stone

		// 0 - floor
		// 1 - stone
		// 2 - wall (reserved)
		// starting from 3, we mark regions.
		// once we are done connecting the map, we set all 
		// values 3 and above to 0 (make them floor again). 

		rooms = new List<Room>();

		bool moreToAdd = true;
		do {
			// get a room
			Room temp = new Room(100);
			int tries = 0;
			do {
				bool okToAdd = true;
				for(int i=0;i<rooms.Count;i++) {
					if(
						temp.x < rooms[i].x + rooms[i].w+1 &&
						temp.x + temp.w+1 > rooms[i].x &&
						temp.y < rooms[i].y + rooms[i].h+1 &&
						temp.y + temp.h+1 > rooms[i].y
						) {
						okToAdd = false;
					}
				}
				if(okToAdd) {
					rooms.Add(temp);
					break;
				} else {
					tries++;
					temp.getRandomSpot(100);
				}
			} while(tries < maxTries); 
			// Debug.Log (tries);
			if(tries == maxTries) moreToAdd = false;
		} while (moreToAdd);

		for(int i=0;i<rooms.Count;i++) {
			for(int j = rooms[i].x; j < rooms[i].x + rooms[i].w; j++) {
				for(int k = rooms[i].y; k < rooms[i].y + rooms[i].h; k++) {
					map[j,k].type = 3 + i; // = 0;
				}
			}
		}
		// map[] now holds the map with regions marked. 
		// value 3 and above are regions (rooms).
		// create the halls - maze algorithm
		markWalls();
		createMaze (rooms.Count + 3);

		// now, the fun part.
		// all regions are marked with their unique id, starting from 3. 
		// find tiles that have neighbours with different id's. 
		// let's call them probable connecters (PC). 
		// select a random PC and connect two regions. once connected, 
		// the region id should be the same in both. Also, remove other
		// PCs that connects these two regions. 
		connectMap();
		drawMap();
	}

	private void connectMap() {
		// always choose id=3 as the starting case. 
		// but first; the probable connectors. 
		List<RegionConnector> connectors = new List<RegionConnector>();
		for(int i=1;i<99;i++) {
			for(int j=1;j<99;j++) {
				if(map[i,j].type < 3) {
					if(map[i,j-1].type != map[i,j+1].type && map[i,j+1].type > 2 && map[i,j-1].type > 2) {
						// add this
						connectors.Add(new RegionConnector(i, j, map[i,j-1].type, map[i,j+1].type));
					} else if (map[i-1,j].type != map[i+1,j].type && map[i+1,j].type > 2 && map[i-1,j].type > 2) {
						// add this
						connectors.Add(new RegionConnector(i, j, map[i-1,j].type, map[i+1,j].type));
					}
				}
			}
		}

		/* string l = "";
		for(int i=0;i<connectors.Count;i++) l += (connectors[i].x + "," + connectors[i].y + " is from " + connectors[i].r1 + " to " + connectors[i].r2 + "\n");
		Debug.Log (l);
		return;  */

		int zone = 3;
		while(connectors.Count > 0) {
			// get connections for region (zone = 3)
			List<RegionConnector> temp = new List<RegionConnector>();
			for(int i=0;i<connectors.Count;i++) {
				if(connectors[i].r1 == zone) temp.Add(connectors[i]);
			}
			RegionConnector sansli = temp[Random.Range(0, temp.Count)];
			map[sansli.x, sansli.y].type = zone;
			// bucket fill algorithm here. 
			bucketFill(sansli.x, sansli.y, sansli.r1, sansli.r2);
			// remove all from connectors between these two regions. 
			for(int i=0;i<temp.Count;i++) {
				if(temp[i].r1 == sansli.r1 && temp[i].r2 == sansli.r2)
					connectors.Remove (temp[i]);
			}
			// region has expanded, update connectors. 
			for(int i=0;i<connectors.Count;i++) {
				if(connectors[i].r1 == sansli.r2) {
					connectors[i].updateRegions(connectors[i].r2, zone); 
				} else if (connectors[i].r2 == sansli.r2) {
					connectors[i].updateRegions(connectors[i].r1, zone); 
				}
			}
		}


	}

	private void bucketFill(int px, int py, int from, int to) {
		map[px, py].type = from; 

		if(map[px-1,py].type == to) bucketFill (px-1, py, from, to);
		if(map[px+1,py].type == to) bucketFill (px+1, py, from, to);
		if(map[px,py-1].type == to) bucketFill (px, py-1, from, to);
		if(map[px,py+1].type == to) bucketFill (px, py+1, from, to);
	}
	
	// 1 stone
	// 2 wall

	private void markWalls() {
		for(int i=0;i<100;i++) {
			for(int j=0;j<100;j++) {
				if(j-1<0 || j+1 > 99 || i-1<0 || i+1 > 99) {
					map[i,j].wallCount = 2; // mark them inaccessible - this is the border. 
					continue;
				}
				if(map[i,j-1].type > 2) map[i,j].wallCount++;
				if(map[i,j+1].type > 2) map[i,j].wallCount++;
				if(map[i-1,j].type > 2) map[i,j].wallCount++;
				if(map[i+1,j].type > 2) map[i,j].wallCount++;
			}
		}
	}

	private void createMaze(int c) {
		for(int i=0;i<100;i++) {
			for(int j=0;j<100;j++) {
				if(map[i,j].wallCount == 0) {
					// start carving, recursively. 
					carve(i, j, c); 
					c++;
				}
			}
		}

		// go around once more and carve back ones with four walls
		for(int i=1;i<99;i++) {
			for(int j=1;j<99;j++) {
				if(map[i,j].wallCount < 2 && map[i,j].type > 2)
					map[i,j].type = 1;
			}
		}
	}

	private void carve(int px, int py, int c) {
		// if wall count is greater than 1, return immediately. 
		if(map[px,py].wallCount > 1) return;
		// carve current, make it a room. 
		map[px,py].type = c; 
		// increment wall counts around it.
		// don't worry about the borders, they can never be here.
		// also, find out how many directions we can go. 
		// i can only move into tiles with a wall count of one.
		List<Vector2> dirs = new List<Vector2>();
		if(++map[px, py-1].wallCount == 1) dirs.Add(new Vector2( 0, -1));
		if(++map[px, py+1].wallCount == 1) dirs.Add(new Vector2( 0,  1));
		if(++map[px-1, py].wallCount == 1) dirs.Add(new Vector2(-1,  0));
		if(++map[px+1, py].wallCount == 1) dirs.Add(new Vector2( 1,  0));

		if(dirs.Count == 0) {
			// nowhere to go; return. 
			return;
		}

		// shuffle the list, get a random direction.
		// we need other directions when we back-track
		if(dirs.Count > 2) {
			/* if there are more than 2 possible directions to go,
			 * then shuffle. shuffling 2 only changes their places */
			shuffle (dirs);
		}
		else if (dirs.Count == 2 && Random.value < 0.15) {
			/* if there are only two possible directions, then
			 * shuffle only with luck. */	
			shuffle (dirs);
		}
		for(int i=0;i<dirs.Count;i++) {
			map[px,py].wallCount++;
			carve ((int)dirs[i].x+px, (int)dirs[i].y+py, c);
		}
	}

	private void shuffle(List<Vector2> list) {
		for(int n=list.Count-1;n>=1;n--) {
			int j = Random.Range(0, n);
			Vector2 temp = list[j];
			list[j] = list[n];
			list[n] = temp;
		}
	}

	private void printOutMap() {
		string o = "";
		string c = "";
		string n = "";
		for(int i=0;i<100;i++) {
			for(int j=0;j<100;j++) {
				if(map[i,j].type == 1) o += "S";
				else if (map[i,j].type == 2) o += "W";
				else o += "R";
				c += map[i,j].wallCount.ToString();
				n += map[i,j].type;
			}
			o+="\n";
			c+="\n";
		}
		Debug.Log (o);
	}

	private void drawMap() {
		GameObject yer = GameObject.CreatePrimitive(PrimitiveType.Cube);
		yer.transform.position = new Vector3(49.5f, 0, 49.5f);
		yer.transform.localScale = new Vector3(100.0f, 0.01f, 100.0f);
		yer.renderer.material = floor;
		for(int i=0;i<100;i++) {
			for(int j=0;j<100;j++) {
				if(map[i,j].type < 3) {
					GameObject kare = GameObject.CreatePrimitive(PrimitiveType.Cube);
					kare.transform.position = new Vector3(i, 0.5f, j);
					kare.renderer.material = wall;
				} 
			}
		}
	}
	
	// Update is called once per frame
	void Update () {



	}
}
