using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wooplex.Panels;

public class ScrimBehaviour : MonoBehaviour {

    [Header("--Instantiation--------------------------------------------------------------------------------------")]
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform creatorContainer;
    
    // Prefabs
    private string pathToPrefab = "Prefabs/";
    private Dictionary<Prefabs, string> pathToPrefabsDictionary;
    private Dictionary<Prefabs, GameObject> gameObjects;

    enum Prefabs
    {
        CreatorRed,
        CreatorGreen,
        CreatorBlue,
        EnemyRed,
        EnemyGreen,
        EnemyBlue
    }

    private void Awake()
    {
        PreparePathToPrefabDictionary();
        PrepareGameObjectsDictionary();
    }

    private void Start()
    {
        InstantiatePrefabs();
    }

    private void PreparePathToPrefabDictionary()
    {
        pathToPrefabsDictionary = new Dictionary<Prefabs, string>();
        pathToPrefabsDictionary.Add(Prefabs.CreatorBlue, "WaveCreatorBlue");
        pathToPrefabsDictionary.Add(Prefabs.CreatorGreen, "WaveCreatorGreen");
        pathToPrefabsDictionary.Add(Prefabs.CreatorRed, "WaveCreatorRed");
        pathToPrefabsDictionary.Add(Prefabs.EnemyBlue, "EnemyBlue");
        pathToPrefabsDictionary.Add(Prefabs.EnemyGreen, "EnemyGreen");
        pathToPrefabsDictionary.Add(Prefabs.EnemyRed, "EnemyRed");
    }

    private void PrepareGameObjectsDictionary()
    {
        gameObjects = new Dictionary<Prefabs, GameObject>
        {
            { Prefabs.CreatorBlue, (GameObject)Resources.Load(pathToPrefab + pathToPrefabsDictionary[Prefabs.CreatorBlue]) },
            { Prefabs.CreatorGreen, (GameObject)Resources.Load(pathToPrefab + pathToPrefabsDictionary[Prefabs.CreatorGreen]) },
            { Prefabs.CreatorRed, (GameObject)Resources.Load(pathToPrefab + pathToPrefabsDictionary[Prefabs.CreatorRed]) },
            { Prefabs.EnemyBlue, (GameObject)Resources.Load(pathToPrefab + pathToPrefabsDictionary[Prefabs.EnemyBlue]) },
            { Prefabs.EnemyGreen, (GameObject)Resources.Load(pathToPrefab + pathToPrefabsDictionary[Prefabs.EnemyGreen]) },
            { Prefabs.EnemyRed, (GameObject)Resources.Load(pathToPrefab + pathToPrefabsDictionary[Prefabs.EnemyRed]) }
        };
    }

    private void InstantiatePrefabs()
    {
        InstantiateCreator();
        InstantiateEnemy();
    }

    private void InstantiateCreator()
    {
        // All levels data located in LevelManager
        Instantiate(gameObjects[Prefabs.CreatorBlue], creatorContainer);
        Instantiate(gameObjects[Prefabs.CreatorGreen], creatorContainer);
        Instantiate(gameObjects[Prefabs.CreatorRed], creatorContainer);
    }

    private void InstantiateEnemy()
    {
        // All levels data located in LevelManager
        Instantiate(gameObjects[Prefabs.EnemyBlue], enemyContainer);
        Instantiate(gameObjects[Prefabs.EnemyGreen], enemyContainer);
        Instantiate(gameObjects[Prefabs.EnemyRed], enemyContainer);
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
