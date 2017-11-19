using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wooplex.Panels;

public class WaveCreatorBehaviour : MonoBehaviour
{
    [SerializeField] private RectTransform wave;
    private string color;
    private GameObject wavePrefab;
    private float maxRadius = 0;
    private bool waveInProgress;
    //private GameObject wave;
    //private string pathToPrefab = "Prefabs/Wave";

    private void Start()
    {
        color = tag;
        waveInProgress = false;
        UploadPrefab();
        FindMaxRadius();
    }

    private string str;

    private void OnGUI()
    {
        GUILayout.TextArea(str);
    }

    public void OnWaveCreatorPointerDown()
    {
        if (!waveInProgress)
        {
            //wave = Instantiate(wavePrefab, transform.position, Quaternion.identity, this.transform);
            StartCoroutine(StartWave());
        }
    }

    private void UploadPrefab()
    {
        //wavePrefab = (GameObject)Resources.Load(pathToPrefab + color);
    }

    private IEnumerator StartWave()
    {
        waveInProgress = true;
        float progress = 0;
        //wave = Instantiate(wavePrefab, transform.position, Quaternion.identity, this.transform);
        wave.gameObject.SetActive(true);

        //Vector2 StartScale = wave.transform.localScale;
        Vector2 StartScale = wave.localScale;
        Vector2 TargetScale = new Vector2(maxRadius + StartScale.x, maxRadius + StartScale.y);



        while (progress < 1)
        {
            //wave.transform.localScale = new Vector2(maxRadius * progress, maxRadius * progress);
            //wave.GetComponent<RectTransform>().sizeDelta = Vector2.Lerp(StartScale, new Vector2(maxRadius * progress, maxRadius * progress), progress);
            wave.localScale = Vector2.Lerp(StartScale, TargetScale, progress);
            //wave.sizeDelta = Vector2.Lerp(startSize, TargetScale, progress);
            //wave.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.Lerp(StartScale, TargetScale, progress);

            progress += 0.003f;
            yield return null;
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
