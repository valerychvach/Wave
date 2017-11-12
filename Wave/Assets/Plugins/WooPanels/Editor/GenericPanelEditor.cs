using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;

using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Reflection;
using UnityEngine.UI;
using System.Linq;

namespace Wooplex.Panels
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(WooPanel), true)]
	public class GenericPanelEditor : Editor
	{
		private string scriptName = "";
		private bool waitForCompile = false;
		private bool shouldAddScript = false;
		private string componentName;
		private GameObject targetGameObject;

		private bool wasEdited;
		private bool toggleValue;

		private const string PANEL_OPENED_STATE = "PanelOpened";
		private const string PANEL_IDLE_STATE = "PanelIdle";
		private const string PANEL_CLOSED_STATE = "PanelClosed";
		private const string PANEL_NOT_DEFINED_STATE = "NotDefined";
		private const string LAYER_NAME = "Panel";

		private const string ROOT_ANIMATIONS_PATH = "Assets/Animations";
		private const string IS_OPENED = "IsOpened";
		private const string IS_DEFINED = "IsDefined";
		private const string OPENING_SPEED = "OpeningSpeed";
		private const string CLOSING_SPEED = "ClosingSpeed";

		private bool blurSelected = false;
		private bool whiteSelected = false;
		private bool darkSelected = false;
		private bool foldout = false;
		private bool eventsFoldout = false;

		public void OnEnable()
		{
			scriptName = target.name;
			//(target as WooPanel).GetComponent<WooPanelGelper>().

			// This is here to instantiate panel manager when first panel script is added
			if (PanelManager.GetPanel<WooPanel>() == null)
			{
				return;
			}
		}

		public override void OnInspectorGUI()
		{
			var it = serializedObject.GetIterator();
			it.NextVisible(true);

			EditorGUILayout.PropertyField(it, true);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("PanelType"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("PanelProperties"), true);

			eventsFoldout = EditorGUILayout.Foldout(eventsFoldout, "Events", true);

			if (eventsFoldout)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPanelOpen"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPanelOpenEnd"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPanelClose"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPanelCloseEnd"));
			}


			serializedObject.ApplyModifiedProperties();

			var prevColor = GUI.contentColor;
			GUI.contentColor = Color.red;

			GUI.contentColor = prevColor;
			WooPanel myScript = (WooPanel)target;
			if (myScript == null)
			{
				return;
			}

			var animator = myScript.gameObject.GetComponent<Animator>();

			if (targets.Length > 1)
				return;

			ProcessEffects();

			if (animator.runtimeAnimatorController == null || animator.runtimeAnimatorController.name == "Default Panel Controller")
			{
				var controllerName = target.name + " Controller.controller";

				if(GUILayout.Button("Create Mirrored \"" + controllerName + "\""))
				{
					CreateMirroredAnimator(myScript.gameObject, controllerName);
				}

				if(GUILayout.Button("Create \"" + controllerName + "\""))
				{
					CreateAnimator(myScript.gameObject, controllerName);
				}
			}
			else
			{
				bool found = false;

				if (!myScript.IsMirrored())
				{
					if (GUILayout.Button("Make Open/Close mirrored"))
					{
						ConvertToMirrored();
					}
				}
				else
				{
					if (GUILayout.Button("Make Open/Close separate"))
					{
						ConvertToSeparate();
					}
				}
				for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
				{
					var clip = animator.runtimeAnimatorController.animationClips[i];
					if (clip.name == PANEL_OPENED_STATE)
					{
						found = true;
					}
				}

				if (!found)
				{
					if (GUILayout.Button("Add Mirrored states to controller"))
					{
						CreateAnimatorStates(animator.runtimeAnimatorController as AnimatorController, true);
					}

					if (GUILayout.Button("Add Open/Close states to controller"))
					{
						CreateAnimatorStates(animator.runtimeAnimatorController as AnimatorController);
					}
				}
			}

			if (target.GetType() == typeof(WooPanel))
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Script Name: ");
				var prevScriptName = scriptName;
				if (!wasEdited)
				{
					scriptName = target.name;
				}

				scriptName = GUILayout.TextField(scriptName);
				if (scriptName != prevScriptName)
				{
					wasEdited = true;
				}
				bool exists = false;

				if (!File.Exists(GetFilePath(scriptName)))
				{
					if (GUILayout.Button("Create \"" + GenerateFileName(scriptName) + "\""))
					{
						CreateScript(((WooPanel)target).gameObject, scriptName);
					}
				}
				else
				{
					if (GUILayout.Button("Replace with \"" + GenerateFileName(scriptName) + "\""))
					{
						CreateScript(((WooPanel)target).gameObject, scriptName);
					}
					exists = true;
				}

				GUILayout.EndHorizontal();

				var msg = "";

				if (waitForCompile)
				{
					msg = "Please, wait while the script is compiling. It will add automatically.";
				}
				else if (exists)
				{
					msg = "Script with this name already exists. ";
				}
				else
				{
					msg = "Component will be created and will replace WooPanel on this gameobject.";
				}

				EditorGUILayout.HelpBox(msg, MessageType.Info);
			}

			if (waitForCompile)
			{
				if (!EditorApplication.isCompiling)
				{
					if (shouldAddScript)
					{
						waitForCompile = false;
						shouldAddScript = false;
						targetGameObject.AddComponent(System.Type.GetType(componentName + ",Assembly-CSharp"));
						DestroyImmediate(targetGameObject.GetComponent<WooPanel>());

						return;
					}
					shouldAddScript = true;
				}
			}

			if (!(target is GenericPanelEditor))
			{
				while (it.NextVisible(false))
				{
					var name = it.name;

					if (name != "PanelType" && name != "PanelProperties" &&
						name != "OnPanelOpen" && name != "OnPanelOpenEnd" &&
						name != "OnPanelClose" && name != "OnPanelCloseEnd")
					{
						EditorGUILayout.PropertyField(it, true);
					}
				}

			}

			if (!PanelManagerWindow.IsWindowOpen)
			{
				if (GUILayout.Button("Open Panel Manager"))
				{
					PanelManagerWindow.DoWindow();
				}
			}


			serializedObject.ApplyModifiedProperties();		
		}

		private void ProcessEffects()
		{
			WooPanel myScript = (WooPanel)target;

			foldout = EditorGUILayout.Foldout(foldout, "Effects", true);

			if (foldout)
			{
				blurSelected = GUILayout.Button("Add Blur");
				whiteSelected = GUILayout.Button("Add White Background");
				darkSelected = GUILayout.Button("Add Dark Background");
			}

			var effects = myScript.transform.Find("Effects");

			if (blurSelected || whiteSelected || darkSelected)
			{
				if (myScript.transform.Find("Effects") == null)
				{
					effects = CreateGameObject("Effects", myScript.transform).transform;
					effects.SetAsFirstSibling();
				}

				if (blurSelected)
				{
					var blur = effects.Find("Blur");
					if (blur == null)
					{
						var go = CreateGameObjectWithRawImage("Blur", effects);
						var blurMaterial = Resources.Load<Material>("WooPanels/Materials/Blurred");;

						go.GetComponent<RawImage>().material = blurMaterial;
					}
				}

				if (whiteSelected)
				{
					var blur = effects.Find("Whiten");
					if (blur == null)
					{
						var go = CreateGameObjectWithRawImage("Whiten", effects);
						go.GetComponent<RawImage>().color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
					}
				}
				if (darkSelected)
				{
					var blur = effects.Find("Darken");
					if (blur == null)
					{
						var go = CreateGameObjectWithRawImage("Darken", effects);
						go.GetComponent<RawImage>().color = new Color(0.0f, 0.0f, 0.0f, 0.3f);
					}
				}
			}
		}

		private GameObject CreateGameObjectWithRawImage(string name, Transform transform)
		{
			var go = CreateGameObject(name, transform);

			go.AddComponent<RawImage>();

			return go;
		}

		private GameObject CreateGameObject(string name, Transform transform)
		{
			var go = new GameObject();

			go.transform.parent = transform;
			var panelRectTransform = go.AddComponent<RectTransform>();

			panelRectTransform.anchorMin = new Vector2(0, 0);
			panelRectTransform.anchorMax = new Vector2(1, 1);
			panelRectTransform.sizeDelta = new Vector2(0.0f, 0.0f);

			panelRectTransform.pivot = new Vector2(0.5f, 0.5f);
			panelRectTransform.localScale = Vector3.one;
			panelRectTransform.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

			go.name = name;

			return go;
		}

		private string GenerateScriptName(string name)
		{
			name = RemoveSpecialCharacters(scriptName);
			name = name.Replace("-","_");
			name = name.Replace(" ", "");
			//name += "Behaviour";

			return name;
		}

		private string GenerateFileName(string name)
		{
			return GenerateScriptName(name) + ".cs";
		}

		private string GetFilePath(string name)
		{
			string copyPath = "Assets/Scripts/Panels/" + GenerateFileName(scriptName);

			return copyPath;
		}

		private void CreateScript(GameObject target, string scriptName)
		{
			string copyPath = GetFilePath(scriptName);

			if (!Directory.Exists("Assets/Scripts/Panels/"))
			{
				Directory.CreateDirectory("Assets/Scripts/Panels/");
			}

			if( File.Exists(copyPath) == false ){ // do not overwrite
				using (StreamWriter outfile = 
					new StreamWriter(copyPath))
				{
					outfile.WriteLine("using UnityEngine;");
					outfile.WriteLine("using UnityEngine.UI;");
					outfile.WriteLine("using System.Collections;");
					outfile.WriteLine("using System.Collections.Generic;");
					outfile.WriteLine("using Wooplex.Panels;");

					outfile.WriteLine("");
					outfile.WriteLine("public class " + GenerateScriptName(scriptName) + " : WooPanel");
					outfile.WriteLine("{");
					outfile.WriteLine("\t// At the beginning even if the panel is closed ");
					outfile.WriteLine("\tvoid OnInit ()");
					outfile.WriteLine("\t{");
					outfile.WriteLine("\t\t");
					outfile.WriteLine("\t}");
					outfile.WriteLine("");
					outfile.WriteLine("\t// Before animation starts");
					outfile.WriteLine("\tvoid OnOpen ()");
					outfile.WriteLine("\t{");
					outfile.WriteLine("\t\t");
					outfile.WriteLine("\t}");
					outfile.WriteLine("");
					outfile.WriteLine("\t// After animation finishes");
					outfile.WriteLine("\tvoid OnOpenEnd ()");
					outfile.WriteLine("\t{");
					outfile.WriteLine("\t\t");
					outfile.WriteLine("\t}");
					outfile.WriteLine("");
					outfile.WriteLine("\t// Before animation starts");
					outfile.WriteLine("\tvoid OnClose ()");
					outfile.WriteLine("\t{");
					outfile.WriteLine("\t\t");
					outfile.WriteLine("\t}");
					outfile.WriteLine("");
					outfile.WriteLine("\t// After animation finishes");
					outfile.WriteLine("\tvoid OnCloseEnd ()");
					outfile.WriteLine("\t{");
					outfile.WriteLine("\t\t");
					outfile.WriteLine("\t}");
					outfile.WriteLine("");
					outfile.WriteLine("\t// Every frame when the panel is opened or being opened");
					outfile.WriteLine("\tvoid PanelUpdate ()");
					outfile.WriteLine("\t{");
					outfile.WriteLine("\t\t");
					outfile.WriteLine("\t}");
					outfile.WriteLine("}");
				}

				AssetDatabase.ImportAsset(copyPath, ImportAssetOptions.Default);
				AssetDatabase.Refresh();
			}

			waitForCompile = true;
			componentName = GenerateScriptName(scriptName);
			targetGameObject = target;
		}

		public static string RemoveSpecialCharacters(string str) {
			StringBuilder sb = new StringBuilder();
			foreach (char c in str) {
				if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == ' ') {
					sb.Append(c);
				}
			}
			return sb.ToString();
		}

		public void CreateMirroredAnimator(GameObject go, string name)
		{
			CreateAnimator(go, name, true);
		}

		public void CreateAnimator(GameObject go, string name, bool mirrored = false)
		{
			var path = ROOT_ANIMATIONS_PATH;

			System.IO.Directory.CreateDirectory(path);
			var filePath = AssetDatabase.GenerateUniqueAssetPath(path.Substring(path.IndexOf("Assets")) + "/" + name);
			var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(filePath);

			CreateAnimatorStates(controller, mirrored);

			go.GetComponent<Animator>().runtimeAnimatorController = controller;
		}

		private AnimatorState GetState(AnimatorControllerLayer layer, string stateName)
		{
			var states = layer.stateMachine.states;

			for (int i = 0; i < states.Length; i++)
			{
				if (states[i].state.name == stateName)
				{
					return states[i].state;
				}
			}

			return null;
		}
		private AnimatorControllerLayer GetLayer(Animator animator)
		{
			var layers = (animator.runtimeAnimatorController as AnimatorController).layers;

			for (int i = 0; i < layers.Length; i++)
			{
				if (layers[i].name == LAYER_NAME)
				{
					return layers[i];
				}
			}

			return null;
		}

		private void ConvertToMirrored()
		{
			var panel = target as WooPanel;

			if (!panel.IsMirrored())
			{
				var animator = panel.GetAnimator();

				var layer = GetLayer(animator);

				var closedState = GetState(layer, PANEL_CLOSED_STATE);
				var openedState = GetState(layer, PANEL_OPENED_STATE);
				closedState.motion = openedState.motion;
				closedState.speed = -1.0f;
			}
		}

		private void ConvertToSeparate()
		{
			var panel = target as WooPanel;

			if (panel.IsMirrored())
			{
				var animator = panel.GetAnimator();

				var layer = GetLayer(animator);

				var closedState = GetState(layer, PANEL_CLOSED_STATE);
				var openedState = GetState(layer, PANEL_OPENED_STATE);

				AnimationClip closingClip = new AnimationClip();
				var openingClip = openedState.motion as AnimationClip;
				var bindings = AnimationUtility.GetCurveBindings(openingClip);
				foreach (var binding in bindings)
				{
					var curve = AnimationUtility.GetEditorCurve(openingClip, binding);
					AnimationUtility.SetEditorCurve(closingClip, binding, curve);
				}
				Reverse(closingClip);

				closingClip.name = PANEL_CLOSED_STATE;
				closedState.motion = closingClip;
				closedState.speed = 1.0f;
			}
		}

		public static void Reverse(AnimationClip clip)
		{
			if (clip == null)
				return;
			
			float clipLength = clip.length;
			var bindings = AnimationUtility.GetCurveBindings(clip);

			foreach (var binding in bindings)
			{
				var curve = AnimationUtility.GetEditorCurve(clip, binding);
				var keys = curve.keys;
				int keyCount = keys.Length;
				var postWrapmode = curve.postWrapMode;
				curve.postWrapMode = curve.preWrapMode;
				curve.preWrapMode = postWrapmode;
				for(int i = 0; i < keyCount; i++ )
				{
					Keyframe K = keys[i];
					K.time = clipLength - K.time;
					var tmp = -K.inTangent;
					K.inTangent = -K.outTangent;
					K.outTangent = tmp;
					keys[i] = K;
				}
				curve.keys = keys;
				AnimationUtility.SetEditorCurve(clip, binding, curve);
			}

			var events = AnimationUtility.GetAnimationEvents(clip);
			if (events.Length > 0)
			{
				for (int i = 0; i < events.Length; i++)
				{
					events[i].time = clipLength - events[i].time;
				}
				AnimationUtility.SetAnimationEvents(clip,events);
			}
		}

		private void CreateAnimatorStates(AnimatorController controller, bool mirrored = false)
		{
			AnimationClip PanelOpenedClip = new AnimationClip();
			PanelOpenedClip.name = PANEL_OPENED_STATE;

			AnimationClip PanelClosedClip = new AnimationClip();
			PanelClosedClip.name = PANEL_CLOSED_STATE;

			AnimationCurve curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.4f, 1.0f);

			PanelOpenedClip.SetCurve("", typeof(CanvasGroup), "m_Alpha", curve);
			curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.01f, 1.0f);
			PanelOpenedClip.SetCurve("", typeof(CanvasGroup), "m_Interactable", curve);
			PanelOpenedClip.SetCurve("", typeof(CanvasGroup), "m_BlocksRaycasts", curve);

			curve = AnimationCurve.EaseInOut(0.0f, 1.0f, 0.4f, 0.0f);
			PanelClosedClip.SetCurve("", typeof(CanvasGroup), "m_Alpha", curve);

			curve = AnimationCurve.EaseInOut(0.0f, 1.0f, 0.0f, 0.0f);
			PanelClosedClip.SetCurve("", typeof(CanvasGroup), "m_Interactable", curve);
			PanelClosedClip.SetCurve("", typeof(CanvasGroup), "m_BlocksRaycasts", curve);

			curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.0f, 0.0f);

			//		controller.AddParameter(IS_OPENED, AnimatorControllerParameterType.Bool);
			//		controller.AddParameter(IS_DEFINED, AnimatorControllerParameterType.Bool);
			controller.AddParameter(OPENING_SPEED, AnimatorControllerParameterType.Float);
			controller.AddParameter(CLOSING_SPEED, AnimatorControllerParameterType.Float);

			AnimatorState statePanelClosed;

			AnimatorControllerLayer layer = new AnimatorControllerLayer();
			layer.name = LAYER_NAME;
			layer.defaultWeight = 1.0f;
			layer.stateMachine = new AnimatorStateMachine();

			if (mirrored)
			{
				statePanelClosed = layer.stateMachine.AddState(PANEL_CLOSED_STATE);

				statePanelClosed.motion = PanelOpenedClip;
				statePanelClosed.speed = -1.0f;
				statePanelClosed.speedParameterActive = true;
				statePanelClosed.speedParameter = CLOSING_SPEED;
			}
			else
			{
				statePanelClosed = layer.stateMachine.AddState(PANEL_CLOSED_STATE);

				statePanelClosed.motion = PanelClosedClip;
				statePanelClosed.speed = 1.0f;
				statePanelClosed.speedParameterActive = true;
				statePanelClosed.speedParameter = CLOSING_SPEED;
			}

			AnimatorState statePanelNotDefined = layer.stateMachine.AddState(PANEL_NOT_DEFINED_STATE);


			controller.AddLayer(layer);
			var layerIndex = GetLayerIndex(layer, controller);

			var statePanelOpened = controller.AddMotion(PanelOpenedClip, layerIndex);
			statePanelOpened.speed = 1.0f;
			statePanelOpened.speedParameterActive = true;
			statePanelOpened.speedParameter = OPENING_SPEED;
			layer.stateMachine.defaultState = statePanelNotDefined;

			layer.stateMachine.hideFlags = HideFlags.HideInInspector;
			AssetDatabase.AddObjectToAsset(layer.stateMachine, controller);

			AssetDatabase.AddObjectToAsset(statePanelOpened, controller);
			AssetDatabase.AddObjectToAsset(statePanelClosed, controller);
			AssetDatabase.AddObjectToAsset(statePanelNotDefined, controller);

			AssetDatabase.AddObjectToAsset(PanelOpenedClip, controller);
			AssetDatabase.AddObjectToAsset(PanelClosedClip, controller);

			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
		}

		private int GetLayerIndex(AnimatorControllerLayer layer, AnimatorController controller)
		{
			var layerIndex = 0;
			for (int i = 0; i < controller.layers.Length; i++)
			{
				if (controller.layers[i].name == layer.name)
				{
					layerIndex = i;
				}
			}

			return layerIndex;
		}
	}

	[CustomEditor(typeof(PanelManager))]
	public class PanelManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (!PanelManagerWindow.IsWindowOpen)
			{
				if (GUILayout.Button("Open Panel Manager"))
				{
					PanelManagerWindow.DoWindow();
				}
			}
		}
	}
}
