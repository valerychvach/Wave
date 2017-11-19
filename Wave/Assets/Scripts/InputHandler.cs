using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Wooplex.Panels;

public class InputHandler : Singleton<InputHandler>
{
    public event Action OnTouchToSceen;
    private Touch[] touches;

	private void LateUpdate()
	{
		touches = Input.touches;

        if (touches != null && touches.Length == 1)
        {
            if (touches[0].phase == TouchPhase.Began)
            {
                OnBeganPhase();
            }
            else if (touches[0].phase == TouchPhase.Moved)
            {
                OnMovedPhase();
            }
            else if (touches[0].phase == TouchPhase.Ended)
            {
                OnEndedPhase();
            }
        }
	}

    void OnBeganPhase()
    {
        //OnTouchToSceen.InvokeSafely();
    }

    void OnMovedPhase()
    {

    }

    void OnEndedPhase()
    {

    }

}
