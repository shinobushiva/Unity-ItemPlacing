using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DataSaveLoad;
using Shiva.CameraSwitch;
using System.IO;
using System.Xml.Serialization;

namespace Shiva.Commenting
{
	public class CommentingMaster : MethodAll
	{

		public delegate void OnCommentAdded(CommentObject cd);
		public event OnCommentAdded onCommentAdded;

		public delegate void OnCommentDeleted(CommentObject cd);
		public event OnCommentAdded onCommentDeleted;


		public GameObject commentIndicatorPrefab;
		public CommentPanel commentPanelPrefab;

		public Canvas canvas;
		private List<CommentObject> comments;

		public RectTransform commentsPanel;
		public GameObject commentIndicators;
		public InputField commentInput;



		public CommentObject AddComment (CommentData cd)
		{
			GameObject go = Instantiate<GameObject> (commentIndicatorPrefab);
			go.transform.SetParent (commentIndicators.transform, true);

			CommentObject co = go.GetComponent<CommentObject> ();
			comments.Add (co);
			
			CommentPanel cp = Instantiate<CommentPanel> (commentPanelPrefab);
			cp.transform.SetParent (commentsPanel.transform, true);
			cp.target = go.transform;
			cp.scalar = canvas.GetComponent<CanvasScaler> ();
			cp.transform.localScale = commentPanelPrefab.transform.localScale;
			
			co.commentPanel = cp;
			cp.commentObject = co;

			co.commentPanel.text.text = cd.comment;
			co.objectId = cd.objectId;
			co.transform.position = cd.loc;

			return co;

		}

		public void AddComment ()
		{
			GameObject go = Instantiate<GameObject> (commentIndicatorPrefab);
			go.transform.SetParent (commentIndicators.transform, true);
			targetObject = go;

			CommentObject co = targetObject.GetComponent<CommentObject> ();

			comments.Add (co);
			SetCollider (targetObject, false);
			creating = true;
			endCreation = false;

			CommentPanel cp = Instantiate<CommentPanel> (commentPanelPrefab);
			cp.transform.SetParent (commentsPanel.transform, true);
			cp.target = go.transform;
			cp.scalar = canvas.GetComponent<CanvasScaler> ();
			cp.transform.localScale = commentPanelPrefab.transform.localScale;

			co.commentPanel = cp;
			cp.commentObject = co;

			SetTarget (targetObject);
			SetUIVisible (false);
		}

		public void CommentEditEnd(){

			targetObject.GetComponent<CommentObject> ().commentPanel.text.text = commentInput.text;
			if(onCommentAdded != null)
				onCommentAdded(targetObject.GetComponent<CommentObject> ());

		}

		public void SetUIVisible (bool b)
		{
			canvas.enabled = b;
		}

		public void ToggleComments(bool b){
			commentsPanel.gameObject.SetActive (b);
			commentIndicators.SetActive (b);
		}


		//
		public DataSaveLoadMaster dataSaveLoad;
		public string folder = "Commenting";
		private string latestAutoSavefile = "Commenting_AutoSaved";
		public Text selectedTarget;

		//
		private GameObject targetObject;

		//
		private bool creating = false;
		private bool endCreation = false;
		
		//
		private Vector3 lastMousePosition;
		//
		private Vector3 pressedMousePosition;
		private Vector3 targetPosition;

		//
//		public RectTransform extensionPanel;

		[System.Serializable]
		public enum State
		{
			Selecting,
			Moving,
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

		private CameraSwitcher cameraSwitcher;

		void Start ()
		{
			cameraSwitcher = FindObjectOfType<CameraSwitcher> ();
	
			comments = new List<CommentObject> ();
			SetTarget (null);

			dataSaveLoad.AddHandler (DataLoaded, typeof(List<CommentData>));

			FileInfo fi = new FileInfo (dataSaveLoad.GetFilePath (latestAutoSavefile, folder));
			print (dataSaveLoad.GetFilePath (folder, latestAutoSavefile));
			if (fi.Exists) {

				dataSaveLoad.Load (fi, typeof(List<CommentData>));
				SetTarget (null);
			}
		}

		void OnApplicationQuit ()
		{
			List<CommentData> data = CreateDataForSerialization ();
			dataSaveLoad.Save (latestAutoSavefile, folder, data);
		}

