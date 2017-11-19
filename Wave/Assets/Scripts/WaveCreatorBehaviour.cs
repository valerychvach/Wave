using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wooplex.Panels;

public class WaveCreatorBehaviour : MonoBehaviour
{
    private GameObject wavePrefab;
    private Image waveImage;

    private float maxRadius = 0;
    private bool waveInProgress = false;
    private GameObject wave;
    private string pathToWavePrefab = "Prefabs/Wave";

    private void Awake()
    {
        UploadPrefab();
    }

    private void Start()
    {
        UploadPrefab();
        FindMaxRadius();
    }

    public void OnWaveCreatorPointerDown()
    {
        if (!waveInProgress)
        {
            StartCoroutine(StartWave());
        }
    }

    private void UploadPrefab()
    {
        wavePrefab = (GameObject)Resources.Load(pathToWavePrefab);
        waveImage = wavePrefab.GetComponent<Image>();
    }

    private IEnumerator StartWave()
    {
        waveInProgress = true;
        float progress = 0.01f;
        wave = Instantiate(wavePrefab, transform.position, Quaternion.identity, this.transform);
        Vector2 StartScale = wave.transform.localScale;

        while (progress < 1)
        {
            yield return new WaitForFixedUpdate();
            //wave.transform.localScale = new Vector2(maxRadius * progress, maxRadius * progress);
            wave.transform.localScale = Vector2.Lerp(StartScale, new Vector2(maxRadius * progress, maxRadius * progress), progress);

            progress += 0.002f;
        }

        Destroy(wave);
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
