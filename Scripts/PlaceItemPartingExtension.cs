using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace Shiva.ItemPlacing {
	public class PlaceItemPartingExtension : PlaceItemExtension {

		public RectTransform extensionPanel;

		[System.Serializable]
		public class Part{
			public string name;
			public GameObject target;
			public bool active = true;
		}

		public Part[] parts;

		[System.Serializable]
		private class SerializableParts : SerializablePart{

			public bool[] flags;

			[System.NonSerialized]
			[XmlIgnore]
			private PlaceItemPartingExtension pice;

			public override object StartEditing (PlaceItemExtension pie)
			{
				pice = pie as PlaceItemPartingExtension;

				return this;

			}

			public override void EndEditing ()
			{
			}

			public override void Undo (GameObject o)
			{

			}

			public override bool HasChanged ()
			{
				return false;
			}

		}

		public override SerializablePart GetCopy ()
		{
			SerializableParts sc = new SerializableParts ();
			return sc;
		}

		public override void FromString (string part)
		{
			string[] names = part.Split (',');
			
			foreach (Part p in parts) {
				if(names.Contains(p.name)){
					p.active = true;
					p.target.SetActive(true);
				}else{
					p.active = false;
					p.target.SetActive(false);
				}
			}
		}

		public override string ToString ()
		{
			string str = " ";
			foreach (Part p in parts) {
				if(p.active)
					str+=p.name+",";
			}

			return str.Substring(0, str.Length-1).TrimStart();
		}


		public override RectTransform GetEditorPanel ()
		{
			RectTransform rect = GameObject.Instantiate<RectTransform> (extensionPanel);
			Toggle buttonPref = rect.GetComponentInChildren<Toggle> ();
			List<Toggle> buttons = new List<Toggle> ();

			int idx = 0;
			foreach (Part c in parts) {
				Toggle b = Instantiate<Toggle>(buttonPref);
				b.isOn = c.active;

				b.GetComponentInChildren<Text>().text = c.name;
				b.transform.SetParent(buttonPref.transform.parent);
				int num = idx++;
				b.onValueChanged.AddListener((onOff) => {
					parts[num].target.SetActive(onOff);
					parts[num].active = onOff;
					gameObject.GetComponent<PlaceItem> ().EndEditing ();
				});
			}
			Destroy (buttonPref.gameObject);

			return rect;
		}

		// Use this for initialization
		void Awake () {
			
			foreach (Part c in parts) {
				c.target.SetActive(c.active);
			}
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		// Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
//		static string ColorToHex(Color32 color)
//		{
//			string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
//			return hex;
//		}
//		
//		static Color HexToColor(string hex)
//		{
//			byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
//			byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
//			byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
//			return new Color32(r,g,b, 255);
//		}
	}
}
