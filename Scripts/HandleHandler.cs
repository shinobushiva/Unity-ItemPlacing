using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DataSaveLoad;
using Shiva.CameraSwitch;
using System.IO;
using System.Xml.Serialization;

namespace Shiva.ItemPlacing
{
	[System.Serializable]
	public class HandleHandler
	{

		private PlaceItemMaster master;
		
		public GameObject xHandle;
		public GameObject yHandle;
		public GameObject zHandle;
//		private GameObject plane;
		
		public GameObject xRotateHandle;
		public GameObject yRotateHandle;
		public GameObject zRotateHandle;
		//
		public Transform handle;

		public GameObject movePref;
		public GameObject rotatePref;
		public GameObject scalePref;

		public HandleHandler ()
		{
		}

		public void Init (PlaceItemMaster master)
		{
			this.master = master;
			this.movePref = master.movePref;
			this.rotatePref = master.rotatePref;
			this.scalePref = master.scalePref;

			xHandle = GameObject.Instantiate (movePref);
			xHandle.GetComponent<Renderer> ().material.color = new Color (1, 0, 0, .2f);
			xHandle.gameObject.name = "X Handle";
			yHandle = GameObject.Instantiate (movePref);
			yHandle.GetComponent<Renderer> ().material.color  = new Color (0, 1, 0, .2f);
			yHandle.gameObject.name = "Y Handle";
			zHandle = GameObject.Instantiate (movePref);
			zHandle.GetComponent<Renderer> ().material.color = new Color (0, 0, 1, .2f);
			zHandle.gameObject.name = "Z Handle";

			xRotateHandle = GameObject.Instantiate (rotatePref);
			xRotateHandle.GetComponent<Renderer> ().material.color = new Color (1, 0, 0, .2f);
			xRotateHandle.gameObject.name = "X Rotate Handle";
			yRotateHandle = GameObject.Instantiate (rotatePref);
			yRotateHandle.GetComponent<Renderer> ().material.color  = new Color (0, 1, 0, .2f);
			yRotateHandle.gameObject.name = "Y Rotate Handle";
			zRotateHandle = GameObject.Instantiate (rotatePref);
			zRotateHandle.GetComponent<Renderer> ().material.color = new Color (0, 0, 1, .2f);
			zRotateHandle.gameObject.name = "Z Rotate Handle";
			
//			plane = GameObject.CreatePrimitive (PrimitiveType.Plane);
		}

		private GameObject targetObject;
		
		
		Vector3 pickPosOnScreen;
		Vector3 prevPosOnScreen;
		Vector3 pickPos;

		Vector3 targetPos;
		Quaternion targetRot;
		Vector3 targetScale;

		public void Update (GameObject targetObject)
		{

			this.targetObject = targetObject;

			xHandle.gameObject.SetActive (false);
			yHandle.gameObject.SetActive (false);
			zHandle.gameObject.SetActive (false);

			xRotateHandle.gameObject.SetActive (false);
			yRotateHandle.gameObject.SetActive (false);
			zRotateHandle.gameObject.SetActive (false);

			if (targetObject != null) {
				Quaternion rot = master.selectionBox.transform.rotation;
				master.selectionBox.transform.rotation = Quaternion.identity;
				Bounds b = Helper.GetBoundingBox (master.selectionBox);
				master.selectionBox.transform.rotation = rot;

//				b = master.selectionBox.GetComponent<Renderer>().bounds;

				if(master.editState == PlaceItemMaster.State.Moving){
					xHandle.gameObject.SetActive (true);
					yHandle.gameObject.SetActive (true);
					zHandle.gameObject.SetActive (true);

					float r = b.extents.magnitude/4;
					xHandle.transform.localScale = new Vector3 (r, r*8, r);
					yHandle.transform.localScale = new Vector3 (r, r*8, r);
					zHandle.transform.localScale = new Vector3 (r, r*8, r);

				
					xHandle.transform.position = b.center;
					xHandle.transform.rotation = Quaternion.LookRotation(targetObject.transform.right, targetObject.transform.forward);
					xHandle.transform.Rotate (Vector3.right * 90, Space.Self);
				
					yHandle.transform.position = b.center;
					yHandle.transform.rotation = targetObject.transform.rotation;
					//				yHandle.transform.Rotate (targetObject.transform.forward * 90, Space.World);
				
					zHandle.transform.position = b.center;
					zHandle.transform.rotation = Quaternion.LookRotation(targetObject.transform.right, targetObject.transform.forward);
					zHandle.transform.Rotate (Vector3.up * 90, Space.Self);
				}

				if(master.editState == PlaceItemMaster.State.Rotating){
					xRotateHandle.gameObject.SetActive (true);
					yRotateHandle.gameObject.SetActive (true);
					zRotateHandle.gameObject.SetActive (true);
					
					float r = b.extents.magnitude;
					xRotateHandle.transform.localScale = new Vector3 (r, r, r);
					yRotateHandle.transform.localScale = new Vector3 (r, r, r);
					zRotateHandle.transform.localScale = new Vector3 (r, r, r);

					xRotateHandle.transform.position = b.center;
					xRotateHandle.transform.rotation = Quaternion.LookRotation(targetObject.transform.right);

					yRotateHandle.transform.position = b.center;
					yRotateHandle.transform.rotation = Quaternion.LookRotation(targetObject.transform.up);

					zRotateHandle.transform.position = b.center;
					zRotateHandle.transform.rotation = targetObject.transform.rotation;
				}

			} 

		}

