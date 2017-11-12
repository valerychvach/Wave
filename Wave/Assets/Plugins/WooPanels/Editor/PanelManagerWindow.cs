using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Wooplex.Panels
{
	public class PanelManagerWindow: EditorWindow
	{
		protected GameObject go;
		protected AnimationClip animationClip;
		protected float time = 0.0f;
		protected bool lockSelection = false;
		protected bool animationMode = false;

		public Dictionary<WooPanel, WooPanelNode> panelNodes
		{
			get
			{
				return WooPanelsEngine.Instance.panelNodes;
			}
		}

		public Dictionary<Transform, int> rootsHierarchyIndeces
		{
			get
			{
				return WooPanelsEngine.Instance.rootsHierarchyIndeces;
			}
		}

		public Dictionary<Transform, List<WooPanelNode>> allRootNodes
		{
			get
			{
				return WooPanelsEngine.Instance.allRootNodes;
			}
		}

		protected Vector2 currentScrollPosition = new Vector2(0.0f, 0.0f);

		internal bool editAnimators = true;
		private bool showIcons = true;
		private bool createMode = true;
		private bool showType = false;
		private bool showControllers = false;
		private bool showTexts = false;
		private bool simulate = false;
		bool prevIsPlaying = false;

		private Color LinesColor = new Color(255.0f / 255.0f, 200.0f / 255.0f, 0.0f / 255.0f);

		private Rect m_LastGraphExtents;
		private static readonly Color kGridMinorColorDark = new Color(0.3f, 0.3f, 0.3f, 0.18f);
		private static readonly Color kGridMajorColorDark = new Color(0f, 0f, 0f, 0.28f);

		private Material lineMaterial;

		private static Texture openWithParentTexture;
		private static Texture openWithChildTexture;
		private static Texture openOnAwakeTexture;
		private static Texture arrowDownTexture;
		private static Texture arrowSideTexture;
		private static Texture selectedTexture;

		private WooPanel selectedPanel;

		static internal PanelManagerWindow Instance = null;

		private int selectedRoot = 0;

		public static bool IsWindowOpen
		{
			get { return Instance != null; }
		}

		private void CreateLineMaterial()
		{
			if (!lineMaterial)
			{
				lineMaterial = new Material(Shader.Find("WooPanels/GLlineZOff"));
			}
		}

		[MenuItem("Window/Panel Manager", false, 2000)]
		public static void DoWindow()
		{
			var text = new GUIContent();
			text.text = "Panel Manager";

			Instance = GetWindow<PanelManagerWindow>();
			Instance.titleContent = text;
			Instance.wantsMouseMove = false;
		}

		public void OnEnable()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			CreateLineMaterial();

//			if (!Application.isPlaying)
//			{
//				WooPanelsEngine.Instance.Rebuild();
//			}


			if (selectedRoot > allRootNodes.Count)
			{
				selectedRoot = allRootNodes.Count - 1;
			}

			openWithParentTexture = (Texture)Resources.Load("WooPanels/Textures/IconWithParent");
			openWithChildTexture = (Texture)Resources.Load("WooPanels/Textures/IconWithChild");
			openOnAwakeTexture = (Texture)Resources.Load("WooPanels/Textures/IconOnAwake");
			arrowDownTexture = (Texture)Resources.Load("WooPanels/Textures/ArrowDown");
			arrowSideTexture = (Texture)Resources.Load("WooPanels/Textures/ArrowSide");
			selectedTexture = (Texture)Resources.Load("WooPanels/Textures/SelectedTexture");

			Repaint();

			if (!Application.isPlaying)
			{
				RestartSimulationOrEnableAll();
			}

			Repaint();
		}

		private void OnDisable()
		{
			if (AnimationMode.InAnimationMode())
			{
				AnimationMode.StopAnimationMode();
			}
		}

		void SaveAnimationMode()
		{
			if (editAnimators)
			{
				saveEditAnimators = true;
				editAnimators = false;
				ToggleAnimationMode();

				if (!Application.isPlaying)
				{
					FinishOpeningAndClosing();
					Update();
				}
			}
		}

		void RecoverAnimationMode()
		{
			if (saveEditAnimators)
			{
				saveEditAnimators = false;
				editAnimators = true;
				ToggleAnimationMode();
			}
		}

		void OnBecameInvisible()
		{
			SaveAnimationMode();
		}

		void OnLostFocus()
		{
			if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Animation")
			{
				SaveAnimationMode();
			}
		}

		void OnFocus()
		{
			RecoverAnimationMode();
		}

		void OnBecameVisible()
		{
			RecoverAnimationMode();
		}

		public void OnSelectionChange()
		{
			var selectedPanel = WooPanelsEngine.Instance.GetSelectedPanel(Selection.activeTransform);

			if (selectedPanel != null && selectedRoot != 0)
			{
				var rootParent = WooPanelsEngine.Instance.GetRoot(selectedPanel);
				selectedRoot = allRootNodes.Keys.ToList().IndexOf(rootParent) + 1;
			}


		}

		private void OnHierarchyChange()
		{
			if (!Application.isPlaying)
			{
				WooPanelsEngine.Instance.Rebuild();
			}
		}

		private void DrawPanelButton(WooPanel panel, int level)
		{
			
			var prevBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.clear;

			if (panelNodes[panel].Children.Count > 0)
			{
				GUILayout.Space(8 + level * 30.0f);
				if (!panelNodes[panel].Collapsed)
				{
					if (GUILayout.Button(arrowDownTexture, GUILayout.Width(25), GUILayout.Height(30)))
					{
						panelNodes[panel].Collapsed = true;
					}
				}
				else
				{
					if (GUILayout.Button(arrowSideTexture, GUILayout.Width(25), GUILayout.Height(30)))
					{
						panelNodes[panel].Collapsed = false;
					}
				}
				GUILayout.Space(8.0f);
			}
			else
			{
				GUILayout.Space(45.0f + level * 30.0f);
			}

			var openedColor = new Color(0.2f, 1.0f, 0.05f);
			var closedColor = new Color(0.5f, 0.5f, 0.5f);

			panelNodes[panel].TargetColor = WooPanelsEngine.Instance.IsOpen(panel) || WooPanelsEngine.Instance.IsWaitingToClose(panel)? openedColor : closedColor;

			if (!panel.gameObject.activeInHierarchy)
			{
				panelNodes[panel].TargetColor.a = 0.5f;
			}

			if (editAnimators)
			{
				GUI.backgroundColor = panelNodes[panel].Color;
			}
			else
			{
				GUI.backgroundColor = panelNodes[panel].TargetColor;
			}

			//var buttonName = showType ? panel.name + "\n<" + panel.GetType().Name + ">" : panel.name;
			var buttonName = panel.name;

			GUI.SetNextControlName(panel.gameObject.GetInstanceID().ToString());

			if (GUILayout.Button(buttonName, GUILayout.Width(150), GUILayout.Height(30)))
			{
				Selection.activeGameObject = panel.gameObject;

				if (simulate)
				{
					if (!(WooPanelsEngine.Instance.IsOpen(panel) || panelNodes[panel].IsOpening))
					{
						Open(panel);
					}
					else
					{
						Close(panel);
					}
				}
			}
			panelNodes[panel].rect = GUILayoutUtility.GetLastRect();

			GUI.backgroundColor = prevBackgroundColor;
		}

		private void Open(WooPanel panel)
		{
			if (editAnimators)
			{
				WooPanelsEngine.Instance.Open(panel);
			}
			else
			{
				WooPanelsEngine.Instance.Open(panel, true);
			}
		}

		private void Close(WooPanel panel, bool immmediate = false)
		{
			if (editAnimators && !immmediate)
			{
				WooPanelsEngine.Instance.Close(panel);
			}
			else
			{
				WooPanelsEngine.Instance.Close(panel, true);
			}
		}

		private void DrawSelectedIndicator(Rect rect, bool outline = false)
		{
			Handles.color = new Color(1.0f, 0.7f, 0.2f);
			Handles.BeginGUI();
			Handles.DrawSolidDisc(new Vector3(rect.position.x + 10.0f, rect.position.y + rect.height / 2.0f, 0.0f), Vector3.forward, 3.0f);
	
			if (outline)
			{
				Handles.color = Color.black;
				Handles.DrawWireDisc(new Vector3(rect.position.x + 10.0f, rect.position.y + rect.height / 2.0f, 0.0f), Vector3.forward, 3.0f);
			}

			rect.position -= new Vector2(9.0f, 10.0f);
			rect.width += 20.0f;
			rect.height += 20.0f;
			GUI.DrawTexture(rect, selectedTexture);

			Handles.EndGUI();
			Handles.color = Color.white;	
		}

		private void DrawSelectedIndicator(WooPanel panel)
		{
			if (selectedPanel == panel)
			{
				DrawSelectedIndicator(panelNodes[panel].rect, WooPanelsEngine.Instance.IsOpen(panel));
			}
		}

		private void DrawAlwaysOnTopIndicator(WooPanel panel)
		{
			if (panel.PanelProperties.AlwaysOnTop)
			{
				var rect = panelNodes[panel].rect;
				Handles.color = new Color(0.2f, 0.7f, 1.0f);
				Handles.BeginGUI();
				Handles.DrawSolidDisc(new Vector3(rect.position.x + rect.width - 5.0f, rect.position.y + 5.0f, 0.0f), Vector3.forward, 2.0f);

				if (WooPanelsEngine.Instance.IsOpen(panel))
				{
					Handles.color = Color.black;
					Handles.DrawWireDisc(new Vector3(rect.position.x + rect.width - 5.0f, rect.position.y + 5.0f, 0.0f), Vector3.forward, 2.0f);
				}

				Handles.EndGUI();
				Handles.color = Color.white;	
			}
		}

		private void DrawNode(WooPanel panel, int level)
		{
			if (panel == null)
			{
				return;
			}
			GUILayout.BeginHorizontal();


			DrawPanelButton(panel, level);
			DrawSelectedIndicator(panel);
			DrawAlwaysOnTopIndicator(panel);

			if (showIcons)
			{
				DrawPanelPropertiesTextures(panel);
			}

			if (showControllers)
			{
				DrawAnimatorProperty(panel);
			}

			DrawAnimationProgress(panel);

			if (editAnimators)
			{
				DrawAnimatorSliders(panel);
			}

			if (showType)
			{
				DrawScriptProperty(panel);
			}
			if (showTexts)
			{
				DrawPanelPropertiesText(panel);
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			var children = WooPanelsEngine.Instance.GetChildren(panel);

			if (!panelNodes[panel].Collapsed)
			{
				for (int i = 0; i < children.Count; i++)
				{
					DrawNode(children[i].Panel, level + 1);
				}
			}

			if (!Application.isPlaying)
			{
				DrawCreateButtons(panel, level);
			}

		}

		private void DrawAnimatorProperty(WooPanel panel)
		{
			GUILayout.BeginVertical();
			GUILayout.Space(10.0f);
			panel.GetComponent<Animator>().runtimeAnimatorController = EditorGUILayout.ObjectField(panel.GetComponent<Animator>().runtimeAnimatorController, typeof(UnityEditor.Animations.AnimatorController), false) as UnityEditor.Animations.AnimatorController;
			GUILayout.EndVertical();
		}

		private void DrawScriptProperty(WooPanel panel)
		{
			GUILayout.BeginVertical();
			GUILayout.Space(10.0f);
			var serializedObject = new SerializedObject(panel.GetComponent<WooPanel>());
			var it = serializedObject.GetIterator();
			it.NextVisible(true);

			EditorGUILayout.PropertyField(it, GUIContent.none, true);

			GUILayout.EndVertical();
		}

		private void DrawCreateButtons(WooPanel panel, int level)
		{
			if (selectedPanel != null && panelNodes.ContainsKey(selectedPanel) && panelNodes[selectedPanel] != null)
			{
				if (panelNodes[selectedPanel].Children.Count > 0 && panelNodes[panel].Parent != null)
				{
					if (panelNodes[selectedPanel].Children.Last() == panelNodes[panel])
					{
						DrawCreatePanelButton("Create Child Panel", level, panelNodes[selectedPanel].Panel.transform);
					}
				}
				else if (panelNodes[selectedPanel].Children.Count == 0 && selectedPanel == panel)
				{
					DrawCreatePanelButton("Create Child Panel", level + 1, panel.transform);
				}
				if (panelNodes[selectedPanel].ParentTransform == panelNodes[panel].ParentTransform)
				{
					if (panelNodes[selectedPanel].Parent == null)
					{
						if (allRootNodes[panelNodes[panel].ParentTransform].Last().Panel == panel)
						{
							DrawCreatePanelButton("Create Sibling Panel", level, panel.transform.parent);
						}
					}
					else
					{
						if (panelNodes[selectedPanel].Parent.Children.Count > 0 && panelNodes[selectedPanel].Parent.Children.Last().Panel == panel)
						{
							DrawCreatePanelButton("Create Sibling Panel", level, panel.transform.parent);
						}
					}
				}
			}

		}

		private void DrawGridLines(float gridSize, Color gridColor)
		{
			GL.Color(gridColor);
			for (float x = 0.0f; x < Screen.width; x += gridSize)
				DrawLine(new Vector2(x, 0.0f), new Vector2(x, Screen.height));
			GL.Color(gridColor);
			for (float y = 0.0f; y < Screen.height; y += gridSize)
				DrawLine(new Vector2(0.0f, y), new Vector2(Screen.width, y));
		}

		private void DrawLine(Vector2 p1, Vector2 p2)
		{
			GL.Vertex(p1);
			GL.Vertex(p2);
		}

		private void DrawLines(List<WooPanelNode> rootNodes)
		{
			if (rootNodes.Count > 1)
			{
				var firstPanel = rootNodes.Find((p) => p.Panel.PanelType == PanelType.Dependent);
				var lastPanel = rootNodes.FindLast((p) => p.Panel.PanelType == PanelType.Dependent);

				if (firstPanel != lastPanel)
				{
					rootNodes.ForEach((p) =>
						{
							if (p.Panel.PanelType == PanelType.Dependent)
							{
								DrawDependentHandles(p.rect, LinesColor);
							}
						}
					);
				}
				DrawConnectionBetweenPanels(firstPanel, lastPanel, LinesColor);
			}

			for (int i = 0; i < rootNodes.Count; i++)
			{
				DrawLinesRecursive(rootNodes[i].Panel, 1);
			}
		}


		private void DrawConnectionBetweenPanels(WooPanelNode firstPanelNode, WooPanelNode secondPanelNode, Color color)
		{
			if (firstPanelNode != secondPanelNode)
			{
				DrawVerticalLine(color, firstPanelNode.rect.position + new Vector2(-10.0f, firstPanelNode.rect.height / 2.0f), secondPanelNode.rect.position.y - firstPanelNode.rect.position.y);
			}
		}

		private void DrawLinesRecursive(WooPanel panel, int level)
		{
			var children = WooPanelsEngine.Instance.GetChildren(panel);


			if (WooPanelsEngine.Instance.panelNodes[panel].Children.Count > 1)
			{
				var firstPanel = children.Find((p) => p.Panel.PanelType == PanelType.Dependent);
				var lastPanel = children.FindLast((p) => p.Panel.PanelType == PanelType.Dependent);

				if (firstPanel != lastPanel)
				{
					WooPanelsEngine.Instance.panelNodes[panel].Children.ForEach((p) =>
						{
							if (p.Panel.PanelType == PanelType.Dependent)
							{
								DrawDependentHandles((p as WooPanelNode).rect, LinesColor);
							}
						}
					);
				}
				

				DrawConnectionBetweenPanels(firstPanel as WooPanelNode, lastPanel as WooPanelNode, LinesColor);
			}

			for (int i = 0; i < children.Count; i++)
			{
				DrawLinesRecursive(children[i].Panel, level + 1);
			}
		}

		private void DrawAnimationProgress(WooPanel panel)
		{
			var prevColor = GUI.backgroundColor;
			var color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
			var sliderColor = LinesColor;

			var rect = panelNodes[panel].rect;
			rect.position = rect.position + Vector2.right * 1.0f - Vector2.down * rect.height + Vector2.down * 5.0f;
			rect.height = 5.0f;
			rect.width -= 2.0f;

			var sliderValue = 0.0f;

			if (panelNodes[panel].IsClosing || panelNodes[panel].IsClosed)
			{
				sliderValue = 1.0f - panelNodes[panel].simulatedTime;
			}
			else
			{
				sliderValue = panelNodes[panel].simulatedTime;
			}

			if (panelNodes[panel].IsWaiting)
			{
				sliderValue = (panelNodes[panel].waitingTime - panelNodes[panel].remainingWaitingTime) / panelNodes[panel].waitingTime;
				sliderColor = new Color(0.5f, 0.7f, 1.0f, 1.0f);
			}

			if (panelNodes[panel].IsClosing || panelNodes[panel].IsOpening || panelNodes[panel].IsWaiting)
			{
				GUI.backgroundColor = color;

				if (panelNodes[panel].IsWaitingToOpen)
				{
					rect.width *= sliderValue;
				}
				else if (panelNodes[panel].IsWaitingToClose)
				{
					rect.position += rect.width * Vector2.right * (1.0f - sliderValue);
					rect.width *= sliderValue;
				}

				GUI.Box(rect, "");

				if (!panelNodes[panel].IsWaiting)
				{
					rect.width *= sliderValue;
					if (sliderValue > Mathf.Epsilon)
					{
						GUI.backgroundColor = sliderColor;
						GUI.Box(rect, "");
					}
				}

			}

			GUI.backgroundColor = prevColor;
		}

		private void DrawAnimatorSliders(WooPanel panel)
		{
			GUILayout.BeginVertical();
			GUILayout.Space(10.0f);

			if (panelNodes[panel].IsClosing || panelNodes[panel].IsClosed)
			{
				panelNodes[panel].simulatedTime = 1.0f - EditorGUILayout.Slider(1.0f - panelNodes[panel].simulatedTime, 0.0f, 1.0f, GUILayout.Width(150.0f));

			}
			else
			{
				panelNodes[panel].simulatedTime = EditorGUILayout.Slider(panelNodes[panel].simulatedTime, 0.0f, 1.0f, GUILayout.Width(150.0f));
			}

			GUILayout.EndVertical();
		}

		private void DrawPanelPropertiesTextures(WooPanel panel)
		{
			if (panel == null || panel.PanelProperties == null)
			{
				return;
			}

			var shouldOpenWithChild = panel.PanelProperties.WithChild && WooPanelsEngine.Instance.GetChildren(panel) != null && WooPanelsEngine.Instance.GetChildren(panel).Count > 0;
			var shouldOpenWithParent = panel.PanelProperties.WithParent && WooPanelsEngine.Instance.GetParent(panel) != null;

			if (shouldOpenWithChild || shouldOpenWithParent || panel.PanelProperties.OnAwake)
			{
				var iconRect = panelNodes[panel].rect;
				iconRect.position += Vector2.right * (iconRect.width + 5.0f);
				iconRect.width = openWithChildTexture.width * iconRect.height / openWithChildTexture.height;
				GUILayout.Space(30.0f);

				if (shouldOpenWithChild)
				{
					GUI.DrawTexture(iconRect, openWithChildTexture);
				}

				if (shouldOpenWithParent)
				{
					GUI.DrawTexture(iconRect, openWithParentTexture);
				}

				if (panel.PanelProperties.OnAwake)
				{
					GUI.DrawTexture(iconRect, openOnAwakeTexture);
				}
			}
		}

		private void DrawPanelPropertiesText(WooPanel panel)
		{
			string textToDisplay = "";

			if (panel.PanelProperties.WithParent)
			{
				if (WooPanelsEngine.Instance.GetParent(panel) == null)
				{
					textToDisplay += "With parent (doesn't have any)";
				}
				else
				{
					textToDisplay += "With parent";
				}
			}
			if (panel.PanelProperties.WithChild)
			{
				if (WooPanelsEngine.Instance.GetChildren(panel).Count == 0)
				{
					textToDisplay += "\nWith child (doesn't have any)";
				}
				else
				{
					textToDisplay += "\nWith child ";
				}
			}

			if (textToDisplay != "")
			{
				GUILayout.Label(textToDisplay);
			}

			if (panel.PanelProperties.OnAwake)
			{
				GUILayout.BeginVertical();
				GUILayout.Space(10.0f);
				GUILayout.Label("On awake ");
				GUILayout.EndVertical();
			}

			if (panel.PanelProperties.AlwaysOnTop)
			{
				GUILayout.BeginVertical();
				GUILayout.Space(10.0f);
				GUILayout.Label("Always on top");
				GUILayout.EndVertical();
			}
		}

		private void DrawVerticalLine(Color color, Vector2 position, float height)
		{
			var rect = GUILayoutUtility.GetLastRect();

			rect.width = 2.0f;
			rect.position = position;
			rect.height = height;

			Handles.BeginGUI();
			Handles.DrawSolidRectangleWithOutline(rect, color, color);
			Handles.EndGUI();
		}

		private void DrawDependentHandles(Rect rect, Color color)
		{
			rect.width = 10.0f;
			rect.position = rect.position + new Vector2(-10.0f, rect.height / 2.0f);
			rect.height = rect.height / 10.0f;
			Handles.BeginGUI();
			Handles.DrawSolidRectangleWithOutline(rect, color, color);
			Handles.EndGUI();
		}

		private void FinishOpeningAndClosingForPanel(WooPanel panel)
		{
			panel.gameObject.SetActive(true);
			foreach (var childPanel in panelNodes[panel].Children)
			{
				FinishOpeningAndClosingForPanel(childPanel.Panel);
			}

			if (panelNodes[panel].IsOpening || panelNodes[panel].IsOpen || panelNodes[panel].IsWaitingToOpen)
			{
				WooPanelsEngine.Instance.PlayOpenAnimationImmdiately(panel);
				//WooPanelsEngine.Instance.Open(panel, true);
			}
			else if (panelNodes[panel].IsClosing || panelNodes[panel].IsClosed || panelNodes[panel].IsWaitingToClose)
			{
				WooPanelsEngine.Instance.PlayCloseAnimationImmdiately(panel);
				//WooPanelsEngine.Instance.Close(panel, true);
			}
		}

		private void FinishOpeningAndClosing()
		{
			foreach (var rootNodes in allRootNodes)
			{
				foreach (var root in rootNodes.Value)
				{
					FinishOpeningAndClosingForPanel(root.Panel);
				}
			}
		}

		private void ToggleAnimationMode()
		{
			if (editAnimators && !AnimationMode.InAnimationMode())
			{
				AnimationMode.StartAnimationMode();
			}
			if (!editAnimators && AnimationMode.InAnimationMode())
			{
				AnimationMode.StopAnimationMode();
			}
		}

		private void DrawToolbar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			var prevColor = GUI.color;

			if (editAnimators)
			{
				GUI.color = Color.green;
			}

//			if (!Application.isPlaying)
//			{
//				if (simulate != prevSimulate)
//				{
//					RestartSimulationOrEnableAll();
//				}
//			}
			var prevEditAnimators = editAnimators;
			editAnimators = GUILayout.Toggle(editAnimators, "Animate", EditorStyles.toolbarButton, GUILayout.MaxWidth(100.0f));


			ToggleAnimationMode();

			if (!Application.isPlaying)
			{
				if (editAnimators != prevEditAnimators && !editAnimators)
				{
					FinishOpeningAndClosing();
				}	
			}

			GUI.color = prevColor;
			//GUILayout.Space(4.0f);
			var prevSimulate = simulate;
			simulate = GUILayout.Toggle(simulate, "Toggle", EditorStyles.toolbarButton, GUILayout.MaxWidth(100.0f));

			if (prevSimulate != simulate && simulate)
			{
				createMode = false;
			}

			if (!Application.isPlaying)
			{
				var prevCreateMode = createMode;
				createMode = GUILayout.Toggle(createMode, "Create", EditorStyles.toolbarButton, GUILayout.MaxWidth(100.0f));
				if (createMode != prevCreateMode && createMode)
				{
					simulate = false;
				}
			}
			//GUILayout.Space(4.0f);

			//showIcons = GUILayout.Toggle(showIcons, "Icons", EditorStyles.toolbarButton, GUILayout.MaxWidth(100.0f));

			var prevShowType = showType;
			showType = GUILayout.Toggle(showType, "Type", EditorStyles.toolbarButton, GUILayout.MaxWidth(100.0f));
			if (prevShowType != showType && showType)
			{
				showControllers = false;
				showTexts = false;
			}

			var prevShowController = showControllers;

			showControllers = GUILayout.Toggle(showControllers, "Controllers", EditorStyles.toolbarButton, GUILayout.MaxWidth(100.0f));

			if (prevShowController != showControllers && showControllers)
			{
				showType = false;
				showTexts = false;
			}

			var prevShowTexts = showTexts;

			showTexts = GUILayout.Toggle(showTexts, "Hints", EditorStyles.toolbarButton, GUILayout.MaxWidth(100.0f));

			if (prevShowTexts != showTexts && showTexts)
			{
				showType = false;
				showControllers = false;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void DrawSecondaryToolbar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			var selectedPanel = WooPanelsEngine.Instance.GetSelectedPanel(Selection.activeTransform);

			var buttonText =  "Open (Shift+W)";
			var buttonEnabled = false;

			if (selectedPanel != null)
			{
				if (selectedPanel.IsOpened() || selectedPanel.IsOpening())
				{
					buttonText = "Close (Shift+W)";
					buttonEnabled = true;
				}
				else
				{
				
					if (WooPanelsEngine.Instance.CanBeOpened(selectedPanel))
					{
						buttonEnabled = true;
					}
					else
					{
						buttonEnabled = false;
					}
				}
			}

			if (!buttonEnabled)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button(buttonText, EditorStyles.toolbarButton))
			{
				MenuItemsExtention.TriggerSelectedPanel();
			}
			if (!buttonEnabled)
			{
				GUI.enabled = true;
			}

			var closeAllEnabled = panelNodes.Count(panel => !panel.Value.IsClosed) > 0;

			if (!closeAllEnabled)
			{
				GUI.enabled = false;
			}
			#if UNITY_EDITOR_OSX
			if (GUILayout.Button("Close All (Cmd+Shift+W)", EditorStyles.toolbarButton))
			#else 
			if (GUILayout.Button("Close All (Ctrl+Shift+W)", EditorStyles.toolbarButton))
			#endif
			{
				MenuItemsExtention.CloseAllPanels();
			}
			if (!closeAllEnabled)
			{
				GUI.enabled = true;
			}

			GUILayout.FlexibleSpace();
			if (!Application.isPlaying)
			{
				if (panelNodes.Count(panel => panel.Value.Panel != null && !panel.Value.Panel.gameObject.activeInHierarchy) > 0)
				{
					if (GUILayout.Button("Enable All", EditorStyles.toolbarButton))
					{
						WooPanelsEngine.Instance.EnableAll();
					}
				}
				if (panelNodes.Count(panel => panel.Value.Panel != null && panel.Value.Panel.gameObject.activeInHierarchy) == panelNodes.Count)
				{
					if (GUILayout.Button("Disable All", EditorStyles.toolbarButton))
					{
						WooPanelsEngine.Instance.DisableAll();
					}
				}

			}

			GUILayout.EndHorizontal();
		}

		private void DrawCanvasesPopup()
		{
			List<GUIContent> options = new List<GUIContent>();

			options.Add(new GUIContent("All"));
			var appendix = "";
			foreach (var root in allRootNodes)
			{
				if (root.Key != null)
				{
					options.Add(new GUIContent(root.Key.name + appendix));
				}
				appendix += "\t";
			}

			selectedRoot = EditorGUILayout.Popup(selectedRoot, options.ToArray(), GUILayout.Width(100.0f)); 
		}

		private void DrawPanels()
		{
			currentScrollPosition = EditorGUILayout.BeginScrollView(currentScrollPosition);

			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Your panels hierarchy (" + panelNodes.Count + " panels total):");

			GUILayout.FlexibleSpace();

			if (showIcons)
			{
				DrawIconMeanings();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Label("Looking at: ");
			DrawCanvasesPopup();

			selectedPanel = WooPanelsEngine.Instance.GetSelectedPanel(Selection.activeTransform);

			if (allRootNodes != null)
			{
				if (selectedRoot == 0)
				{
					foreach (var rootNodes in allRootNodes)
					{
						DrawRootNodes(rootNodes);
					}
				}
				else
				{
					if (allRootNodes.Count >= selectedRoot)
					{
						DrawRootNodes(allRootNodes.ElementAt(selectedRoot - 1));
					}
				}
			}

			if (allRootNodes == null || allRootNodes.Count == 0)
			{
				DrawCanvasName("Empty");
				DrawCreatePanelButton("Create First Panel", 0, null, true);
			}

			if (createMode && !simulate && !Application.isPlaying && panelNodes.Count > 0)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Create Canvas", GUILayout.Width(150.0f), GUILayout.Height(30.0f)))
				{
					CreateNewCanvas();
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.Space(20.0f);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}

		private void CreateNewCanvas()
		{
			MenuItemsExtention.CreatePanel(null);

			WooPanelsEngine.Instance.Rebuild();
			selectedRoot = allRootNodes.Count;
		}

		private void DrawRootNodes(KeyValuePair<Transform, List<WooPanelNode>> rootNodes)
		{
			if (rootNodes.Key == null)
			{
				return;
			}
			var rect = DrawCanvasName(rootNodes.Key);
			GUILayout.BeginVertical();
			GUILayout.Space(10.0f);
			for (int i = 0; i < rootNodes.Value.Count; i++)
			{
				DrawNode(rootNodes.Value[i].Panel, 0);
			}
			GUILayout.EndVertical();
			DrawLines(rootNodes.Value);
			if (rootNodes.Value.Count > 0)
			{
				DrawLinesToCanvas(rect, rootNodes.Value[0].rect);
			}

			GUILayout.Space(20.0f);
		}

		private void DrawCanvasName(string name)
		{
			var prevFontSize = GUI.skin.label.fontSize;
			GUI.skin.label.fontSize = 15;
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(name);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUI.skin.label.fontSize = prevFontSize;
		}

		private void DrawLinesToCanvas(Rect canvasNameRect, Rect panelRect)
		{
			var col = new Color(0.3f, 0.3f, 0.3f, 0.8f);

			var targetRect = panelRect;
			var canvasBottomCenter = new Vector2(canvasNameRect.x + canvasNameRect.width / 2.0f, canvasNameRect.position.y + canvasNameRect.height);
			var distanceY = panelRect.position.y - canvasBottomCenter.y;
			var horizontalLinePosition = new Vector2(panelRect.center.x, canvasBottomCenter.y + (distanceY / 2.0f));
			targetRect.position = horizontalLinePosition;
			targetRect.height = 2.0f;
			targetRect.width = canvasNameRect.center.x - panelRect.center.x;

			DrawVerticalLine(col, canvasBottomCenter, distanceY / 2.0f + targetRect.height);
			DrawVerticalLine(col, targetRect.position, distanceY / 2.0f);
			Handles.BeginGUI();
			Handles.DrawSolidRectangleWithOutline(targetRect, col, col);
			Handles.EndGUI();
		}
		private Rect DrawCanvasName(Transform transform)
		{
			var prevFontSize = GUI.skin.label.fontSize;


			var prevCol = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
			GUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();
			if (GUILayout.Button(transform.name, GUILayout.Width(150.0f), GUILayout.Height(30.0f)))
			{
				Selection.activeGameObject = transform.gameObject;
			}
			var rect = GUILayoutUtility.GetLastRect();
			if (Selection.activeTransform == transform)
			{
				DrawSelectedIndicator(rect);
			}

			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();
			GUI.skin.label.fontSize = prevFontSize;
			GUI.backgroundColor = prevCol;

			return rect;
		}

		private void DrawCreatePanelButton(string buttonName, int level, Transform parent, bool drawInSimulate = false)
		{
			if ((simulate || !createMode) && !drawInSimulate)
			{
				return;
			}

			var prevBackgroundColor = GUI.backgroundColor;
			GUILayout.Space(2.0f);
			GUILayout.BeginHorizontal();

			GUILayout.Space(45.0f + level * 30.0f);

			if (GUILayout.Button(buttonName, GUILayout.Width(150), GUILayout.Height(30)))
			{	
				var panel = MenuItemsExtention.CreatePanel(parent);
				WooPanelsEngine.Instance.Rebuild();
				WooPanelsEngine.Instance.Open(panel, true, false);
			}

			GUILayout.EndHorizontal();
		}

		private void DrawIconMeanings()
		{
			var heightToReserve = GUILayoutUtility.GetLastRect().height;
			var textRect = GUILayoutUtility.GetRect(70.0f, heightToReserve + 17);
			var iconRect = GUILayoutUtility.GetRect(20.0f, heightToReserve + 17);
			textRect.height = 30.0f;
			iconRect.height = 30.0f;
			textRect.position -= Vector2.down * (10.0f);
			iconRect.position -= Vector2.down * (10.0f);

			iconRect.position += Vector2.down * (8.0f);

			GUI.Label(textRect, "  On Awake:");
			GUI.DrawTexture(iconRect, openOnAwakeTexture);

			iconRect.position -= Vector2.down * (iconRect.height - 5.0f);
			textRect.position -= Vector2.down * (textRect.height - 3.0f);

			GUI.Label(textRect, "  With Child:");
			GUI.DrawTexture(iconRect, openWithChildTexture);
			iconRect.position -= Vector2.down * (iconRect.height + 3.0f);
			textRect.position -= Vector2.down * (textRect.height - 3.0f);

			GUI.Label(textRect, "With Parent:");
			GUI.DrawTexture(iconRect, openWithParentTexture);

			GUILayout.Space(10.0f);
		}

		private void RestartSimulationOrEnableAll()
		{
			WooPanelsEngine.Instance.CloseAll(false);
			WooPanelsEngine.Instance.OpenAllOnAwake(!editAnimators, false);
		}

		public void OnGUI()
		{
			if (!WooPanelsEngine.Instance.isBuilt)
			{
				WooPanelsEngine.Instance.Rebuild();
				RestartSimulationOrEnableAll();
				GUILayout.Label("Playing..");
				return;
			}

			if (prevIsPlaying != Application.isPlaying && Event.current.type == EventType.Repaint)
			{
				prevIsPlaying = Application.isPlaying;
				return;
			}

			DrawGrid();
			DrawToolbar();
			DrawSecondaryToolbar();
			DrawPanels();
			ProcessSelection();
		}

		void ProcessSelection()
		{
			if (Event.current.type == EventType.MouseUp && mouseOverWindow)
			{
				Selection.activeGameObject = null;
			}
		}

		void DrawGrid()
		{
			if (Event.current.type != EventType.Repaint)
				return;

			//GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), tex , ScaleMode.StretchToFill);

			if (lineMaterial == null)
			{
				CreateLineMaterial();
			}

			lineMaterial.SetPass(0);

			GL.PushMatrix();
			GL.Begin(GL.LINES);

			DrawGridLines(10.0f, kGridMinorColorDark);
			DrawGridLines(50.0f, kGridMajorColorDark);

			GL.End();
			GL.PopMatrix();
		}

		bool LerpColors()
		{
			bool lerp = false;
			foreach (var panel in panelNodes)
			{
				if (panel.Value.LerpColor())
				{
					lerp = true;
				}
			}

			return lerp;
		}

		bool saveEditAnimators = false;

		void Update()
		{
			if (editAnimators)
			{
				if (!Application.isPlaying)
				{
					WooPanelsEngine.Instance.Animate();
				}
				SceneView.RepaintAll();
			}

			if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Animation")
			{
				SaveAnimationMode();
			}

			LerpColors();

			Repaint();
		}
	}
}