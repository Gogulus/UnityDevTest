using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// This class will handle all board mechanics and read input from the player.
/// All stages of the gameplay will be handled by a state machine.
/// </summary>


public class BoardGameplayLogic : MonoBehaviour
{
    enum BoardState { Begin, WaitForInput, DestroyObjects, MoveDownObjects, SpawnMissingObjects, Reshuffle, Complete, Failed }

    BoardState state;

    private GameObject[,] board;//This is the board holding every gameobject for calculations.
    private bool startBegin;
    private bool startMovingDownBlocks;
    private bool startSpawnMissingBlocks;
    private bool startReshuffle;
    private bool startComplete;
    private bool startFailed;

    [HideInInspector]
    public List<GameObject> selectedObjects;

    //Add types in the Inspector, this is normal spawnrate objects like the regular flowers.
    [SerializeField]
    private GameObject objectBackground;
    [SerializeField]
    private GameObject[] objectTypes;

    //Distance between objects on the board.
    private float distanceBetweenObjects = 0.52f;

    //Get recent width and height etc for the level.
    private int width;
    private int height;
    private int moves;
    private int score;
    private int scoreGoal;
    private int currentLevel;

    //Checks if reshuffle is needed
    private bool checkBoard = true;

    //UI text
    [SerializeField]
    private Text totalScoreText;
    [SerializeField]
    private Text movesLeftText;
    [SerializeField]
    private Text remainingScoreText;
    [SerializeField]
    private Text statusText;

    GameManager gameManager;
    SoundManager soundManager;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        //Load information from the GameManager
        width = gameManager.GetBoardWidth();
        height = gameManager.GetBoardHeight();
        scoreGoal = gameManager.GetScoreGoal();
        currentLevel = gameManager.GetCurrentLevel();
        moves = gameManager.GetMoves();

        //Set status to nothing. Will later show "Complete" or "Failed".
        statusText.text = "";

        //Set position and zoom level for the camera depending on the total width on the board. (These values where tested on Android and on Windows)
        if (width == 5) { Camera.main.transform.position = new Vector3(1f, -1.75f, Camera.main.transform.position.z); Camera.main.orthographicSize = 3.6f; }
        else if (width == 6) { Camera.main.transform.position = new Vector3(1.29f, -1.75f, Camera.main.transform.position.z); Camera.main.orthographicSize = 4; }
        else if (width == 7) { Camera.main.transform.position = new Vector3(1.55f, -1.75f, Camera.main.transform.position.z); Camera.main.orthographicSize = 4.2f; }
        else if (width == 8) { Camera.main.transform.position = new Vector3(1.8f, -1.75f, Camera.main.transform.position.z); Camera.main.orthographicSize = 4.4f; }
        else if (width == 9) { Camera.main.transform.position = new Vector3(2.05f, -1.75f, Camera.main.transform.position.z); Camera.main.orthographicSize = 4.8f; }

        board = new GameObject[width, height];

        //Initialize starting state.
        startBegin = true;
        state = BoardState.Begin;
    }

    void Update()
    {
        //Keep score goal to zero value if lower.
        if (scoreGoal < 0)
        {
            scoreGoal = 0;
        }

        //Handle UIText
        movesLeftText.text = moves.ToString();
        totalScoreText.text = score.ToString();
        remainingScoreText.text = scoreGoal.ToString();


        //Handle game mechanics by switching state. This keep the game mechanics in steps.
        switch(state)
        {
            case BoardState.Begin:
                //This will make the PrepareBoard function run once.
                if(startBegin)
                {
                    StartCoroutine(PrepareBoard());
                    startBegin = false;
                }

                //Change to input stage as soon as all is in place.
                if(CheckIfBoardIsReadyForInput())
                {
                    //When done. Set new state.
                    state = BoardState.WaitForInput;
                }

                break;
            case BoardState.WaitForInput:

                //Spinning and Scaling effect on flowers when selected.
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (board[i, j] != null)
                        {
                            if (selectedObjects.Contains(board[i, j]))
                            {
                                //Selected
                                board[i, j].GetComponent<Transform>().Rotate(new Vector3(0, 0, -4));
                                board[i, j].GetComponent<Transform>().localScale = new Vector3(0.8f, 0.8f);
                            }
                            else
                            {
                                //Unselected
                                board[i, j].GetComponent<Transform>().rotation = new Quaternion(0, 0, 0, 0);
                                board[i, j].GetComponent<Transform>().localScale = new Vector3(1f, 1f);
                            }
                        }
                    }
                }

                //Check if level is complete or if it has failed.
                if (scoreGoal <= 0)
                {
                    startComplete = true;
                    state = BoardState.Complete;
                }
                else if (moves <= 0)
                {
                    startFailed = true;
                    state = BoardState.Failed;
                }

                //Support for Windows, Android and IOS.
