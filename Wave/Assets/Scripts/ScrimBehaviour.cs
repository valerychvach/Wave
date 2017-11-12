using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wooplex.Panels;

public class ScrimBehaviour : MonoBehaviour {

    [Header("--Self References--------------------------------------------------------------------------------------")]
    public RectTransform ObjRectTransform;
    [Header("--Instantiation--------------------------------------------------------------------------------------")]
    public GameObject ShapePrefab;
    [Header("--Shapes--------------------------------------------------------------------------------------")]
    private List<ShapeBehaviour> shapes = new List<ShapeBehaviour>();

    private string partOfPath = "Prefabs/";



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
