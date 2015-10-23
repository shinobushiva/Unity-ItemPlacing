using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Shiva.CameraSwitch;

public class CommentPanel : MonoBehaviour {

	public Transform target;

	private RectTransform rect;
	private Image panelImage;

	public Vector2 offset = Vector3.one * 20f;
	
	public Vector3 lastPosition = Vector3.zero;

	public CanvasScaler scalar;

	public Text text;
	public string commentType;

	private CameraSwitcher switcher;

	public CommentObject commentObject;

	// Use this for initialization
	void Start () {
		
		rect = GetComponent<RectTransform>();
		panelImage = GetComponent<Image> ();

		switcher = FindObjectOfType<CameraSwitcher> ();
	}
	
	// Update is called once per frame
	void Update () {
		Camera c = switcher.CurrentActive.c;

		if (!commentObject.vr.visible || (Vector3.Distance(c.transform.position, target.position) > 50)) {
			panelImage.enabled = false;
			text.enabled = false;
		} else {
			panelImage.enabled = true;
			text.enabled = true;
		}


		Vector2 of = offset;
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(c, target.position+Vector3.up*2.5f);
//		print (screenPoint);
		
		float xMag = scalar.referenceResolution.x/Screen.width;
		float yMag = scalar.referenceResolution.y/Screen.height;

//		float mag = xMag * scalar.matchWidthOrHeight + yMag * (1- scalar.matchWidthOrHeight);
		float mag = 1;
		if (xMag > yMag) {
			mag = xMag;
		} else {
			mag = yMag;
		}
		float ofMag = 1;
//		if (xMag > yMag) {
//			ofMag = scalar.referenceResolution.x;
//		} else {
//			ofMag = scalar.referenceResolution.y;
//		}

		of.x = rect.rect.width/2;

		Vector2 pos2 = screenPoint;
		pos2.x = pos2.x*mag;
		pos2.y = pos2.y*mag;

		if(screenPoint.x > Screen.width/2){
			pos2.x = pos2.x - of.x*ofMag;
		}else{
			pos2.x = pos2.x + of.x*ofMag;
		}
		
		if(screenPoint.y > Screen.height/2){
			pos2.y = pos2.y - of.y*ofMag;
		}else{
			pos2.y = pos2.y + of.y*ofMag;
		}
		
		rect.anchoredPosition = Vector3.Lerp(lastPosition, pos2, Time.deltaTime*8);
		lastPosition = rect.anchoredPosition;

	
	}
}
