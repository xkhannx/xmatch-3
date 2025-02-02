using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LevelManager : MonoBehaviour
{
    GridManager grid;
    private void Start()
    {
        grid = GetComponent<GridManager>();
        LoadLevel();
    }

    [SerializeField] Text timerText;
    private void Update()
    {
        timerText.text = "Time: " + Time.timeSinceLevelLoad.ToString("F0");
    }

    [SerializeField] Text title;
    void LoadLevel() // run only once
    {
        CurrentLevel cur = FindObjectOfType<CurrentLevel>();

        string rootPath = Application.persistentDataPath + "/maps";

        title.text = cur.levelName;

        GetComponent<MapSaver>().LoadMapInGame();

        grid.CreateGrid();
    }

    int moveCount = 0;
    [SerializeField] Text moveCountText;
    public void UpdateMoveCount()
    {
        moveCount++;
        moveCountText.text = "Moves: " + moveCount.ToString();
    }

    public void EditMap()
    {
        SceneManager.LoadScene("Level editor");
    }

    public void Restart()
    {
        LoadLevel();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
