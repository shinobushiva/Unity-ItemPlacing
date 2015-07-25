using UnityEngine;
using System.Collections;

namespace Shiva.ItemPlacing {
	public class PlaceItemExtension : MonoBehaviour {

		public virtual RectTransform GetEditorPanel(){
			return null;
		}

		public virtual SerializablePart GetCopy(){
			return null;
		}

		public virtual void FromString(string part){
		}

		public virtual string ToString(){
			return "";
		}

	}
}