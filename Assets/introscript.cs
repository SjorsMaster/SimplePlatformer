using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class introscript : MonoBehaviour
{
    public bool GO;
    public string nextScene;
    // Update is called once per frame
    void Update()
    {
        if(Input.anyKey){
            GO = true;
        }

        if(GO) SceneManager.LoadScene(nextScene);

    }
}
