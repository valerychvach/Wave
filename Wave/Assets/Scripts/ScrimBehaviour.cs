using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wooplex.Panels;
using UnityEngine.SceneManagement;

public class ScrimBehaviour : MonoBehaviour {

    [Header("--Instantiation--------------------------------------------------------------------------------------")]
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform creatorContainer;
    
    // Prefabs
    private string pathToPrefab = "Prefabs/";
    private Dictionary<Prefabs, string> pathToPrefabsDictionary;
    private Dictionary<Prefabs, GameObject> gameObjects;
    private List<GameObject> enemyList = new List<GameObject>();

    // Menu panel
    [SerializeField] private Transform menuPanel;

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
        LevelManager.Instance.OnLevelEnded += OnLevelEnded;
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
        enemyList.Add(Instantiate(gameObjects[Prefabs.EnemyBlue], enemyContainer));
        enemyList.Add(Instantiate(gameObjects[Prefabs.EnemyGreen], enemyContainer));
        enemyList.Add(Instantiate(gameObjects[Prefabs.EnemyRed], enemyContainer));

        LevelManager.Instance.NumOfEnemyOnScene = enemyList.Count;
    }

    private void OnLevelEnded()
    {
        menuPanel.gameObject.SetActive(true);
    }

    public void RestartLevel()
    {
        menuPanel.gameObject.SetActive(false);
        InstantiateEnemy();
    }

}
