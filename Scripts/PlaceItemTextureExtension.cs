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
	public class SerializableTexture : SerializablePart { 
		
		public int number;
		
		[System.NonSerialized]
		private PlaceItemTextureExtension pice;
		
		public override object StartEditing (PlaceItemExtension pie)
		{
			pice = pie as PlaceItemTextureExtension;
			number = pice.chosenTexture;
			
			return this;
		}
		
		public override void EndEditing ()
		{
		}
		
		public override void Undo (GameObject o)
		{
			pice.chosenTexture = number;
			pice.targetMaterial.mainTexture = pice.textures [number];
		}
		
		public override bool HasChanged ()
		{
			return number != pice.chosenTexture;
		}

		
	}

	public class PlaceItemTextureExtension : PlaceItemExtension {

		public RectTransform extensionPanel;

		public Texture[] textures;

		public Material targetMaterial;

		public int chosenTexture;

		public override SerializablePart GetCopy ()
		{
			SerializableTexture sc = new SerializableTexture ();
			sc.number = chosenTexture;
			return sc;
		}

		public override void FromString (string part)
		{
			chosenTexture = int.Parse (part);
			targetMaterial.mainTexture = textures [chosenTexture];
		}

		public override string ToString ()
		{
			return ""+chosenTexture;
		}

		public override RectTransform GetEditorPanel ()
		{
			RectTransform rect = GameObject.Instantiate<RectTransform> (extensionPanel);
			Button buttonPref = rect.GetComponentInChildren<Button> ();
			List<Button> buttons = new List<Button> ();

			int idx = 0;
			foreach (Texture c in textures) {
				Button b = Instantiate<Button>(buttonPref);
				b.GetComponentInChildren<RawImage>().texture = c;
				b.GetComponentInChildren<Text>().color = Color.black;

				b.GetComponentInChildren<Text>().text = "#"+b.GetComponentInChildren<RawImage>().texture.name;
				b.transform.SetParent(buttonPref.transform.parent);
				int num = idx++;
				b.onClick.AddListener(() => {
					chosenTexture = num;
					targetMaterial.mainTexture = b.GetComponentInChildren<RawImage>().texture;
					gameObject.GetComponent<PlaceItem> ().EndEditing ();
				});
			}
			Destroy (buttonPref.gameObject);

			return rect;
		}

		// Use this for initialization
		void Awake () {

			Material mat = null;
			{
				Renderer[] rs = GetComponentsInChildren<Renderer> ();
				foreach (Renderer r in rs) {
					Material[] mats = r.sharedMaterials;
					for (int i=0; i<mats.Length; i++) {
						Material m = mats [i];
						print (m);
						if (m.name.StartsWith (targetMaterial.name) && m.name.Contains ("_Copied")) {
							targetMaterial = m;
							mat = targetMaterial;
							break;
						}
					}
				}
			}
			if (mat == null){
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

			chosenTexture = 0;
			if(chosenTexture > textures.Length)
				targetMaterial.mainTexture = textures [chosenTexture];
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}
