using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System;

namespace Shiva.ItemPlacing {
	
	[System.Serializable]
	public class SerializableColor : SerializablePart { //, IXmlSerializable{
		
		public int number;
		
		[System.NonSerialized]
		private PlaceItemColorExtension pice;
		
		public override object StartEditing (PlaceItemExtension pie)
		{
			pice = pie as PlaceItemColorExtension;
			number = pice.chosenColor;
			
			return this;
		}
		
		public override void EndEditing ()
		{
		}
		
		public override void Undo (GameObject o)
		{
			pice.chosenColor = number;
			pice.targetMaterial.color = pice.colors [number];
		}
		
		public override bool HasChanged ()
		{
			return number != pice.chosenColor;
		}

		
	}

	public class PlaceItemColorExtension : PlaceItemExtension {

		public RectTransform extensionPanel;

		public Color[] colors;

		public Material targetMaterial;

		public int chosenColor;

		public override SerializablePart GetCopy ()
		{
			SerializableColor sc = new SerializableColor ();
			sc.number = chosenColor;
			return sc;
		}

		public override void FromString (string part)
		{
			chosenColor = int.Parse (part);
			targetMaterial.color = colors [chosenColor];
		}

		public override string ToString ()
		{
			return ""+chosenColor;
		}

		public override RectTransform GetEditorPanel ()
		{
			RectTransform rect = GameObject.Instantiate<RectTransform> (extensionPanel);
			Button buttonPref = rect.GetComponentInChildren<Button> ();
			List<Button> buttons = new List<Button> ();

			int idx = 0;
			foreach (Color c in colors) {
				Button b = Instantiate<Button>(buttonPref);
				b.image.color = c;
				if(Mathf.Sqrt(c.r*c.r+c.b*c.b+c.g*c.g) > 0.5f)
					b.GetComponentInChildren<Text>().color = Color.black;
				else
					b.GetComponentInChildren<Text>().color = Color.white;

				b.GetComponentInChildren<Text>().text = "#"+ColorToHex(b.image.color);
				b.transform.SetParent(buttonPref.transform.parent);
				int num = idx++;
				b.onClick.AddListener(() => {
					chosenColor = num;
					print (targetMaterial.name+":"+targetMaterial.GetHashCode());
					targetMaterial.color = b.image.color;
					gameObject.GetComponent<PlaceItem> ().EndEditing ();
				});
			}
			Destroy (buttonPref.gameObject);

			return rect;
		}

		// Use this for initialization
		void Awake () {

			print (targetMaterial.name);
			Material mat = null;
			{
				Renderer[] rs = GetComponentsInChildren<Renderer> ();
				foreach (Renderer r in rs) {
					Material[] mats = r.sharedMaterials;
					for (int i=0; i<mats.Length; i++) {
						Material m = mats [i];
						if (m.name.StartsWith (targetMaterial.name) && m.name.Contains ("_Copied")) {
							targetMaterial = m;
							mat = targetMaterial;
							break;
						}
					}
				}
			}
			
			if (mat == null) {
				mat = new Material (targetMaterial);
				mat.name = mat.name + "_Copied";
			}
			{
				Renderer[] rs = GetComponentsInChildren<Renderer> ();
				foreach (Renderer r in rs) {
					Material[] mats = r.materials;
					for (int i=0; i<mats.Length; i++) {
						Material m = mats [i];
						if (m.name.StartsWith (targetMaterial.name)) {
							mats [i] = mat;
						}
					}
					r.materials = mats;
				}
				targetMaterial = mat;
			}

			chosenColor = 0;
			targetMaterial.color = colors [chosenColor];
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		// Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
		static string ColorToHex(Color32 color)
		{
			string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			return hex;
		}
		
		static Color HexToColor(string hex)
		{
			byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
			return new Color32(r,g,b, 255);
		}
	}
}
