using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface State {
	void Enter(SimpleAI m);
	void Execute(SimpleAI m);
	void Exit(SimpleAI m);
}

public class StateIdle:State {
	float upTime;
	public void Enter(SimpleAI m) {
		upTime = Time.time;
	}
	public void Execute(SimpleAI m) {
		if(Time.time - upTime > 10) {
			// Roll the dice to decide whether to take a walk
			if(Random.value > 0.75) {
				m.ChangeState(new StateWander());
			}
			else {
				// maybe next time
				upTime = Time.time;
			}
		}
	}
	public void Exit(SimpleAI m) {
		// nothing to clean up
	}
}

public class StateWander:State {
	private List<Vector3> targets; 
	private int currentId;
	public void Enter(SimpleAI m) {
		// Get 4 random points around yourself (n squares)
		// the 5th should be the current point, so 
		// that we can return to this position. 
		targets = new List<Vector3>();
		int cx = (int)m.transform.position.x;
		int cy = (int)m.transform.position.z;
		int [,] map = GameObject.Find("TheGame").GetComponent<TheGame>().getMap();
		for(int i=1;i<4;i++) {
			bool devam = true;
			do {
				int sx = cx - 6; if(sx < 0) sx = 0;
				int ex = cx + 6; if(ex > map.GetLength(0)) ex = map.GetLength(0);
				int secx = Random.Range(sx, ex);

				int sy = cy - 6; if(sy < 0) sy = 0;
				int ey = cy + 6; if(ey > map.GetLength(1)) ey = map.GetLength(1);
				int secy = Random.Range(sy, ey);
				if(map[secx, secy] == 0) {
					devam = false;
					targets.Add(new Vector3(secx, 0, secy));
				}
			} while(devam);
		}
		targets.Add(new Vector3(cx, 0, cy));
		currentId = 0;
	}
	public void Execute(SimpleAI m) {
		Vector3 distance = m.transform.position - (Vector3)targets[currentId];
		if (distance.magnitude < 0.5) {
			if(++currentId == targets.Count) {
				m.ChangeState(new StateIdle());
				return;
			}
		}
		m.seek.target = (Vector3) targets[currentId];
		m.align.target = Mathf.Atan2(m.seek.velocity.x, m.seek.velocity.z) * Mathf.Rad2Deg;
	}
	public void Exit(SimpleAI m) {
		targets.Clear();
	}
}

public class SimpleAI : MonoBehaviour {
	public State currentState; 
	public DynamicAlign align;
	public DynamicSeek seek;
	// Use this for initialization
	void Start () {
		currentState = null;
		align = gameObject.AddComponent<DynamicAlign>();
		seek = gameObject.AddComponent<DynamicSeek>();
		ChangeState(new StateIdle());
	}
	
	// Update is called once per frame
	void Update () {
		if(currentState != null) currentState.Execute(this);
	}

	public void ChangeState(State n) {
		if(currentState != null) {
			currentState.Exit(this);
		} 
		currentState = n; 
		currentState.Enter(this);
	}
}
