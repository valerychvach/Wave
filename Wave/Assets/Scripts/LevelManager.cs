using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelManager : Singleton<LevelManager>
{
    public int NumOfEnemyOnScene = 0;
    public int NumOfDestroedEnemyOnScene = 0;
    public event Action OnLevelEnded;

    private void LateUpdate()
    {
        if (NumOfEnemyOnScene != 0 && NumOfEnemyOnScene == NumOfDestroedEnemyOnScene)
        {
            OnLevelEnded.InvokeSafely();
        }
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
