using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenLogic : MonoBehaviour
{
    SoundManager soundManager;

    private void Awake()
    {
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

    //StartButton - function for the onClick event in the editor.
    public void GoToLevels()
    {
        soundManager.PlayPushButtonSound();
        SceneManager.LoadScene("Levels");
    }
}
