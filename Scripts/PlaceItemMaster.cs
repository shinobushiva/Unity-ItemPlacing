using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DataSaveLoad;
using Shiva.CameraSwitch;

namespace Shiva.ItemPlacing {
	public class PlaceItemMaster : MethodAll
	{

		//
		public DataSaveLoadMaster dataSaveLoad;
		public string folder = "ItemPlacing";
		public Text selectedTarget;

		//
		private GameObject createOb;
		public PlaceItems placeItems;
		public GameObject placeItemPrefab;
		public RectTransform scrollContent;
		//
		private GameObject targetObject;
		private List<PlaceItem> placedItems;

		//
		private bool creating = false;
		private bool endCreation = false;
		
		//
		private Vector3 lastMousePosition;
		//
		private Vector3 pressedMousePosition;
		private Vector3 targetPosition;
		private Quaternion targetRotation;
		private Vector3 targetScale;

		[System.Serializable]
		public enum State
		{
			Selecting,
			Moving,
			Rotating,
			Scaling,
			Creating,
			None
		}
		private State editState = State.Moving;

		public State EditState {
			set {
				editState = value;
			}
			get {
				return editState;
			}
		}
		
		private bool selection = true;
		
		public bool Selection {
			get {
				return selection;
			}
			set {
				selection = value;
			}
		}
		
		private bool xEnabled = true;
		
		public bool XEnabled {
			get {
				return xEnabled;
			}
			set {
				xEnabled = value;
				wasPointerDownInUI = false;
			}
		}
		
		private bool yEnabled = true;
		
		public bool YEnabled {
			get {
				return yEnabled;
			}
			set {
				yEnabled = value;
				wasPointerDownInUI = false;
			}
		}
		
		private bool zEnabled = true;
		
		public bool ZEnabled {
			get {
				return zEnabled;
			}
			set {
				zEnabled = value;
				wasPointerDownInUI = false;
			}
		}
		
		public void SetModeSelection ()
		{
			EditState = State.Selecting;
		}
		
		public void SetModeMove ()
		{
			EditState = State.Moving;
			wasPointerDownInUI = false;
		}
		
		public void SetModeRotate ()
		{
			EditState = State.Rotating;
			wasPointerDownInUI = false;
		}
		
		public void SetModeScale ()
		{
			EditState = State.Scaling;
			wasPointerDownInUI = false;
		}

		private CameraSwitcher cameraSwitcher;
		void Start(){
			cameraSwitcher = FindObjectOfType<CameraSwitcher> ();
	
			foreach (PlaceItem pi in placeItems.placeItems) {

				GameObject pip = GameObject.Instantiate (placeItemPrefab);
				pip.transform.SetParent (scrollContent.transform, false);
				PlaceItemEntry pie = pip.GetComponent<PlaceItemEntry> ();
				pie.PlaceItem = pi;
				pie.itemButton.onClick.AddListener (() => {
					if (Network.isServer || Network.isClient)
						targetObject = (GameObject)Network.Instantiate (pie.PlaceItem.gameObject, Vector3.zero, 
						                                                pie.PlaceItem.transform.rotation, 1);
					else
						targetObject = (GameObject)GameObject.Instantiate (pie.PlaceItem.gameObject, Vector3.zero, 
						                                                   pie.PlaceItem.transform.rotation);

					placedItems.Add (targetObject.GetComponent<PlaceItem> ());
	                        
					SetCollider (targetObject, false);
					creating = true;
					endCreation = false;

				});
			}
			SetTarget (null);

			placedItems = new List<PlaceItem> ();
			PlaceItem[] pis = FindObjectsOfType<PlaceItem> ();
			placedItems.AddRange (pis);

			dataSaveLoad.dataLoadHandler += DataLoaded;
		}