#if UNITY_ANDROID || UNITY_IOS

                //FOR TOUCH CONTROL
                //Select the first flower type and keep selecting new flowers when moving of the same type.
                if (Input.touchCount == 1)
                {
                        RaycastHit2D hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.touches[0].position).x, Camera.main.ScreenToWorldPoint(Input.touches[0].position).y), Vector2.zero, 0);
                        if (hit)
                        {
                            if (selectedObjects.Count == 0)
                            {
                                selectedObjects.Add(hit.transform.gameObject);
                                soundManager.PlaySelectObjectSound(selectedObjects.Count);
                            }
                            else if (!selectedObjects.Contains(hit.transform.gameObject) &&
                                     selectedObjects[0].GetComponent<Object>().blockType == hit.transform.gameObject.GetComponent<Object>().blockType &&
                                     CheckIfNeighbourToLastSelected(selectedObjects[selectedObjects.Count - 1], hit.transform.gameObject))
                            {
                                selectedObjects.Add(hit.transform.gameObject);
                                soundManager.PlaySelectObjectSound(selectedObjects.Count);
                            }

                            //Remove last selected flower if we go back.
                            if (selectedObjects.Count > 1)
                            {
                                if (selectedObjects.Contains(hit.transform.gameObject) &&
                                hit.transform.gameObject == selectedObjects[selectedObjects.Count - 2])
                                {
                                    selectedObjects.RemoveAt(selectedObjects.Count - 1);
                                    soundManager.PlaySelectObjectSound(selectedObjects.Count);
                                }
                            }

                        }
                    }

                    //When releasing touch, destroy flowers within the conditions.
                    if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.touchCount == 0)
                    {
                        if (selectedObjects.Count >= 3)
                        {
                            moves--;
                            state = BoardState.DestroyObjects;
                        }
                        else
                        {
                            selectedObjects.Clear();
                        }
                    }

#elif UNITY_STANDALONE
                //FOR MOUSE CONTROL
                //Select the first flower type and keep selecting new flowers when moving of the same type.
                if (Input.GetMouseButton(0))
                    {
                        RaycastHit2D hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0);
                        if (hit)
                        {
                            if (selectedObjects.Count == 0)
                            {
                                selectedObjects.Add(hit.transform.gameObject);
                                soundManager.PlaySelectObjectSound(selectedObjects.Count);
                            }
                            else if (!selectedObjects.Contains(hit.transform.gameObject) &&
                                     selectedObjects[0].GetComponent<Object>().blockType == hit.transform.gameObject.GetComponent<Object>().blockType &&
                                     CheckIfNeighbourToLastSelected(selectedObjects[selectedObjects.Count - 1], hit.transform.gameObject))
                            {
                                selectedObjects.Add(hit.transform.gameObject);
                                soundManager.PlaySelectObjectSound(selectedObjects.Count);
                            }

                            //Remove last selected flower if we go back.
                            if (selectedObjects.Count > 1)
                            {
                                if (selectedObjects.Contains(hit.transform.gameObject) &&
                                hit.transform.gameObject == selectedObjects[selectedObjects.Count - 2])
                                {
                                    selectedObjects.RemoveAt(selectedObjects.Count - 1);
                                    soundManager.PlaySelectObjectSound(selectedObjects.Count);
                                }
                            }

                        }
                }

                //When releasing mouse, destroy flowers within the conditions.
                if (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0))
                {
                    if (selectedObjects.Count >= 3)
                    {
                        moves--;
                        state = BoardState.DestroyObjects;
                    }
                    else
                    {
                        selectedObjects.Clear();
                    }
                }
