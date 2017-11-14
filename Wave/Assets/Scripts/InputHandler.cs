using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Wooplex.Panels;

public class InputHandler : Singleton<InputHandler>
{
    //public CanvasScaleObserver ScaleObserver;
	public event Action OnWave;

    private Touch[] touches;

	private void LateUpdate()
	{
		touches = Input.touches;

        if (touches.Length == 1)
        {
            OnSingleTouch();
        }
	}

    void OnSingleTouch()
    {
        OnWave.Invoke()
    }


}