		private void SetTarget (GameObject t)
		{
			targetObject = t;
			selectedTarget.text = "<None Selected>";
			selectedTarget.color = Color.gray;
			if (targetObject != null) {
				SetCollider (targetObject, false);
				targetPosition = t.transform.position;
				targetRotation = t.transform.rotation;
				targetScale = t.transform.localScale;
				targetObject.GetComponent<PlaceItem> ().StartEditing ();

				selectedTarget.text = targetObject.name;
				selectedTarget.color = Color.white;
			}

		}

	//	private bool WasJustADamnedButton()
	//	{
	//		UnityEngine.EventSystems.EventSystem ct
	//			= UnityEngine.EventSystems.EventSystem.current;
	//		
	//		if (! ct.IsPointerOverGameObject() ) return false;
	//		if (! ct.currentSelectedGameObject ) return false;
	//
	//		return true;
	//	}

		public PlaceItem PickItem ()
		{
			bool b;
			Ray ray = cameraSwitcher.currentActive.c.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			b = Physics.Raycast (ray, out hit);
			if (b) {
				GameObject go = hit.transform.gameObject;
				PlaceItem p = go.GetComponent<PlaceItem> ();
				if (p)
					return p;
				
				return go.GetComponentInParent<PlaceItem> ();
			}
			return null;

		}

		private bool wasPointerDownInUI;

		// Update is called once per frame
		void Update ()
		{

			if (creating) {
				if (!targetObject)
					return;
				
				bool b;
				RaycastHit hit = RayHit (out b, cameraSwitcher.currentActive.c);
				if (b) {
					targetObject.transform.position = hit.point;
				}
				if (Input.GetMouseButtonDown (0)) {
					endCreation = true;
				}
				
				if (endCreation && Input.GetMouseButtonUp (0)) {
					SetCollider (targetObject, true);
					creating = false;
				}
				
				return;
			}

			if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject ()) {
				if (Input.GetMouseButtonDown (0)) {
					wasPointerDownInUI = true;
				}
				return;
			}
			if (wasPointerDownInUI && Input.GetMouseButtonUp (0)) {
				wasPointerDownInUI = false;
				return;
			}
			if (wasPointerDownInUI)
				return;


			if (Selection) {
				
				if (Input.GetMouseButtonDown (0)) {
					
					if (targetObject) {
						SetCollider (targetObject, true);
						SetTarget (null);
					}

					PlaceItem pi = PickItem ();
					if (pi) {
						SetTarget (pi.gameObject);
						pressedMousePosition = Input.mousePosition;
					}
				}
			}

			if (EditState == State.Moving) {
				if (!targetObject)
					return;
				
				if (Input.GetMouseButton (0)) {

					if (!xEnabled || !yEnabled || !zEnabled) {

						Vector3 dd = Input.mousePosition - pressedMousePosition;
						float dir = 0;
						if (Mathf.Abs (dd.x) < Mathf.Abs (dd.y)) {
							dir = dd.y;
						} else {
							dir = dd.x;
						}

						float d = (Input.mousePosition - pressedMousePosition).magnitude * (dir < 0 ? -1 : 1);

						Vector3 pos = targetPosition + Vector3.one * d;
						if (!xEnabled)
							pos.x = targetPosition.x;
						if (!yEnabled)
							pos.y = targetPosition.y;
						if (!zEnabled)
							pos.z = targetPosition.z;
						
						targetObject.transform.position = pos;
					} else {
						bool b;
						RaycastHit hit = RayHit (out b, cameraSwitcher.currentActive.c);
						if (b) {
							Vector3 pos = hit.point;
							targetObject.transform.position = pos;
						}
					}

				}

				if (Input.GetMouseButtonUp (0)) {
					SetCollider (targetObject, true);
					targetObject.GetComponent<PlaceItem> ().EndEditing ();
				}
			}

