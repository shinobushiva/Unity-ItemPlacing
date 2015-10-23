using UnityEngine;
using System.Collections;

public class VisibilityReport : MonoBehaviour {
	
	public bool visible = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnBecameVisible(){
		visible = true;
	}
	
	void OnBecameInvisible(){
		visible = false;
	}
}
