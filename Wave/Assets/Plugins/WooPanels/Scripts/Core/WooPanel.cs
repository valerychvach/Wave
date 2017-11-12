using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

using System.Collections;
using UnityEngine.EventSystems;

namespace Wooplex.Panels
{
	public enum PanelType
	{
		Dependent,
		Independent
	}

	public enum PanelState
	{
		NotDefined,
		Opened,
		Closed,
		IsOpening,
		IsClosing,
		IsWaitingToOpen,
		IsWaitingToClose
	}

	[System.Serializable]
	public class PanelProperties
	{
		[Header("When To Open")]
		[Tooltip ("Opens on awake")]
		public bool OnAwake = false;
		[Tooltip ("Opens whenever a parent panel in the hierarchy is opened")]
		public bool WithParent = false;
		[Tooltip ("Opens whenever a child panel in the hierarchy is opened")]
		public bool WithChild = true;

		[Header("Animation")]
//		[Tooltip ("If warmed up first animations won't be played")]
//		public bool SkipFirstAnimation = false;

		[Range (0.01f, 5.0f)]
		[Tooltip ("Animation speed multiplier")]
		public float OpeningSpeed = 1.0f;
		[Range (0.01f, 5.0f)]
		[Tooltip ("Animation speed multiplier")]
		public float ClosingSpeed = 1.0f;

		[Header("Before Opening Wait For")]
		[Range (0.0f, 1.0f)]
		[Tooltip ("Waits for all of the panels on the same level of hierarchy to close. Doesn't wait for Not Dependent Panels.")]
		public float SiblingsToClose = 0.5f;
		[Tooltip ("Waits for the parent panel to open")]
		[Range (0.0f, 1.0f)]
		public float ParentToOpen = 0.5f;

		[Header("Before Closing Wait For")]
		[Range (0.0f, 1.0f)]
		[Tooltip ("Waits for all of the children panels in hierarchy to close.")]
		public float ChildrenToClose = 0.5f;

		[Header("Extra")]
		[Tooltip ("Sets as last sibling if always on top")]
		public bool AlwaysOnTop = false;

		[Tooltip ("Whether the gameobject would remain active when closed. When 'false' the performance is better, because gameobjects are not rendered and updated.")]
		public bool ActiveWhenClosed = false;
	}

	[RequireComponent(typeof(CanvasGroup), typeof(Animator))]
	public class WooPanel : MonoBehaviour
	{
		public PanelType PanelType = PanelType.Dependent;

		public PanelProperties PanelProperties = new PanelProperties();

		public UnityEvent OnPanelOpen;
		public UnityEvent OnPanelClose;
		public UnityEvent OnPanelOpenEnd;
		public UnityEvent OnPanelCloseEnd;

		private const string IS_OPENED = "IsOpened";
		private const string ON_OPEN = "OnOpen";
		private const string ON_CLOSE = "OnClose";
		private const string ON_OPEN_END = "OnOpenEnd";
		private const string ON_CLOSE_END = "OnCloseEnd";
		private const string ON_INIT = "OnInit";
		private const string PANEL_LAYER = "Panel";
		private const string IS_DEFINED = "IsDefined";

		private const string PANEL_UPDATE = "PanelUpdate";
		private const string DEFAULT_ANIMATOR_NAME = "WooPanels/Animators/Default Panel Controller";
		private const string LAYER_NAME = "Panel";
		private const string PANEL_OPENED_STATE = "PanelOpened";
		private const string PANEL_CLOSED_STATE = "PanelClosed";

		private Animator animator;

		internal PanelState PanelState = PanelState.NotDefined;

		private Coroutine waitingForOthersToCloseBeforeClosingCoroutine;
		private Coroutine closingCoroutine;
		private Coroutine waitingForOthersToCloseBeforeOpeningCoroutine;
		private Coroutine openingCoroutine;

		private bool InitCalled = false;

		public void Init()
		{
			if (!InitCalled)
			{
				SafeSendMessage(ON_INIT, SendMessageOptions.DontRequireReceiver);
				InitCalled = true;
			}
		}

		protected void OnEnable()
		{
			if (!Application.isPlaying)
			{
				RegisterIfNecessary();
			}
		}

		protected void Awake()
		{
			RegisterIfNecessary();
		}

