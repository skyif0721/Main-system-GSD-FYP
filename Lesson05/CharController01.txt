using UnityEngine;
using System.Collections;

public class CharController : MonoBehaviour {

	Animator anim;

	void Start () {
		anim = GetComponent<Animator>();
	}

	void Update () {
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		Vector2 move = new Vector2(h,v);
		anim.SetFloat("speed", move.magnitude);

	}
}