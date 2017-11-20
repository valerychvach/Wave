using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wooplex.Panels;

public class WaveCreatorBehaviour : MonoBehaviour
{
    [SerializeField] private RectTransform wave;
    private float maxRadius = 0;
    private bool waveInProgress;
    private Vector2 startScaleWave;

    private void Start()
    {
        waveInProgress = false;
        FindMaxRadius();
    }

    public void OnWaveCreatorPointerDown()
    {
        if (!waveInProgress)
        {
            StartCoroutine(StartWave());
        }
    }

    private IEnumerator StartWave()
    {
        waveInProgress = true;
        float progress = 0;
        wave.gameObject.SetActive(true);

        startScaleWave = wave.localScale;
        Vector2 TargetScale = new Vector2(maxRadius + startScaleWave.x, maxRadius + startScaleWave.y);

        while (progress < 1)
        {
            wave.localScale = Vector2.Lerp(startScaleWave, TargetScale, progress);

            progress += 0.003f;
            yield return null;
        }

        //Destroy(wave);
        wave.localScale = startScaleWave;
        wave.gameObject.SetActive(false);
        waveInProgress = false;
    }

    private void FindMaxRadius()
    {
        if (Vector2.Distance((Vector2)transform.position, new Vector2(0, 0)) > maxRadius)
        {
            maxRadius = Vector2.Distance((Vector2)transform.position, new Vector2(0, 0));
        }
        if (Vector2.Distance((Vector2)transform.position, new Vector2(Screen.width, Screen.height)) > maxRadius)
        {
            maxRadius = Vector2.Distance((Vector2)transform.position, new Vector2(Screen.width, Screen.height));
        }
        if (Vector2.Distance((Vector2)transform.position, new Vector2(Screen.width, 0)) > maxRadius)
        {
            maxRadius = Vector2.Distance((Vector2)transform.position, new Vector2(Screen.width, 0));
        }
        if (Vector2.Distance((Vector2)transform.position, new Vector2(0, Screen.height)) > maxRadius)
        {
            maxRadius = Vector2.Distance((Vector2)transform.position, new Vector2(0, Screen.height));
        }

        maxRadius *= 2.1f;
    }

}