		private bool RegisterIfNecessary()
		{
			if (WooPanelsEngine.Instance != null)
			{
				return WooPanelsEngine.Instance.RegisterPanel(this);
			}

			return false;
		}

		public Animator GetAnimator()
		{
			animator = GetComponent<Animator>();

			return animator;
		}

		public void Toggle()
		{
			if (!IsOpenedOrOpening())
			{
				Open();
			}
			else if (!IsClosedOrClosing())
			{
				Close();
			}
		}

		public void Open()
		{
			PanelManager.Open(this);
		}
			
		public void Close()
		{
			PanelManager.Close(this);
		}

		#if UNITY_EDITOR
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

		#endif
		public bool IsMirrored()
		{
			#if UNITY_EDITOR

			if (!Application.isPlaying)
			{
				return GetState(GetLayer(GetAnimator()), PANEL_CLOSED_STATE).speed == -1.0f;
			}

			#endif
			return GetAnimationLength(this.animator, "PanelClosed") == -1.0f;
		}

		private void SafeSendMessage(string message, SendMessageOptions options)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			try
			{
				SendMessage(message, SendMessageOptions.DontRequireReceiver);
			}
			catch
			{

			}
		}

		internal void NotifyClosingBegin()
		{
			SafeSendMessage(ON_CLOSE, SendMessageOptions.DontRequireReceiver);
			OnPanelClose.Invoke();
		}

		internal void NotifyClosingEnd()
		{
			SafeSendMessage(ON_CLOSE_END, SendMessageOptions.DontRequireReceiver);
			OnPanelCloseEnd.Invoke();
		}


		internal void NotifyOpeningBegin()
		{
			SafeSendMessage(ON_OPEN, SendMessageOptions.DontRequireReceiver);
			OnPanelOpen.Invoke();
		}

		internal void NotifyOpeningEnd()
		{
			SafeSendMessage(ON_OPEN_END, SendMessageOptions.DontRequireReceiver);
			OnPanelOpenEnd.Invoke();
		}

		public bool IsOpened()
		{
			return PanelState == PanelState.Opened;
		}

		public bool IsClosed()
		{
			return PanelState == PanelState.Closed;
		}

		public bool IsClosing()
		{
			return PanelState == PanelState.IsClosing;
		}

		public bool IsOpening()
		{
			return PanelState == PanelState.IsOpening;
		}

		internal bool IsNotDefined()
		{
			return PanelState == PanelState.NotDefined;
		}

		internal bool IsClosedOrNotDefined()
		{
			return IsClosed() || IsNotDefined();
		}

		internal bool IsClosedOrClosing()
		{
			return IsClosed() || IsClosing();
		}

		internal bool IsOpenedOrOpening()
		{
			return IsOpened() || IsOpening();
		}

		void BaseUpdate()
		{
			if (IsOpenedOrOpening())
			{
				SendMessage(PANEL_UPDATE, SendMessageOptions.DontRequireReceiver);
			}
		}

		private float GetAnimationLength(Animator anim, string track)
		{
			float length = -1.0f;
			if (anim != null && anim.runtimeAnimatorController != null)
			{
				for (int i = 0; i < anim.runtimeAnimatorController.animationClips.Length; i++)
				{
					var animationClip = anim.runtimeAnimatorController.animationClips[i];
					if (animationClip.name == track)
					{
						length = animationClip.length;
					}
				}
			}
			return length;
		}

		public float GetNormalizedAnimationTime()
		{
			var time = GetCurrentAnimatorStateInfo().normalizedTime;
			time = Mathf.Min(time, 1.0f);
			if (IsMirrored() && IsClosedOrClosing())
			{
				time = 1.0f - time;
			}

			return time;
		}

		public AnimatorStateInfo GetCurrentAnimatorStateInfo()
		{
			AnimatorStateInfo result = new AnimatorStateInfo();;
			if (Application.isPlaying)
			{
				result = GetAnimator().GetCurrentAnimatorStateInfo(GetLayerIndex());
			}

			return result;
		}


		public int GetLayerIndex()
		{
			GetAnimator().logWarnings = false;
			int index = GetAnimator().GetLayerIndex(PANEL_LAYER);
			GetAnimator().logWarnings = true;
			return index;
		}
	}
}
