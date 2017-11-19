using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private bool readyToDestroy = false;

    public void OnEnemyPointerDown()
    {
        Debug.Log("Destroy me if you can!");

        if (readyToDestroy)
        {
            Debug.Log("Oh nooo...");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckBehaviour(collision.GetInstanceID());
    }

    private void CheckBehaviour(int instanceID)
    {
        
    }

}
