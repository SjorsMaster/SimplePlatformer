using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndButtons : MonoBehaviour
{
    public void OpenMiyuGame(){
        #if UNITY_WEBGL
     Application.ExternalEval("window.open(\"https://miyulake.itch.io/the-great-moyai-escape\")");
    #else
        Application.OpenURL("https://miyulake.itch.io/the-great-moyai-escape");
        #endif
    }
    public void BackToMenu(){
        SceneManager.LoadScene("Menu");
    }
}
