using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuinteractions : MonoBehaviour
{
    public bool state;
    public GameObject toggleA, toggleB;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.tag == "Player") {
            toggleA.SetActive(state);
            toggleB.SetActive(!state);

        }
    }

}
