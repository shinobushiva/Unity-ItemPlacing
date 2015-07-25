using UnityEngine;
using System.Collections;

namespace Shiva.ItemPlacing {
	public abstract class SerializablePart{

		public abstract void Undo (GameObject o);
		public abstract object StartEditing (PlaceItemExtension pie);
		public abstract void EndEditing ();
		public abstract bool HasChanged ();

	 	public static Vector3 Copy(Vector3 v){
			Vector3 v2 = new Vector3 ();
			v2.x = v.x;
			v2.y = v.y;
			v2.z = v.z;
			return v2;
		}
		
		public static Quaternion Copy(Quaternion v){
			Quaternion v2 = new Quaternion ();
			v2.x = v.x;
			v2.y = v.y;
			v2.z = v.z;
			v2.w = v.w;
			return v2;
		}

	}
}
