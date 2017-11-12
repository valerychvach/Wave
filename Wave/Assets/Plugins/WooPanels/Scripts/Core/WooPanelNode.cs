using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Wooplex.Panels
{
	public class WooPanelNode
	{
		public WooPanel Panel;

		public bool IsWaiting
		{
			get
			{
				return IsWaitingToOpen || IsWaitingToClose;
			}
		}

		public bool IsWaitingToOpen
		{
			get
			{
				return Panel.PanelState == PanelState.IsWaitingToOpen;
			}
			set
			{
				if (value)
				{
					Panel.PanelState = PanelState.IsWaitingToOpen;
				}

			}
		}

		public bool IsWaitingToClose
		{
			get
			{
				return Panel.PanelState == PanelState.IsWaitingToClose;
			}
			set
			{
				if (value)
				{
					Panel.PanelState = PanelState.IsWaitingToClose;
				}
			}
		}

		public bool IsOpening
		{
			get
			{
				return Panel.PanelState == PanelState.IsOpening;
			}
			set
			{
				if (value)
				{
					Panel.PanelState = PanelState.IsOpening;
				}

			}
		}

		public bool IsClosing
		{
			get
			{
				return Panel.PanelState == PanelState.IsClosing;
			}
			set
			{
				if (value)
				{
					Panel.PanelState = PanelState.IsClosing;
				}
			}
		}

		public bool IsOpen
		{
			get
			{
				return Panel.PanelState == PanelState.Opened;
			}
			set
			{
				if (value)
				{
					Panel.PanelState = PanelState.Opened;
				}

			}
		}

		public bool IsClosed
		{
			get
			{
				return Panel.PanelState == PanelState.Closed;
			}
			set
			{
				if (value)
				{
					Panel.PanelState = PanelState.Closed;
				}

			}
		}

		public bool NotDefined
		{
			get
			{
				return Panel.PanelState == PanelState.NotDefined;
			}
			set
			{
				if (value)
				{
					Panel.PanelState = PanelState.NotDefined;
				}
			}
		}

		public bool Collapsed = false;
		public int HierarchyIndex = 0;

		public List<WooPanelNode> Children = new List<WooPanelNode>();
		public WooPanelNode Parent;
		public Transform ParentTransform;

		public float simulatedTime = 0.0f;
		public float remainingWaitingTime = 0.0f;
		public float waitingTime = 0.0f;

		protected Coroutine coroutine;
		protected List<Coroutine> coroutines = new List<Coroutine>();
		protected List<IEnumerator> enumerators = new List<IEnumerator>();
		internal bool ForceMirrored = false;

		public AnimationClip GetClosingAnimation()
		{
			if (Panel == null)
			{
				return null;
			}

			var animator = Panel.GetComponent<Animator>();
			if (animator == null)
			{
				return null;
			}

			var animatorController = animator.runtimeAnimatorController;

			if (animatorController == null)
			{
				return null;
			}

			var animation = animatorController.animationClips.FirstOrDefault(clip => clip.name == "PanelClosed");

			if (animation == null || ForceMirrored)
			{
				animation = animatorController.animationClips.FirstOrDefault(clip => clip.name == "PanelOpened");
			}

			return animation;
		}

		public AnimationClip GetOpeningAnimation()
		{
			var animator = Panel.GetComponent<Animator>();
			if (animator == null)
			{
				return null;
			}

			var animatorController = animator.runtimeAnimatorController;

			if (animatorController == null)
			{
				return null;
			}

			var animation = animatorController.animationClips.FirstOrDefault(clip => clip.name == "PanelOpened");

			return animation;
		}

		public bool IsMirrored()
		{
			var animator = Panel.GetComponent<Animator>();
			if (animator == null)
			{
				return false;
			}

			var animatorController = animator.runtimeAnimatorController;

			if (animatorController == null)
			{
				return false;
			}

			if (ForceMirrored)
			{
				return true;
			}

			var animation = animatorController.animationClips.FirstOrDefault(clip => clip.name == "PanelClosed");

			return animation == null;
		}

		public AnimationClip GetActiveAnimation()
		{
			return (IsOpening || IsOpen || IsWaitingToOpen) ? GetOpeningAnimation() : GetClosingAnimation();
		}

		public float TimeToClose()
		{
			var modifier = Panel.PanelProperties.ClosingSpeed;

			var timeLeft = (IsOpen || IsOpening || IsWaitingToOpen) ? simulatedTime : (1.0f - simulatedTime);

			if (NotDefined)
			{
				timeLeft = 0.0f;
			}
			else if (!IsMirrored())
			{
				timeLeft = (IsOpen || IsOpening || IsWaitingToOpen) ? 1.0f : (1.0f - simulatedTime);
			}
			else
			{
				timeLeft = (IsOpen || IsOpening || IsWaitingToOpen) ? simulatedTime : (1.0f - simulatedTime);
			}

			timeLeft = Mathf.Clamp(timeLeft, 0.0f, 1.0f);

			return timeLeft * GetClosingAnimation().length / modifier;
		}

		public float TimeToOpen()
		{
			var modifier = Panel.PanelProperties.OpeningSpeed;
			var timeLeft = (IsClosed || IsClosing || IsWaitingToClose) ? simulatedTime : (1.0f - simulatedTime);

			if (NotDefined)
			{
				//timeLeft = Panel.PanelProperties.SkipFirstAnimation ? 0.0f : 1.0f;
				timeLeft = 1.0f;
			}
			else if (!IsMirrored())
			{
				timeLeft = (IsClosed || IsClosing || IsWaitingToClose) ? 1.0f : (1.0f - simulatedTime);
			}
			else
			{
				timeLeft = (IsClosed || IsClosing || IsWaitingToClose) ? simulatedTime : (1.0f - simulatedTime);
			}

			timeLeft = Mathf.Clamp(timeLeft, 0.0f, 1.0f);

			return GetOpeningAnimation() == null ? 0.0f : timeLeft * GetOpeningAnimation().length / modifier;
		}

		public void Invoke(float delay, System.Action action)
		{
			coroutines.Add(InvokePrivate(delay, action));
		}

		public void StopCoroutines()
		{
			if (!Application.isPlaying)
			{
				#if UNITY_EDITOR
				foreach (var enumerator in enumerators)
				{
					if (enumerator != null && Panel != null)
					{
						EditorCoroutine.StopCoroutine(enumerator, Panel);					
					}
				}
				#endif
			}
			else
			{
				foreach (var coroutine in coroutines)
				{
					if (coroutine != null && Panel != null)
					{
						Panel.StopCoroutine(coroutine);					
					}
				}
			}

			coroutines.Clear();
			enumerators.Clear();
		}

		public void MoveNext()
		{
			foreach (var coroutine in enumerators)
			{
				if (coroutine != null)
				{
					coroutine.MoveNext();
				}
			}
		}

		private Coroutine InvokePrivate(float delay, System.Action action)
		{
			if (delay == 0.0f)
			{
				action.Invoke();
				return null;
			}

			if (!Panel.gameObject.activeInHierarchy)
			{
				return null;
			}

			var enumerator = InvokeHelper(new WaitForSeconds(delay), action);
			enumerators.Add(enumerator);
			if (!Application.isPlaying)
			{
				#if UNITY_EDITOR
				EditorCoroutine.StartCoroutine(enumerator, Panel);
				#endif
				return null;
			}

			return Panel.StartCoroutine(enumerator);
		}

		private Coroutine Invoke(YieldInstruction instruction, System.Action action)
		{
			if (!Panel.gameObject.activeInHierarchy)
			{
				return null;
			}
			if (Application.isPlaying)
			{
				#if UNITY_EDITOR
				EditorCoroutine.StartCoroutine(InvokeHelper(instruction, action), Panel);
				#endif
				return null;
			}

			return Panel.StartCoroutine(InvokeHelper(instruction, action));
		}

		private IEnumerator InvokeHelper(YieldInstruction instruction, System.Action action)
		{
			yield return instruction;
			//Debug.Log(Panel.name + " invoke");
			action.Invoke();
		}

		public Rect rect;

		public Color Color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		public Color TargetColor;

		public bool LerpColor()
		{
			Color = Color.Lerp(Color, TargetColor, 0.85f);
			if (DistanceBetweenColors(Color, TargetColor) > Mathf.Epsilon)
			{
				return true;
			}

			return false;
		}

		private float DistanceBetweenColors(Color a, Color b)
		{
			var vecA = new Vector4(a.r, a.g, a.b, a.a);
			var vecB = new Vector4(b.r, b.g, b.b, b.a);

			return (vecA - vecB).sqrMagnitude;
		}
	}
}
