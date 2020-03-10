using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The GameManager is a singleton object that has the intent to hold data between the scenes.
    /// If we would like to save any progress in the game, this should be done through the GameManager.
    /// </summary>
    private static GameManager gameManagerInstance;

    //GameManager should keep track on the following...
    private int lastClearedLevel = 0;

    //LevelSettings for board (This will hold the current level)
    private int currentLevel;
    private int boardWidth;
    private int boardHeight;
    private int moves;
    private int scoreGoal;

    void Awake()
    {
        //We only need one instance of the GameManager alive at all times. Delete the object if it already exists when going back to main menu.
        if (gameManagerInstance == null)
        {
            gameManagerInstance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        //Will keep GameManager alive between the scenes.
        DontDestroyOnLoad(gameManagerInstance);
    }

    //Get and Set for Last Cleared Level.
    public int GetLastClearedLevel()
    {
        return lastClearedLevel;
    }

    public void SetLastClearedLevel(int NumberOfClearedLevel)
    {
        lastClearedLevel = NumberOfClearedLevel;
    }

    //Get and Set for Current Level.
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void SetCurrentLevel(int CurrentLevel)
    {
        currentLevel = CurrentLevel;
    }

    //Get and Set for Current Board Width Setting.
    public int GetBoardWidth()
    {
        return boardWidth;
    }

    public void SetBoardWidth(int width)
    {
        boardWidth = width;
    }

    //Get and Set for Current Rows Setting.
    public int GetBoardHeight()
    {
        return boardHeight;
    }

    public void SetBoardHeight(int height)
    {
        boardHeight = height;
    }

    //Get and Set for Current Moves Setting.
    public int GetMoves()
    {
        return moves;
    }

    public void SetMoves(int nrOfMoves)
    {
        moves = nrOfMoves;
    }

    //Get and Set for Score Goal Setting.
    public int GetScoreGoal()
    {
        return scoreGoal;
    }

    public void SetScoreGoal(int goal)
    {
        scoreGoal = goal;
    }

}