			if (EditState == State.Rotating) {
				if (!targetObject)
					return;
				
				if (Input.GetMouseButton (0)) {
					Vector3 dd = Input.mousePosition - pressedMousePosition;
					float dir = 0;
					if (Mathf.Abs (dd.x) < Mathf.Abs (dd.y)) {
						dir = dd.y;
					} else {
						dir = dd.x;
					}
					
					float d = (Input.mousePosition - pressedMousePosition).magnitude * (dir < 0 ? -1 : 1);
					
					Vector3 axis = Vector3.one;
					if (!xEnabled)
						axis.x = 0;
					if (!yEnabled)
						axis.y = 0;
					if (!zEnabled)
						axis.z = 0;
					targetObject.transform.rotation = targetRotation;
					targetObject.transform.Rotate (axis * d, Space.World);

				}
				
				if (Input.GetMouseButtonUp (0)) {
					SetCollider (targetObject, true);
					targetObject.GetComponent<PlaceItem> ().EndEditing ();
				}
			}

			if (EditState == State.Scaling) {
				if (!targetObject)
					return;
				
				if (Input.GetMouseButton (0)) {
					Vector3 dd = Input.mousePosition - pressedMousePosition;
					float dir = 0;
					if (Mathf.Abs (dd.x) < Mathf.Abs (dd.y)) {
						dir = dd.y;
					} else {
						dir = dd.x;
					}
					
					float d = (Input.mousePosition - pressedMousePosition).magnitude / 10f;
					if (dir < 0) {
						d = 1f / d;
					}
					
					Vector3 axis = targetScale * d;
					if (!xEnabled)
						axis.x = targetScale.x;
					if (!yEnabled)
						axis.y = targetScale.y;
					if (!zEnabled)
						axis.z = targetScale.z;

					if (d > 0f) {
						targetObject.transform.localScale = axis;
					}
				}
				
				if (Input.GetMouseButtonUp (0)) {
					SetCollider (targetObject, true);
					targetObject.GetComponent<PlaceItem> ().EndEditing ();
				}
			}
		}

		public void EditUndo ()
		{
			wasPointerDownInUI = false;

			if (targetObject != null) {
				targetObject.GetComponent<PlaceItem> ().EditUndo ();
			}
		}

		public void Delete ()
		{
			wasPointerDownInUI = false;

			if (targetObject != null) {
				placedItems.Remove (targetObject.GetComponent<PlaceItem> ());
				DestroyImmediate (targetObject);

				SetTarget (null);
			}
		}

		public void ShowSaveDialog ()
		{
			wasPointerDownInUI = false;

			List<PlaceItemData> data = new List<PlaceItemData> ();

			foreach (PlaceItem pi in placedItems) {
				PlaceItemData pid = new PlaceItemData ();
				pid.itemName = pi.itemName;
				pid.loc = pi.transform.position;
				pid.rot = pi.transform.rotation;
				pid.scale = pi.transform.localScale;

				data.Add (pid);
			}

			dataSaveLoad.ShowSaveDialog (data, folder);
		}

		public void ShowLoadDialog ()
		{
			wasPointerDownInUI = false;

			dataSaveLoad.ShowLoadDialog (typeof(List<PlaceItemData>), folder);
		}

		[System.Serializable]
		public class PlaceItemData
		{
			public Vector3 loc;
			public Quaternion rot;
			public Vector3 scale;
			public string itemName;
		}

		public void DataLoaded (object data)
		{

			foreach (PlaceItem pi in placedItems) {
				GameObject.Destroy (pi.gameObject);
			}
			placedItems.Clear ();
				
			List<PlaceItemData> pids = data as List<PlaceItemData>;
			if (pids == null)
				return;

			foreach (PlaceItemData pid in pids) {
				PlaceItem pi = placeItems.GetItemByName (pid.itemName.Trim ());
					
				if (Network.isServer || Network.isClient)
					targetObject = (GameObject)Network.Instantiate (pi.gameObject, Vector3.zero, 
						                                                pi.transform.rotation, 1);
				else
					targetObject = (GameObject)GameObject.Instantiate (pi.gameObject, Vector3.zero, 
						                                                   pi.transform.rotation);
				targetObject.transform.position = pid.loc;
				targetObject.transform.rotation = pid.rot;
				targetObject.transform.localScale = pid.scale;
					
				placedItems.Add (targetObject.GetComponent<PlaceItem> ());
			}
		}

	}
}

