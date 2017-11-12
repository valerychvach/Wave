using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using Wooplex.Panels;
using UnityEngine.UI;
using System.Linq;

namespace Wooplex.Panels
{
	public class MenuItemsExtention 
	{
		#region Unity Methods
		#endregion

		#region Public Methods

		#endregion

		#region Private Methods

		[MenuItem("GameObject/TriggerCloseAllPanels %#w")]
		public static void CloseAllPanels()
		{
			WooPanelsEngine.Instance.CloseAll(false);
		}

		[MenuItem("GameObject/TriggerWooPanel %w")]
		public static void TriggerSelectedPanel()
		{
			var panel = WooPanelsEngine.Instance.GetSelectedPanel(Selection.activeTransform);

			if (panel != null)
			{
				if (PanelManagerWindow.Instance != null && PanelManagerWindow.Instance.editAnimators)
				{
					WooPanelsEngine.Instance.Toggle(panel, false, Application.isPlaying);
				}
				else
				{
					WooPanelsEngine.Instance.Toggle(panel, true, Application.isPlaying);
				}
			}
		}

		[ MenuItem( "GameObject/Create Other/WooPanel" ) ]
		internal static void CreatePanel()
		{
			CreatePanel(Selection.activeTransform);
		}

		internal static WooPanel CreatePanel(Transform parentTransform = null)
		{
			var go = new GameObject();
			var selectedGo = Selection.activeGameObject as GameObject;

			if (selectedGo != null)
			{
				go.transform.SetParent(selectedGo.transform);
			}

			go.AddComponent<WooPanel>();
			go.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("WooPanels/Animators/Default Panel Controller");

			var panelRectTransform = go.AddComponent<RectTransform>();


			go.name = "New Panel";
			go.transform.SetParent(parentTransform);
			Selection.activeGameObject = go;

			var contentGo = new GameObject();
			contentGo.name = "Content";
			var contentRectTransform = contentGo.AddComponent<RectTransform>();

			contentGo.transform.SetParent(go.transform);

			if (go.GetComponentInParent<Canvas>() == null)
			{
				GameObject canvasGo = null;

				if (canvasGo == null)
				{
					canvasGo = new GameObject();
					canvasGo.AddComponent<Canvas>();
					canvasGo.transform.SetParent(go.transform.parent);
					canvasGo.name = "Canvas";
					canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
					canvasGo.AddComponent<CanvasScaler>();
				}

				go.transform.SetParent(canvasGo.transform);
			}

			SetDefaultRectForPanel(panelRectTransform);
			SetDefaultRectForPanel(contentRectTransform);

			if (GameObject.FindObjectOfType<PanelManager>() == null)
			{
				var panelManager = new GameObject();
				panelManager.AddComponent<PanelManager>();
				panelManager.name = "Panel Manager";
			}

			return go.GetComponent<WooPanel>();
		}

		private static void SetDefaultRectForPanel(RectTransform rectTransform)
		{
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(1, 1);
			rectTransform.sizeDelta = new Vector2(0.0f, 0.0f);

			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.localScale = Vector3.one;
			rectTransform.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		}
		#endregion
	}
}
