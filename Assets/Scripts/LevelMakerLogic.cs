using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelMakerLogic : MonoBehaviour
{
    /// <summary>
    /// This code helps creating levels through the inspector.
    /// It generates all the buttons in runtime.
    /// 
    /// As the GameManager has the role to hold data between the scenes in this project, 
    /// we send the configuration of the clicked level to it. Which the Gameplay scene loads from.
    /// </summary>

    [Serializable]
    private class LevelInfo
    {
        //Here are the variables that handles the features of our game.
        [Header("Board width.")]
        [Range(5, 9)]
        public int width = 5;
        [Header("Board height.")]
        [Range(5, 9)]
        public int height = 5;
        [Header("Number of player moves.")]
        public int moves = 10;
        [Header("Score that has to be beaten to be able to go to next level.")]
        public int scoreGoal = 300;
    }

    [SerializeField]
    private GameObject levelButton;

    [SerializeField]
    private GameObject levelsArea;

    [Header("Space between level buttons. Default is 200.")]
    private float spaceBetweenButtons = 200;

    [SerializeField]
    [Header("Fill in number of levels and set options. Script are generating buttons for each level.")]
    private List<LevelInfo> levels = new List<LevelInfo>();

    GameManager gameManager;
    SoundManager soundManager;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        float buttonPositionY = 0;
        int lastClearedLevel = gameManager.GetLastClearedLevel();

        //Prepare levelButtons in the scene
        for(int i = 0; i < levels.Count; i++)
        {
            var tempButton = Instantiate(levelButton);
            tempButton.transform.SetParent(levelsArea.transform);
            tempButton.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, buttonPositionY);
            tempButton.GetComponentInChildren<Text>().text = (i + 1).ToString();
            int number = i;//Needed instead of putting i directly.
            tempButton.GetComponent<Button>().onClick.AddListener(() => GoToLevel(number));
            buttonPositionY += spaceBetweenButtons;

            //Activate buttons for the player, More levels are acquired through completing previous levels.
            if (gameManager.GetLastClearedLevel() + 1 >= i + 1)
            {
                tempButton.GetComponent<Button>().interactable = true;
            }
        }

        levelsArea.GetComponent<RectTransform>().offsetMax = new Vector2(levelsArea.GetComponent<RectTransform>().offsetMax.y, buttonPositionY - spaceBetweenButtons);
    }

    //StartScreenButton - function for the onClick event in the editor.
    public void GoToStartScreen()
    {
        soundManager.PlayPushButtonSound();
        SceneManager.LoadScene("StartScreen");
    }

    public void GoToLevel(int level)
    {

        //Set current level configuration and features to the GameManager.
        gameManager.GetComponent<GameManager>().SetBoardWidth(levels[level].width);
        gameManager.GetComponent<GameManager>().SetBoardHeight(levels[level].height);
        gameManager.GetComponent<GameManager>().SetCurrentLevel(level + 1);
        gameManager.GetComponent<GameManager>().SetMoves(levels[level].moves);
        gameManager.GetComponent<GameManager>().SetScoreGoal(levels[level].scoreGoal);

        //Goto Level Scene
        soundManager.PlayPushButtonSound();
        SceneManager.LoadScene("Gameplay");
    }
}