		public void PickHandle (Camera c)
		{
			handle = null;
			bool b;
			Ray ray = c.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			b = Physics.Raycast (ray, out hit);
			if (b) {
				GameObject go = hit.transform.gameObject;

				if (xHandle == go) {
					handle = xHandle.transform;
				}
				if (yHandle == go) {
					handle = yHandle.transform;
				}
				if (zHandle == go) {
					handle = zHandle.transform;
				}

				if (xRotateHandle == go) {
					handle = xRotateHandle.transform;
				}
				if (yRotateHandle == go) {
					handle = yRotateHandle.transform;
				}
				if (zRotateHandle == go) {
					handle = zRotateHandle.transform;
				}

				if(handle != null){
					pickPosOnScreen = Input.mousePosition;
					prevPosOnScreen = pickPosOnScreen;
					targetPos = targetObject.transform.position;
					targetRot = targetObject.transform.localRotation;
					targetScale = targetObject.transform.localScale;

					pickPos = hit.point;
				}
				return;
				
			}
			handle = null;
		}

		public void MoveHandle (Camera c)
		{

			if (handle == null)
				return;

			
			if (xHandle == handle.gameObject) {

				Vector3 s1 = c.WorldToScreenPoint (pickPos);
				Vector3 s2 = c.WorldToScreenPoint (pickPos + targetObject.transform.right);
				Vector3 s3 = c.WorldToScreenPoint (pickPos - targetObject.transform.right);
				
				float dist = (pickPosOnScreen - Input.mousePosition).magnitude;
			
				if (Vector3.Distance (Input.mousePosition, s2) > Vector3.Distance (Input.mousePosition, s3)) {
					dist = -dist;
				} else {
				}

//				Debug.Log (dist);
				if (Mathf.Abs (dist) > 2) {
					targetObject.transform.position = targetPos + 
						targetObject.transform.right * dist / Vector3.Distance (s1, s2);

					Update (targetObject);
					master.UpdateSelectionBox ();
				}
			}

			if (yHandle == handle.gameObject) {
				
				Vector3 s1 = c.WorldToScreenPoint (pickPos);
				Vector3 s2 = c.WorldToScreenPoint (pickPos + targetObject.transform.up);
				Vector3 s3 = c.WorldToScreenPoint (pickPos - targetObject.transform.up);
				
				float dist = (pickPosOnScreen - Input.mousePosition).magnitude;

				if (Vector3.Distance (Input.mousePosition, s2) > Vector3.Distance (Input.mousePosition, s3)) {
					dist = -dist;
				} else {
				}

				if (Mathf.Abs (dist) > 2) {
					targetObject.transform.position = targetPos + 
						targetObject.transform.up * dist / Vector3.Distance (s1, s2);
					
					Update (targetObject);
					master.UpdateSelectionBox ();
				}
			}

			if (zHandle == handle.gameObject) {
				
				Vector3 s1 = c.WorldToScreenPoint (pickPos);
				Vector3 s2 = c.WorldToScreenPoint (pickPos + targetObject.transform.forward);
				Vector3 s3 = c.WorldToScreenPoint (pickPos - targetObject.transform.forward);
				
				float dist = (pickPosOnScreen - Input.mousePosition).magnitude;
				
				if (Vector3.Distance (Input.mousePosition, s2) > Vector3.Distance (Input.mousePosition, s3)) {
					dist = -dist;
				} else {
				}
				
				//				Debug.Log (dist);
				if (Mathf.Abs (dist) > 2) {
					targetObject.transform.position = targetPos + 
						targetObject.transform.forward * dist / Vector3.Distance (s1, s2);
					
					Update (targetObject);
					master.UpdateSelectionBox ();
				}
			}

			if (xRotateHandle == handle.gameObject) {

				Vector3 s1 = c.WorldToScreenPoint (pickPos);
				Vector3 s2 = c.WorldToScreenPoint (pickPos + targetObject.transform.right);
				Vector3 s3 = c.WorldToScreenPoint (pickPos - targetObject.transform.right);
				Vector3 s4 = c.WorldToScreenPoint (master.selectionBox.transform.position);
				
				float dist = (prevPosOnScreen - Input.mousePosition).magnitude;

				Vector3 v = s4 - Input.mousePosition;
				if (Mathf.Abs(v.x) - Mathf.Abs(v.y) < 0 ? v.y < 0 :  v.x < 0 ) {
					dist = -dist;
				} 
				prevPosOnScreen = Input.mousePosition;	

				if (Mathf.Abs (dist) > 2) {
					targetObject.transform.RotateAround(
						master.selectionBox.transform.position, 
						targetObject.transform.right, 
						dist / Vector3.Distance (s1, s2)*30);
					Update (targetObject);
					master.UpdateSelectionBox ();
				}
			}

			if (yRotateHandle == handle.gameObject) {
				
				Vector3 s1 = c.WorldToScreenPoint (pickPos);
				Vector3 s2 = c.WorldToScreenPoint (pickPos + targetObject.transform.up);
				Vector3 s3 = c.WorldToScreenPoint (pickPos - targetObject.transform.up);
				Vector3 s4 = c.WorldToScreenPoint (master.selectionBox.transform.position);
				
				float dist = (prevPosOnScreen - Input.mousePosition).magnitude;
				
				Vector3 v = s4 - Input.mousePosition;
				if (Mathf.Abs(v.x) - Mathf.Abs(v.y) < 0 ? v.y < 0 :  v.x < 0 ) {
					dist = - dist;
				} 
				prevPosOnScreen = Input.mousePosition;	
				
				if (Mathf.Abs (dist) > 2) {
					targetObject.transform.RotateAround(
						master.selectionBox.transform.position, 
						targetObject.transform.up, 
						dist / Vector3.Distance (s1, s2)*30);
					Update (targetObject);
					master.UpdateSelectionBox ();
				}
			}

			if (zRotateHandle == handle.gameObject) {
				
				Vector3 s1 = c.WorldToScreenPoint (pickPos);
				Vector3 s2 = c.WorldToScreenPoint (pickPos + targetObject.transform.forward);
				Vector3 s3 = c.WorldToScreenPoint (pickPos - targetObject.transform.forward);
				Vector3 s4 = c.WorldToScreenPoint (master.selectionBox.transform.position);
				
				float dist = (prevPosOnScreen - Input.mousePosition).magnitude;

				Vector3 v = s4 - Input.mousePosition;
				if (Mathf.Abs(v.x) - Mathf.Abs(v.y) < 0 ? v.y < 0 :  v.x < 0 ) {
					dist = -dist;
				} 
				prevPosOnScreen = Input.mousePosition;	
				
				if (Mathf.Abs (dist) > 2) {
					targetObject.transform.RotateAround(
						master.selectionBox.transform.position, 
						targetObject.transform.forward, 
						dist / Vector3.Distance (s1, s2)*30);
					Update (targetObject);
					master.UpdateSelectionBox ();
				}
			}
		}

		public void ReleaseHandle (Camera c)
		{
			handle = null;
		}



	}
	
}
