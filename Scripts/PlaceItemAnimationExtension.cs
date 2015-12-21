using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace Shiva.ItemPlacing
{
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

		public AnimatorOverrideController animController;

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

		private AnimatorOverrideController m_overrideController;

		public void ReplaceClip ()
		{

			Animator m_animator = GetComponentInChildren<Animator>();
			m_animator.applyRootMotion = false;

			m_animator.runtimeAnimatorController = animController;
			m_overrideController = new AnimatorOverrideController ();
			m_overrideController.runtimeAnimatorController = m_animator.runtimeAnimatorController;
		

			AnimationClip[] cs = m_animator.runtimeAnimatorController.animationClips;
			foreach (AnimationClip ac in cs) {
				print (ac);
			}

			if (m_overrideController) {
				foreach (TargetPart tp in targetParts) {
					m_overrideController [tp.targetClipName] = tp.clips[tp.currentClipIndex].clip;
				}
				if (!ReferenceEquals (m_animator.runtimeAnimatorController, m_overrideController)) {
					m_animator.runtimeAnimatorController = m_overrideController;
				}
			}

			gameObject.GetComponent<PlaceItem> ().EndEditing ();
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
