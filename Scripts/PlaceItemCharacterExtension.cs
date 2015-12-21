using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace Shiva.ItemPlacing
{
	public class PlaceItemCharacterExtension : PlaceItemExtension
	{
		
		public RectTransform extensionPanel;

		[System.Serializable]
		public class Part
		{
			public string name;
			public GameObject target;
			public bool active = true;
		}


		public int currentCharacterIndex;
		public GameObject currentCharacter;
		public Part[] characters;

		[System.Serializable]
		private class SerializableParts : SerializablePart
		{


			[System.NonSerialized]
			[XmlIgnore]
			private PlaceItemCharacterExtension pice;

			public override object StartEditing (PlaceItemExtension pie)
			{
				pice = pie as PlaceItemCharacterExtension;

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
			
			int idx = 0;
			foreach (Part p in characters) {
				if(p.name == part){
					SetCurrentCharacter (idx);
					break;
				}
				idx++;
			}
		}

		public override string ToString ()
		{
			return characters[currentCharacterIndex].name;
		}

		void SetCurrentCharacter (int num)
		{
			currentCharacterIndex = num;

			Animator animator = null;
			if (currentCharacter) {
				animator = currentCharacter.GetComponent<Animator> ();
				Destroy (currentCharacter);
			}
			currentCharacter = GameObject.Instantiate (characters [num].target);
			currentCharacter.transform.SetParent (transform, false);
			currentCharacter.transform.localPosition = Vector3.zero;
			currentCharacter.transform.localRotation = Quaternion.identity;

			if (animator != null) {
				currentCharacter.GetComponent<Animator> ().runtimeAnimatorController = animator.runtimeAnimatorController;
			}


			gameObject.GetComponent<PlaceItem> ().EndEditing ();
		}

		public override RectTransform GetEditorPanel ()
		{
			RectTransform rect = GameObject.Instantiate<RectTransform> (extensionPanel);
			Toggle buttonPref = rect.GetComponentInChildren<Toggle> ();

			int idx = 0;
			foreach (Part c in characters) {
				Toggle b = Instantiate<Toggle> (buttonPref);

				b.GetComponentInChildren<Text> ().text = c.name;
				b.transform.SetParent (buttonPref.transform.parent);
				int num = idx++;

				if (currentCharacterIndex == num)
					b.isOn = true;
				else
					b.isOn = false;
				
				b.onValueChanged.AddListener ((onOff) => {
					if (onOff) {
						SetCurrentCharacter (num);
					}
				});
			}
			Destroy (buttonPref.gameObject);

			return rect;
		}
			
		void Awale ()
		{
			SetCurrentCharacter (0);
		}
		
		// Update is called once per frame
		void Update ()
		{
		
		}
	}
}
