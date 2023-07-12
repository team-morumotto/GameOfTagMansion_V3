//https://unitech.hatenablog.com/entry/2018/03/21/184223


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

public class CustomCopyComponent : EditorWindow
{
	static Component[] targetComponents;
	static bool[] isCopyComponents;
	static GameObject copyComponentsGameObject;
	static List<Component> copyComponents = new List<Component> ();
	static List<GameObject> targetGameObject = new List<GameObject> ();
	static bool defaultCheck;
	Vector2 scrollPos = Vector2.zero;

	[MenuItem ("CONTEXT/Component/Custom Copy", true)]
	static bool IsEnabled ()
	{
		if (Selection.activeGameObject == null)
			return false;

		var target = Selection.activeGameObject;
		var components = target.GetComponents<Component> ();
		if (components.Length == 0)
			return false;
		
		return true;
	}

	[MenuItem ("CONTEXT/Component/Custom Copy", false, 100)]
	static void CreateWindow ()
	{
		copyComponentsGameObject = Selection.activeGameObject;
		targetComponents = copyComponentsGameObject.GetComponents<Component> ();
		copyComponents.Clear ();
		isCopyComponents = new bool[targetComponents.Length];
		for (int i = 0; i < isCopyComponents.Length; i++)
		{
			isCopyComponents [i] = defaultCheck;
		}
		var window = GetWindow<CustomCopyComponent> ("CustomCopy");

		window.OnSelectionChange ();
	}

	[MenuItem ("CONTEXT/Component/Custom Paste", true)]
	static bool IsPastEnabled ()
	{
		return copyComponents.Count > 0 && targetGameObject.Count > 0;
	}

	[MenuItem ("CONTEXT/Component/Custom Paste", false, 100)]
	static void PastFromContext ()
	{
		Paste ();
	}

	public void OnSelectionChange ()
	{
		targetGameObject.Clear ();
		var targetObjects = Selection.objects;
		foreach (var targetObject in targetObjects)
		{
			if (!(targetObject is GameObject))
				continue;

			if (targetObject == copyComponentsGameObject)
				continue;
			
			targetGameObject.Add ((GameObject)targetObject);
		}
			
		Repaint ();
	}

	private void OnGUI ()
	{
		if (targetComponents == null || targetComponents.Length == 0)
			return;

		scrollPos = EditorGUILayout.BeginScrollView( scrollPos );
		{
			EditorGUILayout.LabelField ("コンポーネント一覧");
			using (new GUILayout.VerticalScope (GUI.skin.box))
			{
				using (new GUILayout.HorizontalScope ())
				{
					EditorGUI.BeginChangeCheck ();
					defaultCheck = EditorGUILayout.Toggle (defaultCheck, GUILayout.Width (30));
					EditorGUILayout.LabelField ("Check", GUILayout.ExpandWidth (true));
					if (EditorGUI.EndChangeCheck ())
					{				 
						for (int i = 0; i < isCopyComponents.Length; i++)
						{
							isCopyComponents [i] = defaultCheck;
						}
					}
				}
				GUILayout.Box (
					string.Empty,
					GUILayout.Width (position.width - 24), 
					GUILayout.Height (1) 
				);
				for (int i = 0; i < targetComponents.Length; i++)
				{
					using (new GUILayout.HorizontalScope ())
					{
						var co = targetComponents [i];
						if (co && co.hideFlags != HideFlags.None)
							continue;
	
						isCopyComponents [i] = EditorGUILayout.Toggle (isCopyComponents [i], GUILayout.Width (30));
						EditorGUILayout.LabelField (co.GetType ().ToString (), GUILayout.ExpandWidth (true));
					}
				}
			}
			if (GUILayout.Button ("Copy"))
			{
				copyComponents.Clear ();
				for (int i = 0; i < targetComponents.Length; i++)
				{
					var co = targetComponents [i];

					if (co && !isCopyComponents [i])
						continue;

					copyComponents.Add (co);
				}
			}

			EditorGUILayout.Space ();

			if (copyComponents.Count > 0)
			{
				if (GUILayout.Button("コピー中:" + copyComponents [0].name, GUI.skin.label))
				{
					Selection.objects = new[] { copyComponentsGameObject };
				}
				using (new GUILayout.VerticalScope (GUI.skin.box))
				{
					for (int i = 0; i < copyComponents.Count; i++)
					{
						var coco = copyComponents [i];
						if (coco)
						{
							EditorGUILayout.LabelField (coco.GetType ().ToString ());
						}
					}
				}

				EditorGUI.BeginDisabledGroup (!IsPastEnabled ());
				if (GUILayout.Button ("Paste"))
				{
					Paste ();
				}
				EditorGUI.EndDisabledGroup ();
			}
		}
		EditorGUILayout.EndScrollView();
	}

	static public void Paste ()
	{
		foreach (var copyComponent in copyComponents)
		{
			if (!copyComponent)
				continue;
			
			var copyComponentType = copyComponent.GetType ();
			UnityEditorInternal.ComponentUtility.CopyComponent (copyComponent);

			var targetObjects = Selection.objects;
			foreach (var targetObject in targetObjects)
			{
				if (!(targetObject is GameObject))
					continue;

				var target = (GameObject)targetObject;

				var targetComponent = target.GetComponent (copyComponentType);
				if (targetComponent)
				{
					UnityEditorInternal.ComponentUtility.PasteComponentValues (targetComponent);
				}
				else
				{
					UnityEditorInternal.ComponentUtility.PasteComponentAsNew (target);
				}
			}
		}
	}
}