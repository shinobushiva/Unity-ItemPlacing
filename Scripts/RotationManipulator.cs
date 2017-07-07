using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shiva.ItemPlacing
{
	public class RotationManipulator : MonoBehaviour {

		private PlaceItemMaster master;

		public Slider sliderX;
		public Slider sliderY;
		public Slider sliderZ; 

		public Toggle visibilityToggle;

		// Use this for initialization
		void Start () {
			
			master = FindObjectOfType<PlaceItemMaster> ();

			master.placeItemEvent += (e, item) => {
				print(e);
				if(e == PlaceItemEvent.PlaceItemTransformed 
					|| e == PlaceItemEvent.ItemSelected){
					
					Vector3 v = item.transform.rotation.eulerAngles;
					sliderX.value = v.x;
					sliderY.value = v.y;
					sliderZ.value = v.z;
						
				}
			};

			if(visibilityToggle)
				gameObject.SetActive (visibilityToggle.isOn);
		}
		
		// Update is called once per frame
		public void ValueChanged () {
			master.TransfromTarget (null, new Vector3 (sliderX.value, sliderY.value, sliderZ.value), null);
		}
	}
}
