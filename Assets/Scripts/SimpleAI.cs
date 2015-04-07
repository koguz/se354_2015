using UnityEngine;
using System.Collections;

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
	public void Enter(SimpleAI m) {
		// Get 4 random points around yourself (n squares)
		// the 5th should be the current point, so 
		// that we can return to this position. 

	}
	public void Execute(SimpleAI m) {
	}
	public void Exit(SimpleAI m) {
	}
}

public class SimpleAI : MonoBehaviour {
	public State currentState; 
	// Use this for initialization
	void Start () {
		currentState = null;
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
