using UnityEngine;
//using System.Collections;
using System.Collections.Generic;

public class PlaceItem : MonoBehaviour
{

	public Sprite sprite;

	public bool movable = true;
	public bool destroiable = true;
	public bool rotatable = false;
	public bool scalable = true;
	public bool commnetable = true;

	private Stack<LocRotScale> undoStack;

	public string itemName;

	[System.Serializable]
	private class LocRotScale{
		public Vector3 loc;
		public Quaternion rot;
		public Vector3 scale;
	}

	private LocRotScale lrs;
	public void StartEditing(){
		lrs = new LocRotScale ();
		lrs.loc = Copy(transform.position);
		lrs.rot = Copy(transform.rotation);
		lrs.scale = Copy(transform.localScale);
	}

	private Vector3 Copy(Vector3 v){
		Vector3 v2 = new Vector3 ();
		v2.x = v.x;
		v2.y = v.y;
		v2.z = v.z;
		return v2;
	}

	private Quaternion Copy(Quaternion v){
		Quaternion v2 = new Quaternion ();
		v2.x = v.x;
		v2.y = v.y;
		v2.z = v.z;
		v2.w = v.w;
		return v2;
	}

	public void EndEditing(){
		if (lrs.loc == transform.position && lrs.rot == transform.rotation && lrs.scale == transform.localScale)
			return;
		
		undoStack.Push (lrs);
	}


	public void EditUndo(){
		if (undoStack.Count > 0) {
			print ("Undo");

			LocRotScale lrs = undoStack.Pop ();
			transform.position = lrs.loc;
			transform.rotation = lrs.rot;
			transform.localScale = lrs.scale;
		}
	}


	void Start ()
	{
		undoStack = new Stack<LocRotScale> ();
		if (itemName == null || itemName.Length <= 0) {
			itemName = gameObject.name;
		}
	}

	public void SetInActive(){
		gameObject.SetActive (false);
		if(GetComponent<NetworkView>())
			GetComponent<NetworkView>().RPC("SetInActiveRPC", RPCMode.Others);
	}
	
	[RPC]
	void SetInActiveRPC(){
		gameObject.SetActive (false);
	}
}
