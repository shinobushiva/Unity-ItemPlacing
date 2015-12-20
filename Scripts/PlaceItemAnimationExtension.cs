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
			
//			foreach (Part p in characters) {
//				if(names.Contains(p.name)){
//					p.active = true;
//					p.target.SetActive(true);
//				}else{
//					p.active = false;
//					p.target.SetActive(false);
//				}
//			}
		}

		public override string ToString ()
		{
			string str = " ";
//			foreach (Part p in characters) {
//				if (p.active)
//					str += p.name + ",";
//			}

			return str.Substring (0, str.Length - 1).TrimStart ();
		}

		private AnimatorOverrideController m_overrideController;

		public void ReplaceClip ()
		{

			Animator m_animator = GetComponentInChildren<Animator>();

			AnimatorClipInfo[] c1 = m_animator.GetCurrentAnimatorClipInfo (1);
			AnimatorClipInfo[] c2 = m_animator.GetCurrentAnimatorClipInfo (2);
			if (c1.Length > 0) {
				print ("clip1="+c1 [0].clip);
				print ("clip2="+c2 [0].clip);
			}
		
			print (m_animator.runtimeAnimatorController);

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
				List<Toggle> buttons = new List<Toggle> ();
				

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

		// Use this for initialization
		void Awake ()
		{
			
//			foreach (Part c in characters) {
//				c.target.SetActive(c.active);
//			}
		
		}

		void Start ()
		{
			ReplaceClip ();
		}
		
		// Update is called once per frame
		void Update ()
		{
		
		}
	}
}