#endif

                //Check if the board needs a reshuffle. Checks only once per turn.
                if (checkBoard)
                {
                    if (CheckIfBoardNeedsReshuffle())
                    {
                        startReshuffle = true;
                        state = BoardState.Reshuffle;
                    }
                    else
                    {
                        checkBoard = false;
                    }
                }

                break;
            case BoardState.DestroyObjects:

                for (int j = height - 1; j >= 0; j--)
                {
                    for (int i = 0; i < width; i++)
                    {
                        //Clear from board and destroy.
                        if(selectedObjects.Contains(board[i, j]))
                        {
                            Destroy(board[i, j]);
                            board[i, j] = null;

                            //Set new score after each flower destroyed.
                            score += 10;
                            scoreGoal -= 10;
                        }
                    }
                }
                soundManager.PlayDestroyObjectSound();

                selectedObjects.Clear();

                startMovingDownBlocks = true;
                state = BoardState.MoveDownObjects;

                break;
            case BoardState.MoveDownObjects:

                if(startMovingDownBlocks)
                {
                    StartCoroutine(MoveDownBlocks());
                    startMovingDownBlocks = false;
                }
                
                break;
            case BoardState.SpawnMissingObjects:

                if(startSpawnMissingBlocks)
                {
                    StartCoroutine(SpawnMissingBlocks());
                    startSpawnMissingBlocks = false;
                }

                break;
            case BoardState.Reshuffle:

                if (startReshuffle)
                {
                    StartCoroutine(ReshuffleBoard());
                    startReshuffle = false;
                }

                break;
            case BoardState.Complete:

                if (startComplete)
                {
                    StartCoroutine(CompletedLevel());
                    startComplete = false;
                }

                break;
            case BoardState.Failed:

                if (startFailed)
                {
                    StartCoroutine(FailedLevel());
                    startFailed = false;
                }

                break;
        }
    }

    IEnumerator PrepareBoard()
    {
        float waitTimeToNextRow = 0.3f;

        //Spawn background for positions.
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //Background for every position.
                var backgroundObject = Instantiate(objectBackground, new Vector3(this.transform.position.x + distanceBetweenObjects * i,
                                                                      -(this.transform.position.y + distanceBetweenObjects * j), 4), Quaternion.identity);
                backgroundObject.transform.SetParent(this.transform);
            }
        }

        //Spawn flowers.
        for (int j = height-1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                //Random flower is created at all empty positions.
                var randomObjectType = Random.Range(0, objectTypes.Length);
                board[i, j] = Instantiate(objectTypes[randomObjectType], new Vector3(this.transform.position.x + distanceBetweenObjects * i,
                                                                                  -(this.transform.position.y + distanceBetweenObjects * j), 0), Quaternion.identity);
                board[i, j].transform.SetParent(this.transform);
            }

            //Wait a small time for every row
            yield return new WaitForSeconds(waitTimeToNextRow);
        }

        yield return null;
    }

    IEnumerator MoveDownBlocks()
    {
        //Move down flowers.
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                //Find new position for each flower
                if(board[i, j] != null)
                {
                    var k = j;
                    while(k < height - 1)
                    {
                        if(board[i, k+1] == null)
                        {
                            k++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //If new position is found, inform the flower to fall and set new position in board.
                    if(k > j)
                    {
                        board[i, k] = board[i, j];
                        board[i, j] = null;

                        //Set new newPositionY for the Flower to fall to.
                        board[i, k].GetComponent<Object>().newPositionY = -(this.transform.position.y + distanceBetweenObjects * k);
                    }
                }
            }
        }

        startSpawnMissingBlocks = true;
        state = BoardState.SpawnMissingObjects;

        yield return null;
    }

    IEnumerator SpawnMissingBlocks()
    {
        float waitTimeToNextRow = 0.125f;

        //Spawn objects in certain times.
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                //Random block is created at all empty positions.
                if(board[i, j] == null)
                {
                    var randomObjectType = Random.Range(0, objectTypes.Length);
                    board[i, j] = Instantiate(objectTypes[randomObjectType], new Vector3(this.transform.position.x + distanceBetweenObjects * i,
                                                                                      -(this.transform.position.y + distanceBetweenObjects * j), 0), Quaternion.identity);
                    board[i, j].transform.SetParent(this.transform);
                }
            }

            //Wait a small time for every row
            yield return new WaitForSeconds(waitTimeToNextRow);
        }

        //Wait for falling blocks.
        yield return new WaitForSeconds(1.0f);

        checkBoard = true;
        state = BoardState.WaitForInput;

        yield return null;
    }

    bool CheckIfNeighbourToLastSelected(GameObject lastSelected, GameObject selected)
    {
        //Find block position in the board
        var foundSelected = false;
        int posX = -999, posY = -999;
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                if (board[i, j] == selected)
                {
                    posX = i;
                    posY = j;
                    foundSelected = true;
                }

                if(foundSelected)
                {
                    break;
                }
            }

            if(foundSelected)
            {
                break;
            }
        }

        //Check neighbours of selected and see if you can find lastSelected.
        var foundLastSelected = false;

        // [x][x][x]
        // [x][ ][x] Check all neighbours.
        // [x][x][x]

        if(posX - 1 >= 0)
        {
            if(posY - 1 >= 0)
            {
                if (board[posX - 1, posY - 1] == lastSelected) { foundLastSelected = true; }
            }

            if (board[posX - 1, posY] == lastSelected) { foundLastSelected = true; }

            if(posY + 1 < height)
            {
                if (board[posX - 1, posY + 1] == lastSelected) { foundLastSelected = true; }
            }
        }

        if (posY - 1 >= 0)
        {
            if (board[posX, posY - 1] == lastSelected) { foundLastSelected = true; }
        }

        if (posY + 1 < height)
        {
            if (board[posX, posY + 1] == lastSelected) { foundLastSelected = true; }
        }

        if(posX + 1 < width)
        {
            if (posY - 1 >= 0)
            {
                if (board[posX + 1, posY - 1] == lastSelected) { foundLastSelected = true; }
            }

            if (board[posX + 1, posY] == lastSelected) { foundLastSelected = true; }

            if (posY + 1 < height)
            {
                if (board[posX + 1, posY + 1] == lastSelected) { foundLastSelected = true; }
            }
        }
        

        if(foundSelected && foundLastSelected)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    bool CheckIfBoardIsReadyForInput()
    {
        var blocksNotReady = 0;
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                //Check every object.
                if(board[i, j] != null)
                {
                    if (!board[i, j].GetComponent<Object>().ObjectIsReady())
                    {
                        blocksNotReady++;
                    }
                }
                else
                {
                    blocksNotReady++;
                }
            }
        }
        
        if(blocksNotReady > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    bool CheckIfBoardNeedsReshuffle()
    {
        var reshuffle = true;
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                //Check every object.
                if (board[i, j] != null)
                {
                    var nrOfNeighboursWithSameType = 0;
                    var myType = board[i, j].GetComponent<Object>().blockType;
                    if (i - 1 >= 0)
                    {
                        if (j - 1 >= 0)
                        {
                            if (board[i - 1, j - 1].GetComponent<Object>().blockType == myType) { nrOfNeighboursWithSameType++; }
                        }

                        if (board[i - 1, j].GetComponent<Object>().blockType == myType) { nrOfNeighboursWithSameType++; }

                        if (j + 1 < height)
                        {
                            if (board[i - 1, j + 1].GetComponent<Object>().blockType == myType) { nrOfNeighboursWithSameType++; }
                        }
                    }

                    if (j - 1 >= 0)
                    {
                        if (board[i, j - 1].GetComponent<Object>().blockType == myType) { nrOfNeighboursWithSameType++; }
                    }

                    if (j + 1 < height)
                    {
                        if (board[i, j + 1].GetComponent<Object>().blockType == myType) { nrOfNeighboursWithSameType++; }
                    }

                    if (i + 1 < width)
                    {
                        if (j - 1 >= 0)
                        {
                            if (board[i + 1, j - 1].GetComponent<Object>().blockType == myType) { nrOfNeighboursWithSameType++; }
                        }

                        if (board[i + 1, j].GetComponent<Object>().blockType == myType) { nrOfNeighboursWithSameType++; }

                        if (j + 1 < height)
                        {
                            if (board[i + 1, j + 1].GetComponent<Object>().blockType == myType) { nrOfNeighboursWithSameType++; }
                        }
                    }

                    //Two neighbours of the same type means that board does not need a reshuffle. Return false.
                    if(nrOfNeighboursWithSameType >= 2)
                    {
                        reshuffle = false;
                        return reshuffle;
                    }
                }
            }
        }

        return reshuffle;
    }

    IEnumerator ReshuffleBoard()
    {
        statusText.text = "Reshuffles";
        yield return new WaitForSeconds(1.0f);

        soundManager.PlayReshuffleSound();
        //Shuffle objects.
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                //Shuffle with random object for each object once.
                int k = Random.Range(0, width);
                int l = Random.Range(0, height);

                //Change place in board and change the position on both objects.
                
                var tempPosY = board[i, j].GetComponent<Object>().newPositionY;
                var tempPos = board[i, j].transform.position;
                var temp = board[i, j];

                
                board[i, j].GetComponent<Object>().newPositionY = board[k, l].GetComponent<Object>().newPositionY;
                board[i, j].transform.position = board[k, l].transform.position;
                board[i, j] = board[k, l];

                
                board[k, l].GetComponent<Object>().newPositionY = tempPosY;
                board[k, l].transform.position = tempPos;
                board[k, l] = temp;

            }
        }

        //Wait for falling blocks.
        yield return new WaitForSeconds(1.0f);

        statusText.text = "";

        checkBoard = true;
        state = BoardState.WaitForInput;

        yield return null;
    }

    IEnumerator CompletedLevel()
    {
        //Show text for a while then move to levels.
        statusText.text = "Completed";

        soundManager.PlayCompleteSound();

        //Add score for remaining moves
        var addedScoreForRemainingMoves = moves;
        for(int i = 0; i < addedScoreForRemainingMoves; i++)
        {
            yield return new WaitForSeconds(1.0f);
            score += 100;
            moves--;
        }

        //Send progress to GameManager
        if(gameManager.GetLastClearedLevel() < currentLevel)
        {
            gameManager.SetLastClearedLevel(currentLevel);
        }

        yield return new WaitForSeconds(4.0f);

        SceneManager.LoadScene("Levels");

        yield return null;
    }

    IEnumerator FailedLevel()
    {
        //Show text for a while then move to levels.
        statusText.text = "Failed";

        soundManager.PlayFailedSound();

        yield return new WaitForSeconds(4.0f);

        SceneManager.LoadScene("Levels");

        yield return null;
    }
}
