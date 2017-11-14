using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wooplex.Panels;

public class ScrimBehaviour : MonoBehaviour {

    [Header("--Self References--------------------------------------------------------------------------------------")]
    public RectTransform ObjRectTransform;
    [Header("--Instantiation--------------------------------------------------------------------------------------")]
    public GameObject ShapePrefab;
    [Header("--Shapes--------------------------------------------------------------------------------------")]
    private List<ShapeBehaviour> shapes = new List<ShapeBehaviour>();
    [Header("--Wave----------------------------------------------------------------------------------------")]
    [SerializeField] private Image WaveImage;
    [SerializeField] private AnimationCurve waveCurve;
    private IEnumerator waveIenumerator;
    private RectTransform waveRectTransform;
    private bool readyToTweenScale;


    private string partOfPath = "Prefabs/";

    private void Start()
    {
        InputHandler.Instance.OnWave += OnWave;
        waveRectTransform = WaveImage.GetComponent<RectTransform>();
    }

    private void OnGUI()
    {
        GUILayout.TextArea("OnWave");
    }

    private void OnWave()
    {
        //waveRectTransform.gameObject.SetActive(true);

        //Vector2 startSize = new Vector2(0f, 0f);
        ////Color startColor = new Color(30f / 255f, 30f / 255f, 30f / 255f, 0f);
        ////Color curentColor = startColor;
        //Vector2 wavePosition = Input.;
        ////        wavePosition.z = 0;
        //waveRectTransform.position = wavePosition;
        //waveRectTransform.sizeDelta = startSize;
        ////WaveImage.color = startColor;

        //Vector2 targetSize = Vector2.zero;
        //float targetRadius = 0;



        //targetSize = (Vector2.one * 2 * targetRadius) / canvasScaleMultiplier.y;

        //float progress = 0;
        //while (progress < 1)
        //{
        //    float evaluatedValue = WaveAlphaCurve.Evaluate(progress);
        //    curentColor.a = evaluatedValue;
        //    ScreenWaveImage.color = curentColor;
        //    screenWaveRectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, progress);
        //    progress += 0.04f;
        //    yield return null;
        //}
        //screenWaveRectTransform.gameObject.SetActive(false);
        //screenWaveIenumerator = null;
    }

        public void InstantiateShapesFromPrefab(string textureName)
    {
        string prefabPath = partOfPath + textureName;
        GameObject letterPrefab = (GameObject)Resources.Load(prefabPath);
        int shapesCount = letterPrefab.transform.childCount;

        for (int i = 0; i < shapesCount; i++)
        {
            ShapeBehaviour newShape = InstantiateShape(letterPrefab.transform.GetChild(i).gameObject);
        }
    }

        public ShapeBehaviour InstantiateShape(GameObject sourceObject)
    {
        ShapeBehaviour newShape = Instantiate(sourceObject, ObjRectTransform).GetComponent<ShapeBehaviour>();
        shapes.Add(newShape);
        return newShape;
    }

}
