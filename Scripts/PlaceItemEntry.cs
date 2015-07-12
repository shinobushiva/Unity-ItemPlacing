using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Shiva.ItemPlacing {
	public class PlaceItemEntry : MonoBehaviour {

		public Text itemNameText;
		public Image itemImage;
		public Button itemButton;
		
		private string layerName;
//		private ObjectLayerMaster master;

		private PlaceItem placeItem;

		public PlaceItem PlaceItem{
			get{
				return placeItem;
			}
			set{
				placeItem = value;
//				print (itemImage);
//				print (placeItem);
				itemImage.sprite = placeItem.sprite;
				itemNameText.text = placeItem.name;
			}
		}

 
		// Use this for initialization
		void Start () {
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public void Dispose(){

		}

		public void Load(){

		}

//		public void Set(string layerName, ObjectLayerMaster master){
//			layerText.text = layerName;
//			this.layerName = layerName;
//			this.master = master;
//		}

//		public void SetVisible(Toggle t){
//			master.SetLayerVisible (t.isOn, layerName);
//		}
	}
}