using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public GameObject spawnPoint;

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "Player"){
            spawnPoint.transform.position = transform.position;
            Destroy(this.gameObject);
        }

    }
}
