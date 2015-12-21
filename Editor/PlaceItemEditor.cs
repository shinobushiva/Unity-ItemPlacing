using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityStandardAssets.ImageEffects;

[CustomEditor (typeof(PlaceItem))]
public class PlaceItemEditor : Editor
{

	int resWidth = 512;
	int resHeight = 512;

	public Vector3 offset;
	public Vector3 offsetRot;

	public bool keepCamera;

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

//		EditorGUILayout.BeginVertical (GUILayout.ExpandWidth (true));
		{
//			EditorGUILayout.DoubleField (10);


			EditorGUILayout.Separator ();
			EditorGUILayout.HelpBox ("Here you can setup the icon for this Item", MessageType.Info);

			PlaceItem pi = target as PlaceItem;

			keepCamera = EditorGUILayout.Toggle ("KeepCamera", keepCamera);
			offset = EditorGUILayout.Vector3Field ("Screenshot Offset", offset);
			offsetRot = EditorGUILayout.Vector3Field ("Screenshot Offset Rot", offsetRot);


			if (GUILayout.Button ("SetIcon")) {
				Debug.Log ("SetIcon");

				GameObject go = new GameObject ("Tmp Camera");
				Camera c = go.AddComponent<Camera> ();

				GameObject g = pi.gameObject;
				Vector3 orgPos = g.transform.position;
				int layer = g.layer;

				if(!keepCamera)
					g.transform.position = Vector3.one * 100000;

				Bounds bb = GetBoundingBox (g);

				c.transform.position = bb.center + Vector3.one * bb.extents.magnitude * 1.1f;
				c.transform.LookAt (g.transform.position, Vector3.up);
				c.cullingMask = LayerMask.GetMask (LayerMask.LayerToName (layer));
				c.transform.Translate (offset);
				c.transform.Rotate (offsetRot);
				c.clearFlags = CameraClearFlags.SolidColor;


				Antialiasing aa = c.gameObject.AddComponent<Antialiasing> ();
				aa.ssaaShader = Shader.Find ("Hidden/SSAA");
				aa.dlaaShader = Shader.Find ("Hidden/DLAA");
				aa.nfaaShader = Shader.Find ("Hidden/NFAA");
				aa.shaderFXAAPreset2 = Shader.Find ("Hidden/FXAA Preset 2");
				aa.shaderFXAAPreset3 = Shader.Find ("Hidden/FXAA Preset 3");
				aa.shaderFXAAII = Shader.Find ("Hidden/FXAA II");
				aa.mode = AAMode.FXAA3Console;


				RenderTexture rt = new RenderTexture (resWidth, resHeight, 24);
				c.targetTexture = rt;
				Texture2D screenShot = new Texture2D (resWidth, resHeight, TextureFormat.RGB24, false);
				c.Render ();
				RenderTexture.active = rt;
				screenShot.ReadPixels (new Rect (0, 0, resWidth, resHeight), 0, 0);
				c.targetTexture = null;
				RenderTexture.active = null; // JC: added to avoid errors
				DestroyImmediate (rt);
				byte[] bytes = screenShot.EncodeToPNG ();
				string filename = ScreenShotName (resWidth, resHeight, g.name);
				System.IO.File.WriteAllBytes (filename, bytes);
				Debug.Log (string.Format ("Took screenshot to: {0}", filename));

				if(!keepCamera)
					DestroyImmediate (c.gameObject);

				int idx = filename.IndexOf ("Assets");
				filename = filename.Substring (idx, filename.Length - idx);
				Debug.Log (filename);
				AssetDatabase.ImportAsset (filename);

//				string guid = AssetDatabase.AssetPathToGUID (filename);

				TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath (filename);
				if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single) {
					
					importer.textureType = TextureImporterType.Sprite;
					importer.spriteImportMode = SpriteImportMode.Single;
					AssetDatabase.ImportAsset (filename, ImportAssetOptions.ForceUpdate);
					importer.maxTextureSize = 512;
				}

				Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite> (filename);
				pi.sprite = sp;
				if (pi.itemName == null || pi.itemName.Trim().Length == 0) {
					pi.itemName = pi.gameObject.name;
				}


				g.transform.position = orgPos;

				//			if (GUI.changed)
				EditorUtility.SetDirty (target);
			}

			GUI.enabled = false;
			EditorGUILayout.ObjectField ("Sprite", pi.sprite, typeof(Sprite), false);
			GUI.enabled = true;

		}
//		EditorGUILayout.EndVertical ();

	}

	public static string ScreenShotName (int width, int height, string name)
	{
		System.IO.Directory.CreateDirectory (string.Format ("{0}/screenshots/", Application.dataPath));
			
		return string.Format ("{0}/screenshots/{3}_{1}x{2}_PlaceItem.png", 
			Application.dataPath, 
			width, height, name);
	}

	public Bounds GetBoundingBox (GameObject go, bool rotationVariant = false)
	{

		Renderer[] rs = go.GetComponentsInChildren<Renderer> ();
		if (rs.Length <= 0) {
			return new Bounds ();
		}

		Bounds b = new Bounds (rs [0].bounds.center, rs [0].bounds.size);
		for (int i = 1; i < rs.Length; i++) {
			b.Encapsulate (rs [i].bounds);
		}

		return b;
	}
}
