using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
using Shiva.ItemPlacing;

public class PlaceItem : MonoBehaviour
{

	public Sprite sprite;

	public bool movable = true;
	public bool destroiable = true;
	public bool rotatable = false;
	public bool scalable = true;
	public bool commnetable = true;

	private Stack<List<SerializablePart>> undoStack = new Stack<List<SerializablePart>> ();

	public string itemName;

	[System.Serializable]
	private class LocRotScale : SerializablePart{
		public Vector3 loc;
		public Quaternion rot;
		public Vector3 scale;

		[System.NonSerialized]
		public Transform to;

		public static LocRotScale Copy (Transform o)
		{
			LocRotScale lrs = new LocRotScale ();
			
			lrs.loc = Copy(o.position);
			lrs.rot = Copy(o.rotation);
			lrs.scale = Copy(o.localScale);
			
			return lrs;
		}

		public override object StartEditing (PlaceItemExtension pie)
		{
			loc = pie.transform.position;
			rot = pie.transform.rotation;
			scale = pie.transform.localScale;

			return this;
		}

		public override void EndEditing ()
		{
		}

		public override void Undo (GameObject o)
		{
			o.transform.position = loc;
			o.transform.rotation = rot;
			o.transform.localScale = scale;
		}

		public override bool HasChanged ()
		{
			return to.position != loc || to.rotation != rot || to.localScale != scale;
		}
	}

	private List<SerializablePart> selParts = new List<SerializablePart>();

	private PlaceItemExtension[] pies;

	public void StartEditing(){
		
		selParts.Clear ();

		LocRotScale lrs = LocRotScale.Copy (transform);
		lrs.to = transform;
		selParts.Add (lrs);

		pies = GetComponents<PlaceItemExtension> ();
		foreach (PlaceItemExtension pie in pies) {
			SerializablePart cp = pie.GetCopy().StartEditing(pie) as SerializablePart;
			selParts.Add (cp);
		}

	}

	public void EndEditing(){

		bool flag = false;
		foreach (SerializablePart sp in selParts) {
			if(sp.HasChanged()){
				flag = true;
			}
			sp.EndEditing();
		}

		if (!flag)
			return;

		List<SerializablePart> sps = new List<SerializablePart> ();
		sps.AddRange (selParts);
		
		undoStack.Push (sps);
		FindObjectOfType<PlaceItemMaster>().undoButton.interactable = IsUndoable ();

		StartEditing ();
	}

	public bool IsUndoable(){
		return undoStack.Count > 0;
	}

	public void EditUndo(){
		if (IsUndoable()) {

			List<SerializablePart> lrs = undoStack.Pop ();
			foreach(SerializablePart sp in lrs)
				sp.Undo(gameObject);

		}
	}


	void Start ()
	{
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
