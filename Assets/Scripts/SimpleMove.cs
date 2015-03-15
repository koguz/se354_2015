using UnityEngine;
using System.Collections;

public class SimpleMove : MonoBehaviour {
	public float speed = 10.0F;
	public float rotationSpeed = 100.0F;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	// from http://docs.unity3d.com/ScriptReference/Input.GetAxis.html
	void Update () {
		float translation = Input.GetAxis("Vertical") * speed;
		float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
		translation *= Time.deltaTime;
		rotation *= Time.deltaTime;
		transform.Translate(0, 0, translation);
		transform.Rotate(0, rotation, 0);
	}
}
