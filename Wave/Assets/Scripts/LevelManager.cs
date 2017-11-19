using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public LevelData LevelData;

    private void Awake()
    {
        //LevelData = new LevelData(1);
    }


}

[HideInInspector]
public class LevelData
{
    //public List<>

    public LevelData(int numLevel)
    {
        for (int i = 0; i < numLevel; i++)
        {
            //Levels.Add
        }
    }
}
