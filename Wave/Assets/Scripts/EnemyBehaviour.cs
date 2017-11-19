using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private string enemyColor;
    private bool readyToDestroy;
    private int collisionInstanceID;
    private bool stay = true;

    public void OnEnemyPointerDown()
    {
        if (readyToDestroy)
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        enemyColor = tag;
        readyToDestroy = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collisionInstanceID = collision.GetInstanceID();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (stay && collisionInstanceID == collision.GetInstanceID())
        {
            if (enemyColor == collision.tag)
            {
                readyToDestroy = true;
                stay = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        readyToDestroy = false;
        stay = true;
    }

}