		private void SetTarget (GameObject t)
		{
			targetObject = t;
			selectedTarget.text = "" + comments.Count + " comments";
			selectedTarget.color = Color.white;

			if (targetObject != null) {
				targetPosition = t.transform.position;

				selectedTarget.text = targetObject.name;
				selectedTarget.color = Color.green;

				CommentObject co = targetObject.GetComponent<CommentObject> ();
				commentInput.text = co.commentPanel.text.text;

				commentInput.interactable = true;
			} else {
				commentInput.interactable = false;
			}

		}


		public CommentObject PickItem ()
		{
			bool b;
			Ray ray = cameraSwitcher.CurrentActive.c.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			b = Physics.Raycast (ray, out hit);
			if (b) {
				GameObject go = hit.transform.gameObject;
				CommentObject p = go.GetComponent<CommentObject> ();
				if (p)
					return p;
				
				return go.GetComponentInParent<CommentObject> ();
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
				RaycastHit hit = RayHit (out b, cameraSwitcher.CurrentActive.c);
				if (b) {
					targetObject.transform.position = hit.point;
				}
				if (Input.GetMouseButtonDown (0)) {
					endCreation = true;
				}
				
				if (endCreation && Input.GetMouseButtonUp (0)) {

					SetCollider (targetObject, true);
					creating = false;

					SetTarget (targetObject);
					wasPointerDownInUI = false;
					SetUIVisible (true);

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

					CommentObject pi = PickItem ();
					if (pi) {
						SetTarget (pi.gameObject);
						pressedMousePosition = Input.mousePosition;
//						targetObject.GetComponent<PlaceItem> ().StartEditing ();

					}
				}
			}

			if (EditState == State.Moving) {
				if (!targetObject)
					return;
				
				if (Input.GetMouseButton (0)) {

					Vector3 dd = Input.mousePosition - pressedMousePosition;
					if (dd.magnitude < 2)
						return;

					SetUIVisible (false);
					bool b;
					RaycastHit hit = RayHit (out b, cameraSwitcher.CurrentActive.c);
					if (b) {
						Vector3 pos = hit.point;
						targetObject.transform.position = pos;
					}
				}

				if (Input.GetMouseButtonUp (0)) {

					SetCollider (targetObject, true);
					SetUIVisible (true);
				}
			}

		}


		public void Delete ()
		{
			wasPointerDownInUI = false;

			if (targetObject != null) {
				comments.Remove (targetObject.GetComponent<CommentObject> ());
				if(onCommentDeleted != null)
					onCommentDeleted(targetObject.GetComponent<CommentObject> ());

				DestroyImmediate(targetObject.GetComponent<CommentObject> ().commentPanel.gameObject);
				DestroyImmediate (targetObject);

				SetTarget (null);
			}
		}

		public void DeleteAll ()
		{
			foreach (CommentObject pi in comments) {
				Destroy(pi.GetComponent<CommentObject> ().commentPanel.gameObject);
				Destroy (pi.gameObject);
			}
			comments.Clear ();
			
			SetTarget (null);
		}

		public static CommentData ToCommentData (CommentObject pi)
		{
			CommentData pid = new CommentData ();
			pid.comment = pi.commentPanel.text.text;
			pid.loc = pi.transform.position;
			return pid;
		}

		private List<CommentData> CreateDataForSerialization ()
		{
			List<CommentData> data = new List<CommentData> ();

			foreach (CommentObject pi in comments) {
				CommentData pid = ToCommentData (pi);
				
				data.Add (pid);
			}

			return data;
		}

		public void ShowSaveDialog ()
		{
			wasPointerDownInUI = false;

			List<CommentData> data = CreateDataForSerialization ();
			dataSaveLoad.ShowSaveDialog (data, folder);
		}

		public void ShowLoadDialog ()
		{
			wasPointerDownInUI = false;

			dataSaveLoad.ShowLoadDialog (typeof(List<CommentData>), folder);
		}

		[System.Serializable]
		public class CommentData
		{
			public Vector3 loc;
			public string comment;
			public string objectId;
		}

		public void DataLoaded (object data)
		{

			DeleteAll ();
				
			List<CommentData> pids = data as List<CommentData>;
			if (pids == null)
				return;

			foreach (CommentData pid in pids) {
					
//				placedItems.Add (targetObject.GetComponent<PlaceItem> ());
			}
		}

	}
}

