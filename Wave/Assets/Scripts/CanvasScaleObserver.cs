using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScaleObserver : MonoBehaviour
{
	private Vector2 scaleMultiplier = new Vector2(1, 1);

	public Vector2 ScaleMultiplier
	{
		get
		{
			return scaleMultiplier;
		}
	}

	private CanvasScaler canvasScaler;

	void Awake()
	{
		canvasScaler = GetComponent<CanvasScaler>();
		float curentDpi = Screen.dpi;
					
		if (canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
		{
			scaleMultiplier = new Vector2(Screen.width / canvasScaler.referenceResolution.x, Screen.height / canvasScaler.referenceResolution.y);
		}
		else
		{
			Debug.LogError("Currently CanvasScaleObserver does not support " + canvasScaler.screenMatchMode);
		}
	}



}
