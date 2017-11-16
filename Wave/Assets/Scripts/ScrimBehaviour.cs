using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wooplex.Panels;

public class ScrimBehaviour : MonoBehaviour {

    [Header("--Instantiation--------------------------------------------------------------------------------------")]
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform creatorContainer;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject waveCreatorPrefab;


    private void Awake()
    {
        
    }

    private void Start()
    {
        InstantiatePrefabs(1, 1);
    }

    private void InstantiatePrefabs(int numCreator, int numEnemy)
    {
        InstantiateCreator(numCreator);
        InstantiateEnemy(numEnemy);
    }

    private void InstantiateCreator(int numCreator)
    {
        for (int i = 0; i < numCreator; i++)
        {
            GameObject go = Instantiate(waveCreatorPrefab, creatorContainer);
            
        }
    }

    private void InstantiateEnemy(int numEnemy)
    {
        for (int i = 0; i < numEnemy; i++)
        {
            GameObject go = Instantiate(enemyPrefab, enemyContainer);
        }
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

    //public void InstantiateShapesFromPrefab(string textureName)
    //{
    //    string prefabPath = partOfPath + textureName;
    //    GameObject letterPrefab = (GameObject)Resources.Load(prefabPath);
    //    int shapesCount = letterPrefab.transform.childCount;

    //    for (int i = 0; i < shapesCount; i++)
    //    {
    //        ShapeBehaviour newShape = InstantiateShape(letterPrefab.transform.GetChild(i).gameObject);
    //    }
    //}

    //public ShapeBehaviour InstantiateShape(GameObject sourceObject)
    //{
    //    ShapeBehaviour newShape = Instantiate(sourceObject, ObjRectTransform).GetComponent<ShapeBehaviour>();
    //    shapes.Add(newShape);
    //    return newShape;
    //}

}
