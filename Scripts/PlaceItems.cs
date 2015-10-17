using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Shiva.ItemPlacing {

	public class PlaceItems: MonoBehaviour
	{
		public PlaceItem[] placeItems;
		
		private Dictionary<string, PlaceItem> map;

		void Awake(){
			map = new Dictionary<string, PlaceItem> ();

			foreach (PlaceItem pi in placeItems) {
				string n = pi.itemName;
				if(n == null || n.Length <= 0){
					n = pi.gameObject.name;
				}
				n = n.Trim();

				pi.itemName = n;
				map.Add(n, pi);
			}
		}

		public PlaceItem GetItemByName(string n){
			if (!map.ContainsKey (n)) {
				print (n);
				return null;
			}

			return map [n];
		}




	}

}
