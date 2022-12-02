using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleFullscreen : MonoBehaviour
{
    public Toggle button;
    public BoxCollider2D col;

    void Awake(){
        button.isOn = Screen.fullScreen;

        #if UNITY_WEBGL || UNITY_EDITOR
            button.interactable = false;
            col.enabled = false;
        #endif
    }

    void OnTriggerEnter2D(Collider2D other) {

        if (other.tag == "Player") {
            Toggle();
        }
    }

    public void Toggle(){
            Screen.fullScreen = !Screen.fullScreen;
            button.isOn = Screen.fullScreen;
    }

}
