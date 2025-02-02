using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapEditor : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        LoadLevel();
    }

    List<Node> usedNodes = new List<Node>();

    // Grid generation
    [Header("Grid gen parameters:")]
    [SerializeField] public Gem[] gemPrefab;
    [SerializeField] GameObject floorPrefab;
    public int gridSizeX, gridSizeY;
    float nodeRadius = 0.5f;
    public Node[,] grid;

    public List<int> usedColors = new List<int>();
    public void CreateGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        grid = new Node[gridSizeX, gridSizeY];

        float halfX = gridSizeX % 2 > 0 ? gridSizeX / 2 : gridSizeX / 2 - nodeRadius;
        float halfY = gridSizeY % 2 > 0 ? gridSizeY / 2 : gridSizeY / 2 - nodeRadius;

        Vector3 worldBottomLeft = transform.position - new Vector3(halfX, halfY);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + new Vector3(x * nodeRadius * 2, y * nodeRadius * 2, 0);

                grid[x, y] = new Node(true, worldPoint, x, y);

            }
        }

        ConstructMap();
    }

    public void ConstructMap()
    {
        int count = 0;
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                if (grid[i, j].walkable)
                {
                    grid[i, j].gem = Instantiate(gemPrefab[usedColors[count % usedColors.Count]], grid[i, j].worldPosition, Quaternion.identity, transform);
                    if (grid[i, j].walkable)
                        grid[i, j].floorTile = Instantiate(floorPrefab, grid[i, j].worldPosition, Quaternion.identity, transform);
                    count++;
                }
            }
        }
    }

    [Header("Titles")]
    [SerializeField] Text title;
    [SerializeField] Text pathTitle;
    void LoadLevel() // run only once
    {
        CurrentLevel cur = FindObjectOfType<CurrentLevel>();

        string rootPath = Application.persistentDataPath + "/maps";

        title.text = cur.levelName;
        pathTitle.text = rootPath + "/" + cur.levelName;

        if (!cur.newLevel)
        {
            GetComponent<MapSaver>().LoadMapInEditor();
            CreateGrid();
        }
        else
        {
            // default params
            gridSizeX = 9;
            gridSizeY = 13;

            for (int i = 0; i < gemPrefab.Length; i++)
            {
                usedColors.Add(i);
            }

            CreateGrid();

            FindObjectOfType<MapParameters>().OpenParamsWindow(true);
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void PlayMap()
    {
        SceneManager.LoadScene("Game Scene");
    }

    public void ClearMap()
    {
        FindObjectOfType<CurrentLevel>().newLevel = true;
        LoadLevel();
    }

    public void ReloadLevel()
    {
        CurrentLevel cur = FindObjectOfType<CurrentLevel>();
        
        if (SaveSystem.MapExists(cur.levelName))
        {
            cur.newLevel = false;
            LoadLevel();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSizeX, gridSizeY, 1));
    }

}
