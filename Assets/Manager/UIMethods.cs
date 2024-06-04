using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMethods : MonoBehaviour
{

    public void OnStart()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnEnd()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ReturnOpeningScene()
    {
        SceneManager.LoadScene("Opening");
    }
}
