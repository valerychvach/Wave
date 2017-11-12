using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wooplex.Panels
{
	public class WooPanelsEngine
	{
		private static WooPanelsEngine instance;

		public static WooPanelsEngine Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new WooPanelsEngine();
				}

				instance.TryToRebuildIfNecessary();

				return instance;
			}
		}

		public Dictionary<WooPanel, WooPanelNode> panelNodes = new Dictionary<WooPanel, WooPanelNode>();
		public Dictionary<Transform, int> rootsHierarchyIndeces;
		public Dictionary<Transform, List<WooPanelNode>> allRootNodes;

		public bool isBuilt = false;

		private const string DEFAULT_ANIMATOR_NAME = "WooPanels/Animators/Default Panel Controller";

		private WooPanelsEngine()
		{
			#if UNITY_EDITOR
			EditorApplication.playmodeStateChanged += OnPlaymodeChanged;
			#endif
		}

		private void OnPlaymodeChanged()
		{
			if (!Application.isPlaying)
			{
				Instance.Rebuild();
				Instance.CloseAll(false);
				Instance.OpenAllOnAwake(true);
			}
		}

		List<T> GetAllOnScene<T>() where T : Component
		{
			List<T> objectsInScene = new List<T>();

			foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
			{
				var wooPanel = go.GetComponent<T>();
				if (go.scene != SceneManager.GetActiveScene())
				{
					continue;
				}
				if (wooPanel != null)
				{
					objectsInScene.Add(wooPanel);
				}
			}

			return objectsInScene;
		}

		public WooPanelNode GetOrCreatePanelNode(WooPanel panel)
		{
			WooPanelNode panelNode;

			if (!panelNodes.ContainsKey(panel))
			{
				panelNode = new WooPanelNode();
				panelNode.Panel = panel;
				panelNodes.Add(panel, panelNode);
			}
			else
			{
				panelNode = panelNodes[panel];
				panelNode.Panel = panel;
			}

			return panelNode;
		}

		private void InitializePanels()
		{
			var panelsList = GetAllOnScene<WooPanel>();

			var defaultAnimator = Resources.Load(DEFAULT_ANIMATOR_NAME) as RuntimeAnimatorController;

			foreach (var panel in panelsList)
			{
				if (panel.GetComponent<Animator>().runtimeAnimatorController == null)
				{
					panel.GetComponent<Animator>().runtimeAnimatorController = defaultAnimator;
				}
			}
		}

		public void OnDisable()
		{
			isBuilt = false;
		}

		public void TryToRebuildIfNecessary()
		{
			if (!isBuilt)
			{
				Rebuild();
			}
			else
			{
				foreach (var panel in panelNodes)
				{
					if (panel.Value.Panel == null || panel.Value.Panel.gameObject == null)
					{
						isBuilt = false;
					}
				}
				if (!isBuilt)
				{
					Rebuild();
				}
			}
		}

		public void Rebuild()
		{
//			if (isBuilt && !Application.isPlaying)
//			{
//				return;
//			}

			InitializePanels();
			prevTime = DateTime.Now;

			isBuilt = true;

			allRootNodes = new Dictionary<Transform, List<WooPanelNode>>();
			rootsHierarchyIndeces = new Dictionary<Transform, int>();

			var panels = GetAllOnScene<WooPanel>();

			foreach (var key in panelNodes.Keys)
			{
				panelNodes[key].Panel = null;
				panelNodes[key].Children = new List<WooPanelNode>();
			}

			for (int i = 0; i < panels.Count; i++)
			{
				WooPanelNode panelNode = GetOrCreatePanelNode(panels[i]);

				WooPanel parentPanel = GetParentPanel(panels[i]);

				if (parentPanel != null)
				{
					WooPanelNode parentNode = GetOrCreatePanelNode(parentPanel);
					panelNode.Parent = parentNode;
					parentNode.Children.Add(panelNode);
					panelNode.ParentTransform = parentNode.Panel.transform;
				}
				else
				{
					var parentTransform = panels[i].transform.parent;

					if (parentTransform != null)
					{
						if (allRootNodes.ContainsKey(parentTransform))
						{
							allRootNodes[parentTransform].Add(panelNode);
						}
						else
						{
							List <WooPanelNode> rootNodes = new List <WooPanelNode>();
							rootNodes.Add(panelNode);

							allRootNodes.Add(parentTransform, rootNodes);
						}
					}

					panelNode.ParentTransform = parentTransform;
				}

			}


			RemoveNullPanels();
			SortNodes();
			if (Application.isPlaying)
			{
				EnableAll();
				InitPanels();
			}
		}

		public void InitPanels()
		{
			foreach (var node in panelNodes)
			{
				node.Value.Panel.Init();
			}
		}

		public void RemoveNullPanels()
		{
			List<WooPanel> toRemove = new List<WooPanel>();
			foreach (var key in panelNodes.Keys)
			{
				if (panelNodes[key].Panel == null)
				{
					toRemove.Add(key);
				}
			} 
			foreach (var key in toRemove)
			{
				panelNodes.Remove(key);
			}

		}

		public void SortNodes()
		{
			var currentIndex = 0;
			List<Transform> rootTransforms = new List<Transform>();

			foreach (Transform obj in GetAllOnScene<Transform>())
			{
				if (obj.parent == null)
				{
					rootTransforms.Add(obj);
				}
			}

			rootTransforms.Sort((x, y) => x.GetSiblingIndex().CompareTo(y.GetSiblingIndex()));

			foreach (Transform rootTransform in rootTransforms)
			{
				currentIndex = Traverse(rootTransform, currentIndex);
			}

			foreach (var rootNode in allRootNodes)
			{
				rootNode.Value.Sort((x, y) => x.HierarchyIndex.CompareTo(y.HierarchyIndex));
				for (int i = 0; i < rootNode.Value.Count; i++)
				{
					SortChildren(rootNode.Value[i]);
				}
			}

			var list = allRootNodes.Keys.ToList();
			list.Sort((x, y) => rootsHierarchyIndeces[x].CompareTo(rootsHierarchyIndeces[y]));

			var sortedDictionary = new Dictionary<Transform, List <WooPanelNode>>();

			foreach (var root in list)
			{
				sortedDictionary.Add(root, allRootNodes[root]);
			}

			allRootNodes = sortedDictionary;
		}

		int Traverse(Transform transformToTraverse, int index)
		{
			index++;

			var panel = transformToTraverse.GetComponent<WooPanel>();
			if (panel != null)
			{
				panelNodes[panel].HierarchyIndex = index;
			}

			if (allRootNodes.ContainsKey(transformToTraverse))
			{
				rootsHierarchyIndeces[transformToTraverse] = index;
			}

			foreach (Transform child in transformToTraverse)
			{
				index = Traverse(child, index);
			}

			return index;
		}

		public void SortChildren(WooPanelNode panel)
		{
			panel.Children.Sort((x, y) => x.HierarchyIndex.CompareTo(y.HierarchyIndex));	

			for (int i = 0; i < panel.Children.Count; i++)
			{
				SortChildren(panel.Children[i]);
			}
		}

		public WooPanel GetParentPanel(WooPanel panel)
		{
			var cursor = panel.transform.parent;

			while (cursor != null)
			{
				var result = cursor.GetComponent<WooPanel>();
				if (result != null)
				{
					return result; 
				}
				cursor = cursor.parent;
			}

			return null;
		}

		public WooPanel GetSelectedPanel(Transform transform)
		{
			var cur = transform;
			WooPanel result = null;

			while (cur != null)
			{
				result = cur.GetComponent<WooPanel>();
				if (result != null)
				{
					return result;
				}
				cur = cur.parent;
			}

			return result;
		}

		public Transform GetRoot(WooPanel panel)
		{
			var underRootPanel = GetParent(panel);
			var underRootTransform = panel.transform;

			if (underRootPanel != null && underRootPanel.Panel != null)
			{
				underRootTransform = underRootPanel.Panel.transform;
			}

			return underRootTransform.parent;
		}

		public List<WooPanelNode> GetRootNodes(WooPanel panel)
		{
			return allRootNodes[GetRoot(panel)];
		}

		public WooPanelNode GetParent(WooPanel panel)
		{
			return panelNodes.ContainsKey(panel) ? panelNodes[panel].Parent as WooPanelNode : null;
		}

		public List<WooPanelNode> GetChildren(WooPanel panel)
		{
			return panelNodes.ContainsKey(panel) ? panelNodes[panel].Children : null;
		}

		public float TimeToOpen(WooPanel panel)
		{
			float timeToOpen = 0.0f;
			var parent = GetParent(panel) != null ? GetParent(panel).Panel : null;

			if (parent != null)
			{
				timeToOpen += TimeToStartOpening(parent) + panelNodes[parent].TimeToOpen() * panel.PanelProperties.ParentToOpen;
			}

			timeToOpen += panelNodes[panel].TimeToOpen() + TimeForSameLevelToClose(panel) * panel.PanelProperties.SiblingsToClose;

			return timeToOpen;
		}

		public float TimeToStartOpening(WooPanel panel)
		{
			if (panel == null)
			{
				return 0.0f;
			}

			return TimeToOpenParents(panel) * panel.PanelProperties.ParentToOpen + TimeForSameLevelToClose(panel) * panel.PanelProperties.SiblingsToClose;
		}

		public float TimeToOpenParents(WooPanel panel)
		{
			float timeForParentsToOpen = 0.0f;

			var parent = GetParent(panel);

			if (parent != null)
			{
				timeForParentsToOpen += TimeToOpen(parent.Panel);
			}

			return timeForParentsToOpen;
		}

		public float TimeForSameLevelToClose(WooPanel panel)
		{
			var timeToClose = 0.0f;
			if (panel.PanelType == PanelType.Independent)
			{
				return timeToClose;
			}

			var parent = GetParent(panel);


			if (parent != null)
			{
				var children = GetChildren(parent.Panel);

				for (int i = 0; i < children.Count; i++)
				{
					if (panel != children[i].Panel && children[i].Panel.PanelType != PanelType.Independent)
					{
						timeToClose += TimeToClose(children[i].Panel);
					}
				}
			}
			else
			{
				var rootNodes = GetRootNodes(panel);

				for (int i = 0; i < rootNodes.Count; i++)
				{
					if (rootNodes[i].Panel != panel && rootNodes[i].Panel.PanelType != PanelType.Independent)
					{
						timeToClose += TimeToClose(rootNodes[i].Panel);
					}
				}
			}

			return timeToClose;
		}

		public float TimeToClose(WooPanel panel)
		{
			float timeToClose = 0.0f;

			timeToClose += TimeForChildrenToClose(panel) * panel.PanelProperties.ChildrenToClose;
			timeToClose += panelNodes[panel].TimeToClose();

			return timeToClose;
		}

		public float TimeForChildrenToClose(WooPanel panel)
		{
			float timeForChildrenToClose = 0.0f;

			var children = GetChildren(panel);
			for (int i = 0; i < children.Count; i++)
			{
				timeForChildrenToClose += TimeToClose(children[i].Panel);
			}

			timeForChildrenToClose *= panel.PanelProperties.ChildrenToClose;

			return timeForChildrenToClose;
		}

		public void TryOpeningParents(WooPanel panel, bool immediate = false, bool notify = true, WooPanel target = null)
		{
			var parent = GetParent(panel);

			if (parent != null)
			{
				if (parent.Panel.PanelProperties.WithChild)
				{
					Open(parent.Panel, immediate, notify, target);
				}
			}
		}

		private bool HasInTree(WooPanel panel, WooPanel child)
		{
			var children = GetChildren(panel);

			for (int i = 0; i < children.Count; i++)
			{
				if (HasInTree(children[i].Panel, child))
				{
					return true;
				}
			}

			if (panel == child)
			{
				return true;
			}

			return false;
		}

		public void TryOpeningChildren(WooPanel panel, bool immediate = false, bool notify = false, WooPanel target = null)
		{
			var children = GetChildren(panel);

			bool foundDependent = false;

			for (int i = 0; i < children.Count; i++)
			{
				if (HasInTree(children[i].Panel, target))
				{
					Open(children[i].Panel, immediate, notify, target);
					if (children[i].Panel.PanelType == PanelType.Dependent)
					{
						foundDependent = true;
					}
				}
			}

			for (int i = 0; i < children.Count; i++)
			{
				if (foundDependent && children[i].Panel.PanelType == PanelType.Dependent)
				{
					continue;
				}

				if (children[i].Panel.PanelProperties.WithParent)
				{
					Open(children[i].Panel, immediate, notify, target);
				}
			}
		}

		public void CloseSameLevelPanels(WooPanel panel, bool immediate = false, bool notify = true)
		{
			if (panel.PanelType == PanelType.Independent)
			{
				return;
			}

			var parent = GetParent(panel);

			if (parent != null)
			{
				var children = GetChildren(parent.Panel);

				for (int i = 0; i < children.Count; i++)
				{
					if (panel != children[i].Panel && children[i].Panel.PanelType != PanelType.Independent)
					{
						Close(children[i].Panel, immediate, notify);
					}
				}
			}
			else
			{
				var rootNodes = GetRootNodes(panel);

				for (int i = 0; i < rootNodes.Count; i++)
				{
					if (rootNodes[i].Panel != panel && rootNodes[i].Panel.PanelType != PanelType.Independent)
					{
						Close(rootNodes[i].Panel, immediate, notify);
					}
				}
			}
		}

		public void PlayOpenAnimationImmdiately(WooPanel panel, bool notify = true)
		{
			panel.gameObject.SetActive(true);

			panelNodes[panel].StopCoroutines();

			panel.PanelState = PanelState.IsOpening;
			if (notify)
			{
				panel.NotifyOpeningBegin();
			}

			panel.PanelState = PanelState.Opened;

			if (notify)
			{
				panel.NotifyOpeningEnd();
			}
			ProcessAlwaysOnTop(panel);

			panel.PanelState = PanelState.IsOpening;
			RewindToEndPanelAnimation(panel);
			SamplePanelAnimator(panel);
			panel.PanelState = PanelState.Opened;
		}

		public void PlayCloseAnimationImmdiately(WooPanel panel, bool notify = true)
		{
			panel.gameObject.SetActive(true);

			panelNodes[panel].StopCoroutines();
			panel.PanelState = PanelState.IsClosing;
			if (notify)
			{
				panel.NotifyClosingBegin();
			}

			panel.PanelState = PanelState.Closed;

			if (notify)
			{
				panel.NotifyClosingEnd();
			}

			panel.PanelState = PanelState.IsClosing;
			RewindToEndPanelAnimation(panel);
			SamplePanelAnimator(panel);
			panel.PanelState = PanelState.Closed;

			panel.gameObject.SetActive(panel.PanelProperties.ActiveWhenClosed);
		}

		public void Open(WooPanel panel, bool instant = false, bool notify = true, WooPanel target = null)
		{
			if (target == null)
			{
				target = panel;
			}

			if (!instant)
			{
				OpenAnimated(panel, notify, target);
			}
			else
			{
				OpenNotAnimated(panel, notify, target);
			}
		}

		public void RewindPanelAnimation(WooPanel panel)
		{
//			if (panelNodes[panel].NotDefined)
//			{
//				panelNodes[panel].simulatedTime = panel.PanelProperties.SkipFirstAnimation ? 1.0f : 0.0f;
//			}
//			else
//			{
				panelNodes[panel].simulatedTime = 1.0f - Mathf.Min(panelNodes[panel].simulatedTime, 1.0f);
//			}
		}

		public void RewindToEndPanelAnimation(WooPanel panel)
		{
			panelNodes[panel].simulatedTime = 1.0f;
		}

		public void OpenNotAnimated(WooPanel panel, bool notify = true, WooPanel target = null)
		{
			if (IsOpen(panel) || !CanBeOpened(panel))
			{
				return;
			}
			if (panel == null)
			{
				return;
			}
			RebuildIfPanelGameObjectIsNull(panel);
			panel.gameObject.SetActive(true);

			ProcessAlwaysOnTop(panel);
			TryOpeningParents(panel, true, notify, target);
			CloseSameLevelPanels(panel, true, notify);

			panelNodes[panel].IsOpening = true;
			RewindToEndPanelAnimation(panel);
			SamplePanelAnimator(panel);

			panelNodes[panel].IsOpen = true;

			if (notify)
			{
				panel.NotifyOpeningBegin();
			}

			TryOpeningChildren(panel, true, target);

			if (notify)
			{
				panel.NotifyOpeningEnd();
			}


		}

		public bool CanBeOpened(WooPanel panel)
		{
			var parent = GetParent(panel);

			if (parent != null)
			{
				if (parent.Panel.PanelProperties.WithChild)
				{
					return CanBeOpened(parent.Panel);
				}
				else
				{
					return parent.Panel.IsOpenedOrOpening() || parent.Panel.PanelState == PanelState.IsWaitingToOpen;
				}
			}
			else
			{
				return true;
			}
		}

		private void MakeTreeActive(WooPanel panel)
		{
			var parent = GetParent(panel);

			panel.gameObject.SetActive(true);

			if (parent != null)
			{
				MakeTreeActive(parent.Panel);
			}
		}

		private void RebuildIfPanelGameObjectIsNull(WooPanel panel)
		{
			if (panel.gameObject == null)
			{
				Rebuild();
			}

			if (panel.gameObject == null)
			{
				return;
			}
		}

		public void OpenAnimated(WooPanel panel, bool notify = true, WooPanel target = null)
		{
			if (IsOpen(panel) || panelNodes[panel].IsOpening || panelNodes[panel].IsWaitingToOpen || !CanBeOpened(panel))
			{
				return;
			}

			RebuildIfPanelGameObjectIsNull(panel);

			MakeTreeActive(panel);

			panelNodes[panel].StopCoroutines();
			panelNodes[panel].IsWaitingToOpen = true;
			RewindPanelAnimation(panel);

			CloseSameLevelPanels(panel, false, notify);
			var parent = GetParent(panel) != null ? GetParent(panel).Panel : null;


			var timeToOpen = panelNodes[panel].TimeToOpen();

			var timeToCloseSameLevel = TimeForSameLevelToClose(panel) * panel.PanelProperties.SiblingsToClose;

			var timeToOpenParents = 0.0f;
			if (parent != null)
			{
				timeToOpenParents = TimeToStartOpening(parent) + panelNodes[parent].TimeToOpen() * panel.PanelProperties.ParentToOpen;
			}
			panelNodes[panel].waitingTime = timeToCloseSameLevel + timeToOpenParents;
			panelNodes[panel].remainingWaitingTime = timeToCloseSameLevel + timeToOpenParents;

			panelNodes[panel].ForceMirrored = true;
			panelNodes[panel].Invoke(timeToCloseSameLevel, () =>
				{
					ProcessAlwaysOnTop(panel);

					TryOpeningParents(panel, false, notify, target);

					panelNodes[panel].Invoke(timeToOpenParents, () =>
						{
							panelNodes[panel].IsOpening = true; 

							if (notify)
							{
								panel.NotifyOpeningBegin();
							}

							TryOpeningChildren(panel, false, notify, target);
						});
				});

			var timeToFinishOpening = timeToCloseSameLevel + timeToOpen + timeToOpenParents;
			//Debug.Log(panel.name + " " + timeToFinishOpening);
			panelNodes[panel].Invoke(timeToFinishOpening, () =>
				{
					panelNodes[panel].IsOpen = true;
					panelNodes[panel].ForceMirrored = false;
					RewindToEndPanelAnimation(panel);

					if (notify)
					{
						panel.NotifyOpeningEnd();
					}
				});
		}

		public void ProcessAlwaysOnTop(WooPanel panel)
		{
			if (panel.PanelProperties.AlwaysOnTop)
			{
				panel.transform.SetAsLastSibling();
			}
		}

		public bool IsWaitingToClose(WooPanel panel)
		{
			return panelNodes[panel].IsWaitingToClose;
		}

		public bool IsOpen(WooPanel panel)
		{
			return panelNodes[panel].IsOpen;
		}

		public bool IsClosed(WooPanel panel)
		{
			return panelNodes[panel].IsClosed;
		}

		public void CloseAll(bool notify = true)
		{
			foreach (var root in allRootNodes)
			{
				foreach(var child in root.Value)
				{
					CloseChildrenAndSelfImmediately(child.Panel, notify);
				}
			}
		}

		private void CloseChildrenAndSelfImmediately(WooPanel panel, bool notify = true)
		{
			var children = GetChildren(panel);
			panel.gameObject.SetActive(true);
			foreach (var child in children)
			{
				CloseChildrenAndSelfImmediately(child.Panel, notify);
			}

			PlayCloseAnimationImmdiately(panel, notify);
		}

		public void EnableAll()
		{
			foreach (var panel in panelNodes)
			{
				if (panel.Value.Panel != null)
				{
					panel.Value.Panel.gameObject.SetActive(true);
				}
			}
		}

		public void DisableAll()
		{
			foreach (var panel in panelNodes)
			{
				if (panel.Value.Panel != null)
				{
					panel.Value.Panel.gameObject.SetActive(false);
				}
			}
		}

		public void OpenAllOnAwake(bool immediate = false, bool notify = true)
		{
//			foreach (var panel in panelNodes)
//			{
//				Close(panel.Value.Panel, true, false);
//			}
//
//			foreach (var panel in panelNodes)
//			{
//				panel.Value.Panel.PanelState = PanelState.NotDefined;
//			}

			foreach (var panel in panelNodes)
			{
				if (panel.Value.Panel.PanelProperties.OnAwake)
				{
					Open(panel.Value.Panel, immediate, notify);
				}
			}
		}

		public void Close(WooPanel panel, bool immediate = false, bool notify = true)
		{
			if (!immediate)
			{
				CloseAnimated(panel, notify);
			}
			else
			{
				CloseNotAnimated(panel, notify);
			}
		}

		public void Toggle(WooPanel panel, bool immediate = false, bool notify = true)
		{
			if (panel.IsOpenedOrOpening())
			{
				Close(panel, immediate, notify);
			}
			else
			{
				Open(panel, immediate, notify);
			}
		}

		public void CloseNotAnimated(WooPanel panel, bool notify = true)
		{
			if (panel.IsClosed())
			{
				return;
			}

			RebuildIfPanelGameObjectIsNull(panel);

			panel.gameObject.SetActive(true);
			panel.PanelState = PanelState.IsClosing;
			var children = GetChildren(panel);
			for (int i = 0; i < children.Count; i++)
			{
				Close(children[i].Panel, true, notify);
			}

			if (notify)
			{
				panel.NotifyClosingBegin();
			}

			CloseImmediately(panel, notify);

			panel.gameObject.SetActive(panel.PanelProperties.ActiveWhenClosed);
		}

		public void CloseAnimated(WooPanel panel, bool notify = true)
		{
			if (IsClosed(panel) || panelNodes[panel].IsClosing || panelNodes[panel].IsWaitingToClose)
			{
				return;
			}

			if (panel == null)
			{
				return;
			}
			RebuildIfPanelGameObjectIsNull(panel);

			panel.gameObject.SetActive(true);
			panelNodes[panel].StopCoroutines();
			panelNodes[panel].IsWaitingToClose = true;
			RewindPanelAnimation(panel);

			var children = GetChildren(panel);

			var timeForChildrenToClose = TimeForChildrenToClose(panel) * panel.PanelProperties.ChildrenToClose;

			for (int i = 0; i < children.Count; i++)
			{
				Close(children[i].Panel);
			}
			panelNodes[panel].waitingTime = timeForChildrenToClose;
			panelNodes[panel].remainingWaitingTime = timeForChildrenToClose;

			panelNodes[panel].Invoke(timeForChildrenToClose, () =>
				{
					panelNodes[panel].IsClosing = true;

					if (notify)
					{
						panel.NotifyClosingBegin();
					}

				});

			panelNodes[panel].Invoke(TimeToClose(panel), () =>
				{
					CloseImmediately(panel, notify);
				});
		}

		public void CloseImmediately(WooPanel panel, bool notify = false)
		{
			var children = GetChildren(panel);

			for (int i = 0; i < children.Count; i++)
			{
				CloseImmediately(children[i].Panel, notify);
			}

			RewindToEndPanelAnimation(panel);
			SamplePanelAnimator(panel);
			panelNodes[panel].IsClosed = true;


			if (notify)
			{
				panel.NotifyClosingEnd();
			}

			panel.gameObject.SetActive(panel.PanelProperties.ActiveWhenClosed);
		}


		private void SamplePanelAnimator(WooPanel panel)
		{
			if (panel == null || panel.GetComponent<Animator>() == null)
			{
				return;
			}

			AnimationClip animation = null;
			var animatorController = panel.GetComponent<Animator>().runtimeAnimatorController;

			animation = panelNodes[panel].GetActiveAnimation();

			var time = panelNodes[panel].simulatedTime;
			time = Mathf.Clamp(time, 0.0f, 1.0f);

			if (panelNodes[panel].IsClosed || panelNodes[panel].IsClosing || panelNodes[panel].IsWaitingToClose)
			{
				if (panelNodes[panel].IsMirrored())
				{
					time = 1.0f - time;
				}
			}

			if (animation != null)
			{
				#if UNITY_EDITOR
				if (AnimationMode.InAnimationMode())
				{
					if (!Application.isPlaying)
					{
						AnimationMode.SampleAnimationClip(panel.gameObject, animation, animation.length * time);
					}
				}
				else
				{
					if (!Application.isPlaying && panel.GetLayerIndex() != -1)
					{
						panel.GetAnimator().Play(animation.name, panel.GetLayerIndex(), time);
						panel.GetAnimator().Update(Time.deltaTime);
						panel.GetAnimator().StopPlayback();
					}
				}
				#endif

				if (Application.isPlaying && panel.GetLayerIndex() != -1)
				{
					panel.GetAnimator().Play(animation.name, panel.GetLayerIndex(), time);
					panel.GetAnimator().Update(Time.deltaTime);
					panel.GetAnimator().StopPlayback();
				}
			}
		}

		public bool RegisterPanel(WooPanel panel)
		{
			if (!panelNodes.ContainsKey(panel))
			{
				Rebuild();
			}

			return false;
		}

		private DateTime prevTime;
		public void Animate()
		{
			foreach (var node in panelNodes)
			{
				float deltaTime = (float) (DateTime.Now.Subtract(prevTime).TotalMilliseconds / 1000.0f);

				if (node.Value.IsOpening || node.Value.IsClosing)
				{
					var mod = node.Value.IsClosing ? node.Value.Panel.PanelProperties.ClosingSpeed : 1.0f;
					mod = node.Value.IsOpening ? node.Value.Panel.PanelProperties.OpeningSpeed : mod;
					var activeAnimation = node.Value.GetActiveAnimation();
					if (activeAnimation != null)
					{
						node.Value.simulatedTime += deltaTime / (node.Value.GetActiveAnimation().length / mod);
					}
				}
				if (node.Value.IsWaiting)
				{
					node.Value.remainingWaitingTime -= deltaTime;
					node.Value.remainingWaitingTime = Mathf.Max(node.Value.remainingWaitingTime, 0.0f);
				}

				SamplePanelAnimator(node.Value.Panel);
			}
			prevTime = DateTime.Now;
		}
	}
}
