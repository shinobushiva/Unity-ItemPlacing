using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace Shiva.ItemPlacing
{
	public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
	{
		public AnimationClipOverrides(int capacity) : base(capacity) {}

		public AnimationClip this[string name]
		{
			get { return this.Find(x => x.Key.name.Equals(name)).Value; }
			set
			{
				int index = this.FindIndex(x => x.Key.name.Equals(name));
				if (index != -1)
					this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
			}
		}
	}

	public class PlaceItemAnimationExtension : PlaceItemExtension
	{
		
		public RectTransform extensionPanel;

		[System.Serializable]
		public class AnimationPart
		{
			public string name;
			public AnimationClip clip;
			public bool active = true;
		}

		[System.Serializable]
		public class TargetPart{

			public string name = "下半身";

			public string targetClipName = "lower";

			public int currentClipIndex;
			public AnimationPart[] clips;
		}

		public TargetPart[] targetParts;


		[System.Serializable]
		private class SerializableParts : SerializablePart
		{

			public string data;

			[System.NonSerialized]
			[XmlIgnore]
			private PlaceItemAnimationExtension piae;

			public override object StartEditing (PlaceItemExtension pie)
			{
				piae = pie as PlaceItemAnimationExtension;
				data = piae.ToString ();
				return this;
			}

			public override void EndEditing ()
			{
			}

			public override void Undo (GameObject o)
			{
				piae.FromString (data);
			}

			public override bool HasChanged ()
			{
				return data != piae.ToString();
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
			if (names.Length < 2)
				return;

			for (int i = 0; i < names.Length; i+=2) {
				string tn = names [i];
				string an = names [i + 1];
				foreach (TargetPart tp in targetParts) {
					if (tp.name == tn) {
						int idx = 0;
						foreach (AnimationPart ap in tp.clips) {
							if (ap.name == an) {
								tp.currentClipIndex = idx;
							}
							idx++;
						}
					}
				}
				ReplaceClip ();
			}

		}

		public override string ToString ()
		{
			string str = " ";
			foreach (TargetPart tp in targetParts) {
				AnimationPart ap = tp.clips [tp.currentClipIndex];
				str += tp.name + "," + ap.name+",";
			}

			return str.Substring (0, str.Length - 1).TrimStart ();
		}


//		public AnimatorOverrideController animController;
//		private AnimatorOverrideController m_overrideController;

		protected Animator animator;
		protected AnimatorOverrideController animatorOverrideController;
		protected AnimationClipOverrides clipOverrides;

		public void ReplaceClip ()
		{
			animator = GetComponentInChildren<Animator>();
			animator.applyRootMotion = false;

			animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
			animator.runtimeAnimatorController = animatorOverrideController;

			clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
			animatorOverrideController.GetOverrides(clipOverrides);

			foreach (TargetPart tp in targetParts) {
				clipOverrides [tp.targetClipName] = tp.clips [tp.currentClipIndex].clip;
			}
			animatorOverrideController.ApplyOverrides(clipOverrides);

			animator.gameObject.SetActive (false);
			animator.gameObject.SetActive (true);

		}

		public override RectTransform GetEditorPanel ()
		{
			
			RectTransform rect = GameObject.Instantiate<RectTransform> (extensionPanel);

			ToggleGroup tgPref = rect.gameObject.GetComponentInChildren<ToggleGroup> ();

			foreach (TargetPart tp in targetParts) {

				ToggleGroup tg = GameObject.Instantiate<ToggleGroup> (tgPref);
				tg.transform.SetParent (rect.transform);

				tg.GetComponentInChildren<Text> ().text = tp.name;

				Toggle buttonPref = tg.GetComponentInChildren<Toggle> ();
				

				int idx = 0;
				foreach (AnimationPart c in tp.clips) {
					
					Toggle b = Instantiate<Toggle> (buttonPref);

					b.GetComponentInChildren<Text> ().text = c.name;
					b.transform.SetParent (tg.transform);
					int num = idx++;

					if (tp.currentClipIndex == num)
						b.isOn = true;
					else
						b.isOn = false;


					TargetPart tpp = tp;
					b.onValueChanged.AddListener ((onOff) => {
						if (onOff) {
							print ("Here:"+tpp.clips [num].clip);
							tpp.currentClipIndex = num;

							ReplaceClip ();
						}
					});
				}

				Destroy (buttonPref.gameObject);
			}

			Destroy (tgPref.gameObject);


			return rect;
		}

		void Start ()
		{
			ReplaceClip ();
		}

//		public void Start()
//		{
//			animator = GetComponentInChildren<Animator>();
//
//			animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
//			animator.runtimeAnimatorController = animatorOverrideController;
//
//			clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
//			animatorOverrideController.GetOverrides(clipOverrides);
//			foreach (var co in clipOverrides) {
//				print (co.Key);
//			}
//		}

		bool playStarted = false;
		
		// Update is called once per frame
		void Update ()
		{
			if (!playStarted) {
				ReplaceClip ();
				playStarted = true;
			}
		}
	}
}
