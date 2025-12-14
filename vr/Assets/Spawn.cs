using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour {

	public GameObject item;
	public float rate=0.5f;
	
	void Start () {
		InvokeRepeating ("Duplicate", 0f, rate);
	}

	void Duplicate () {
		GameObject clone = Instantiate(item, transform.position, transform.rotation) as GameObject;
		Physics.IgnoreCollision (clone.GetComponent<Collider>(), GetComponent<Collider>()); 
	}
}