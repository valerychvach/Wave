// Version 1.0.0

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Wooplex.Panels
{
	public class PanelManager : MonoBehaviour
	{
		private static PanelManager instance;

		protected static PanelManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<PanelManager>();

					if (FindObjectsOfType<PanelManager>().Length > 1)
					{
						Debug.LogError("There should never be more than 1 Panel Manager!");

						return instance;
					}
				}

				return instance;
			}
		}

		[HideInInspector]
		public Animator OpenedPanelAnimator;
		[HideInInspector]
		public Animator OpenedPopupAnimator;

		private WooPanel previousPanel;

		private WooPanel[] panels;

		private const string BASE_UPDATE = "BaseUpdate";

		protected PanelManager()
		{
		}
		// guarantee this will be always a singleton only - can't use the constructor!

		private void Awake()
		{
			WooPanelsEngine.Instance.Rebuild();
			WooPanelsEngine.Instance.CloseAll(false);
			WooPanelsEngine.Instance.OpenAllOnAwake();
		}

		private void OnDisable()
		{
			WooPanelsEngine.Instance.OnDisable();
			#if UNITY_EDITOR
			UnityEditor.AnimationMode.StopAnimationMode();
			#endif
		}

		public static T GetPanelInChildren<T>(Transform transform) where T: WooPanel
		{
			var panels = GetPanels<T>();

			foreach (var panel in panels)
			{
				if (HasParent(panel.transform, transform))
				{
					return panel;
				}
			}

			return null;
		}

		public static T[] GetPanelsInChildren<T>(Transform transform) where T: WooPanel
		{
			var panels = GetPanels<T>();
			List<T> panelsInChildren = new List<T>();
			foreach (var panel in panels)
			{
				if (HasParent(panel.transform, transform))
				{
					panelsInChildren.Add(panel);
				}
			}

			return panelsInChildren.ToArray();
		}

		private static bool HasParent(Transform child, Transform parent)
		{
			if (child == parent)
			{
				return true;
			}

			var parentTransform = child.parent;

			while (parentTransform != null)
			{
				return HasParent(parentTransform, parent);
			}

			return false;
		}

		public static T[] GetPanels<T>() where T : WooPanel
		{
			List<T> panels = new List<T>();

			foreach (var panel in WooPanelsEngine.Instance.panelNodes)
			{
				if (panel.Value.Panel is T)
				{
					panels.Add(panel.Value.Panel as T);
				}
			}

			return panels.ToArray();
		}

		public static T GetPanel<T>() where T : WooPanel
		{
			foreach (var panel in WooPanelsEngine.Instance.panelNodes)
			{
				if (panel.Value.Panel is T)
				{
					return panel.Value.Panel as T;
				}
			}
			return null;
		}

		public static T Open<T>() where T : WooPanel
		{
			return Instance.OpenPanel<T>();
		}

		public static WooPanel Open(WooPanel panel)
		{
			return Instance.OpenPanel(panel);
		}

		public static T Close<T>() where T : WooPanel
		{
			return Instance.ClosePanel<T>();
		}

		public static WooPanel Close(WooPanel panel)
		{
			return Instance.ClosePanel(panel);
		}

		public static T Toggle<T>() where T : WooPanel
		{
			return Instance.TogglePanel<T>();
		}

		public static WooPanel Toggle(WooPanel panel)
		{
			return Instance.TogglePanel(panel);
		}

		public static bool IsOpen<T>() where T : WooPanel
		{
			var panel = GetPanel<T>();

			return panel != null && WooPanelsEngine.Instance.IsOpen(panel);
		}

		public WooPanel TogglePanel(WooPanel panel)
		{
			panel.Toggle();

			return panel;
		}

		public T TogglePanel<T>() where T : WooPanel
		{
			var panel = GetPanel<T>();

			if (panel != null)
			{
				panel.Toggle();
			}

			return panel;
		}

		public WooPanel OpenPanel(WooPanel panel)
		{
			WooPanelsEngine.Instance.Open(panel);

			return panel;
		}

		public T OpenPanel<T>() where T : WooPanel
		{
			var panel = GetPanel<T>();

			if (panel != null)
			{
				WooPanelsEngine.Instance.Open(panel);
			}

			return panel;
		}

		public WooPanel ClosePanel(WooPanel panel)
		{
			WooPanelsEngine.Instance.Close(panel);

			return panel;
		}

		public T ClosePanel<T>() where T : WooPanel
		{
			var panel = GetPanel<T>();

			if (panel != null)
			{
				WooPanelsEngine.Instance.Close(panel);
			}

			return panel;
		}

		void Update()
		{
			WooPanelsEngine.Instance.Animate();

			int counter = 0;
			foreach (var panel in WooPanelsEngine.Instance.panelNodes)
			{
				if (panel.Value.Panel == null)
				{
					counter++;
				}
				else
				{
					panel.Value.Panel.SendMessage(BASE_UPDATE, SendMessageOptions.DontRequireReceiver);
				}
			}

			if (counter > 0)
			{
				WooPanelsEngine.Instance.Rebuild();
			}
		}

	}
}
