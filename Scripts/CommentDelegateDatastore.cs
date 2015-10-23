using UnityEngine;
using System.Collections;
using Shiva.Commenting;
using DataSaveLoad;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class CommentDelegateDatastore : MonoBehaviour {

	public CommentingMaster master;

	//
	public DataSaveLoadMaster dataSaveLoad;
	public string folder = "Commenting";
	private string latestAutoSavefile = "Commenting_AutoSaved";

	// Use this for initialization
	void Start () {
		master.onCommentAdded += OnCommentAdded;
		master.onCommentDeleted += OnCommentDeleted;

		dataSaveLoad.AddHandler (DataLoaded, typeof(List<CommentingMaster.CommentData>));
		
		FileInfo fi = new FileInfo (dataSaveLoad.GetFilePath (latestAutoSavefile, folder));
		print (dataSaveLoad.GetFilePath (folder, latestAutoSavefile));
		if (fi.Exists) {
			
			dataSaveLoad.Load (fi, typeof(List<CommentingMaster.CommentData>));
			master.SetTarget (null);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public CommentObject AddComment (CommentingMaster.CommentData cd)
	{
		return master.AddComment (cd);
	}
	
	
	public void DataLoaded (object data)
	{
		
		master.DeleteAll ();
		
		List<CommentingMaster.CommentData> pids = data as List<CommentingMaster.CommentData>;
		if (pids == null)
			return;
		
		foreach (CommentingMaster.CommentData pid in pids) {
			
			//				placedItems.Add (targetObject.GetComponent<PlaceItem> ());
		}
	}
	
	void OnApplicationQuit ()
	{
		List<CommentingMaster.CommentData> data = CommentingMaster.CreateDataForSerialization (master.Comments);
		dataSaveLoad.Save (latestAutoSavefile, folder, data);
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

	
	public void ShowSaveDialog ()
	{
//		wasPointerDownInUI = false;
		
		List<CommentingMaster.CommentData> data = CommentingMaster.CreateDataForSerialization (master.Comments);
		dataSaveLoad.ShowSaveDialog (data, folder);
	}
	
	public void ShowLoadDialog ()
	{
//		wasPointerDownInUI = false;
		
		dataSaveLoad.ShowLoadDialog (typeof(List<CommentingMaster.CommentData>), folder);
	}
}
