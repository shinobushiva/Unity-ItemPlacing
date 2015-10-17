using UnityEngine;
using System.Collections;
using Shiva.Commenting;

public class CommentDelegateSample : MonoBehaviour {

	public CommentingMaster master;

	// Use this for initialization
	void Start () {
		master.onCommentAdded += OnCommentAdded;
		master.onCommentDeleted += OnCommentDeleted;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnCommentAdded (CommentObject co){
		
		CommentingMaster.CommentData data = CommentingMaster.ToCommentData (co);

		print ("Added");
		print (data.comment);
	}

	public void OnCommentDeleted (CommentObject co){
		
		CommentingMaster.CommentData data = CommentingMaster.ToCommentData (co);
		print ("Delete");
		print (data.comment);
	}
}
