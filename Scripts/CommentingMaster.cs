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
		private List<CommentObject> comments = new List<CommentObject>();
		public List<CommentObject> Comments{
			get{
				return comments;
			}
		}

		public RectTransform commentsPanel;
		public GameObject commentIndicators;
		public InputField commentInput;
		public Dropdown commentType;

		public GameObject commentInputPanel;


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

		public CommentObject AddComment (CommentData cd){
			return AddComment (cd.objectId, cd.commentType, cd.comment, cd.loc);
		}

		private CommentObject AddComment (string objectId, string commentType, string comment, Vector3 loc)
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
			
			co.commentPanel.text.text = comment;
			co.commentPanel.commentType = commentType;
			co.objectId = objectId;
			co.transform.position = loc;
			
			return co;
		}

		public void CommentEditEnd(){

			targetObject.GetComponent<CommentObject> ().commentPanel.text.text = commentInput.text;
			targetObject.GetComponent<CommentObject> ().commentPanel.commentType = commentType.options [commentType.value].text;
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

		}

		public void SetPinVisible(bool b){
			commentIndicators.SetActive (b);
		}


		public void SetTarget (GameObject t)
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

				int idx = 0;
				for(int i=0;i<commentType.options.Count; i++){
					if( commentType.options[i].text ==  co.commentPanel.commentType){
						idx = i;
					}
				}
				commentType.value = idx;

				commentInputPanel.SetActive(true);
				commentInput.interactable = true;
			} else {
				commentInput.interactable = false;
				commentInputPanel.SetActive(false);
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
					
					targetObject.SetLayerRecursively(LayerMask.NameToLayer("Ignore Raycast"));

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

					targetObject.SetLayerRecursively(LayerMask.NameToLayer("Default"));

					CommentEditEnd();
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

		
		[System.Serializable]
		public class CommentData
		{
			public Vector3 loc;
			public string commentType;
			public string comment;
			public string objectId;
		}

		public static CommentData ToCommentData (CommentObject pi)
		{
			CommentData pid = new CommentData ();
			pid.comment = pi.commentPanel.text.text;
			pid.loc = pi.transform.position;
			pid.commentType = pi.commentPanel.commentType;

			return pid;
		}
		
		public static List<CommentData> CreateDataForSerialization (List<CommentObject> comments)
		{
			List<CommentData> data = new List<CommentData> ();
			
			foreach (CommentObject pi in comments) {
				CommentData pid = ToCommentData (pi);
				
				data.Add (pid);
			}
			
			return data;
		}


	}
}

