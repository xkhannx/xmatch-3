using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] Gem[] gemPrefabs;
    [SerializeField] Gem sunPrefab;
    [SerializeField] GameObject floorPrefab;

    [Header("Grid gen parameters:")]
    public int gridSizeX, gridSizeY;
    float nodeRadius = 0.5f;
    public Node[,] grid;

    public List<int> usedColors = new List<int>();
    List<Gem> availableGems = new List<Gem>();
    PlayerController player;
    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

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
        List<Gem> usedGems = new List<Gem>();
        List<Gem> unusedGems = new List<Gem>();

        foreach(int i in usedColors)
        {
            unusedGems.Add(gemPrefabs[i]);
            availableGems.Add(gemPrefabs[i]);
        }
        
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                if (grid[i, j].walkable)
                {            
                    foreach (Node neighbor in GetNeighbors(grid[i, j]))
                        if (neighbor.gem != null)
                        {
                            foreach (var gemPrefab in unusedGems)
                            {
                                if (gemPrefab.gemColor == neighbor.gem.gemColor)
                                {
                                    usedGems.Add(gemPrefab);
                                }
                            }
                            foreach (var gemPrefab in usedGems)
                            {
                                unusedGems.Remove(gemPrefab);
                            }
                        }

                    int randInd = Random.Range(0, unusedGems.Count);
                    grid[i, j].gem = Instantiate(unusedGems[randInd], grid[i, j].worldPosition, Quaternion.identity, transform);

                    foreach (var t in usedGems)
                    {
                        unusedGems.Add(t);
                    }
                    usedGems.Clear();

                    if (grid[i, j].walkable)
                        grid[i, j].floorTile = Instantiate(floorPrefab, grid[i, j].worldPosition, Quaternion.identity, transform);
                }
            }
        }
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((x + y) % 2 == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkY >= 0 && checkX < gridSizeX && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition, bool clampGrid)
    {
        float percentX = (worldPosition.x + (float) gridSizeX / 2) / (float) gridSizeX;
        float percentY = (worldPosition.y + (float) gridSizeY / 2) / (float) gridSizeY;

        if (!clampGrid)
        {
            if (percentX < 0 || percentX > 1 || percentY < 0 || percentY > 1)
                return null;
        }
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        if (percentX == 1) percentX = 0.99f;
        if (percentY == 1) percentY = 0.99f;

        int x = Mathf.FloorToInt(gridSizeX * percentX);
        int y = Mathf.FloorToInt(gridSizeY * percentY);

        return grid[x, y];
    }


    List<Node> gemsToDestroy = new List<Node>();
    List<Node> sunTiles = new List<Node>();
    public bool CheckForLines()
    {
        gemsToDestroy.Clear();

        List<Node> longSuns = new List<Node>();

        bool lineFound = false;
        // Vertical
        for (int i = 0; i < gridSizeX; i++)
        {
            List<Node> streak = new List<Node>();
            streak.Add(grid[i, 0]);
            Node.GemColor curColor = grid[i, 0].gem.gemColor;
            
            for (int j = 1; j < gridSizeY; j++)
            {
                if (grid[i, j].gem.gemColor == curColor || grid[i, j].gem.gemColor == Node.GemColor.Sun)
                {
                    streak.Add(grid[i, j]);
                }
                else
                {
                    if (streak.Count >= 3)
                    {
                        lineFound = true;
                        foreach (Node n in streak)
                            gemsToDestroy.Add(n);

                        if (streak.Count >= 5)
                        {
                            longSuns.Add(streak[(streak.Count + 1) / 2 - 1]);
                        }
                    }
                    streak.Clear();
                    streak.Add(grid[i, j]);

                    if (grid[i, j-1].gem.gemColor == Node.GemColor.Sun)
                        streak.Add(grid[i, j - 1]);
                    
                    curColor = grid[i, j].gem.gemColor;
                }
            }

            if (streak.Count >= 3)
            {
                lineFound = true;
                foreach (Node n in streak)
                    gemsToDestroy.Add(n);

                if (streak.Count >= 5)
                {
                    longSuns.Add(streak[(streak.Count + 1) / 2 - 1]);
                }
            }
            streak.Clear();
        }

        // Horizontal
        for (int j = 0; j < gridSizeY; j++)
        {
            List<Node> streak = new List<Node>();
            streak.Add(grid[0, j]);
            Node.GemColor curColor = grid[0, j].gem.gemColor;

            for (int i = 1; i < gridSizeX; i++)
            {
                if (grid[i, j].gem.gemColor == curColor || grid[i, j].gem.gemColor == Node.GemColor.Sun)
                {
                    streak.Add(grid[i, j]);
                }
                else
                {
                    if (streak.Count >= 3)
                    {
                        lineFound = true;
                        foreach (Node n in streak)
                            gemsToDestroy.Add(n);
                        
                        if (streak.Count >= 5)
                        {
                            longSuns.Add(streak[(streak.Count + 1) / 2 - 1]);
                        }
                    }
                    streak.Clear();
                    streak.Add(grid[i, j]);

                    if (grid[i - 1, j].gem.gemColor == Node.GemColor.Sun)
                        streak.Add(grid[i - 1, j]);

                    curColor = grid[i, j].gem.gemColor;
                }
            }

            if (streak.Count >= 3)
            {
                lineFound = true;
                foreach (Node n in streak)
                    gemsToDestroy.Add(n);
                
                if (streak.Count >= 5)
                {
                    longSuns.Add(streak[(streak.Count + 1) / 2 - 1]);
                }
            }
            streak.Clear();
        }

        List<Node> sunNodes = new List<Node>();
        foreach (Node sun in grid)
        {
            if (sun.gem.gemColor == Node.GemColor.Sun)
                sunNodes.Add(sun);
        }

        // Sun diagonals
        lineFound = CheckExistingSunDiagonals(lineFound, sunNodes);

        CheckForNewSun(longSuns);

        if (lineFound)
            StartCoroutine(DestroyGems());

        return lineFound;
    }

    private bool CheckExistingSunDiagonals(bool lineFound, List<Node> sunNodes)
    {
        foreach (Node sun in sunNodes)
        {
            bool diagFound = false;

            Vector2Int[] diags = new Vector2Int[4];
            diags[0] = new Vector2Int(-1, 1);
            diags[1] = new Vector2Int(1, 1);
            diags[2] = new Vector2Int(1, -1);
            diags[3] = new Vector2Int(-1, -1);

            List<Node>[] streaks = new List<Node>[4];

            for (int i = 0; i < diags.Length; i++)
            {
                streaks[i] = new List<Node>();
                Node curNode = sun;
                while (true)
                {
                    int x = Mathf.Clamp(curNode.gridX + diags[i].x, 0, gridSizeX - 1);
                    int y = Mathf.Clamp(curNode.gridY + diags[i].y, 0, gridSizeY - 1);

                    if (x != curNode.gridX + diags[i].x || y != curNode.gridY + diags[i].y)
                    {
                        break;
                    }

                    curNode = grid[x, y];
                    if (streaks[i].Count == 0)
                    {
                        streaks[i].Add(grid[x, y]);
                    }
                    else
                    {
                        if (grid[x, y].gem.gemColor == streaks[i][0].gem.gemColor)
                            streaks[i].Add(grid[x, y]);
                        else
                            break;
                    }
                }

                if (streaks[i].Count >= 2)
                {
                    lineFound = true;
                    diagFound = true;
                    foreach (Node diagNode in streaks[i])
                    {
                        gemsToDestroy.Add(diagNode);
                    }
                }

                if (i > 1)
                {
                    if (streaks[i].Count > 0 && streaks[i - 2].Count > 0 && streaks[i][0].gem.gemColor == streaks[i - 2][0].gem.gemColor)
                    {
                        lineFound = true;
                        diagFound = true;
                        if (streaks[i].Count == 1)
                            gemsToDestroy.Add(streaks[i][0]);
                        if (streaks[i - 2].Count == 1)
                            gemsToDestroy.Add(streaks[i - 2][0]);
                    }
                }
            }
            if (diagFound)
            {
                gemsToDestroy.Add(sun);
            }
        }

        return lineFound;
    }

    private void CheckForNewSun(List<Node> longSuns)
    {
        List<Node> uniqGems = gemsToDestroy.Distinct().ToList();
        foreach (Node uGem in uniqGems)
        {
            gemsToDestroy.Remove(uGem);
        }
        sunTiles = gemsToDestroy;
        gemsToDestroy = uniqGems;

        foreach (Node n in longSuns)
        {
            if (!sunTiles.Contains(n))
                sunTiles.Add(n);
        }

        foreach (Node sun in sunTiles)
        {
            if (sun.gem.gemColor != Node.GemColor.Sun)
            {
                // Create new sun
                gemsToDestroy.Remove(sun);
                Destroy(sun.gem.gameObject);

                sun.gem = Instantiate(sunPrefab, sun.worldPosition, Quaternion.identity, transform);
            }
        }
    }

    IEnumerator DestroyGems()
    {

        float t = 0;
        float destroyTime = 0.2f;

        while (t < destroyTime)
        {
            t += Time.deltaTime;
            foreach (Node g in gemsToDestroy)
            {
                g.gem.transform.localScale = (1 + t / destroyTime) * Vector3.one;
                Color gemTrans = Color.white;
                gemTrans.a = 1 - t / destroyTime;
                g.gem.GetComponent<SpriteRenderer>().color = gemTrans;
            }
            yield return null;
        }

        foreach (Node node in gemsToDestroy)
        {
            if (node.gem != null)
            {
                Destroy(node.gem.gameObject);
                node.gem = null;
            }
        }
        gemsToDestroy.Clear();

        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FillHoles());
    }

    IEnumerator FillHoles()
    {
        // Gravity fall
        List<Node> fallingGems = new List<Node>();
        for (int i = 0; i < gridSizeX; i++)
        {
            int holeCounter = 0;
            for (int j = 0; j < gridSizeY; j++)
            {
                if (grid[i, j].gem == null)
                {
                    holeCounter++;
                }
                else 
                { 
                    if (holeCounter > 0)
                    {
                        fallingGems.Add(grid[i, j - holeCounter]);
                        grid[i, j - holeCounter].gem = grid[i, j].gem;
                        grid[i, j].gem = null;
                    }

                }
            }
        }
        
        float t = 0;
        float fallTime = 0.2f;
        List<Vector3> oldPos = new List<Vector3>();
        foreach (Node node in fallingGems)
        {
            oldPos.Add(node.gem.transform.position);
        }
        while (t < fallTime)
        {
            t += Time.deltaTime;
            for (int i = 0; i < fallingGems.Count; i++)
            {
                fallingGems[i].gem.transform.position = oldPos[i] + (fallingGems[i].worldPosition - oldPos[i]) * t / fallTime;
            }
            yield return null;
        }

        foreach (Node node in fallingGems)
        {
            node.gem.transform.position = node.worldPosition;
        }

        // Add new tiles
        List<Node> newTiles = new List<Node>();
        foreach (Node node in grid)
        {
            if (node.gem == null)
            {
                int randInd = Random.Range(0, availableGems.Count);
                node.gem = Instantiate(availableGems[randInd], node.worldPosition, Quaternion.identity, transform);
                newTiles.Add(node);
            }
        }

        t = 0;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            foreach (Node node in newTiles)
            {
                node.gem.transform.localScale = Vector3.one * t / fallTime;
            }
            yield return null;
        }

        foreach (Node node in newTiles)
        {
            node.gem.transform.localScale = Vector3.one;
        }
        // Check again
        if (!CheckForLines())
        {
            /*
            PotentialMatches();
            if (hintMatch.Count == 0)
            {
                // GAME OVER
            }*/
            player.controlEnabled = true;
        }
    }

    void PotentialMatches()
    {
        hintMatch.Clear();
        // Vertical
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 2; j < gridSizeY; j++)
            {
                if (CheckPotentialMatch(grid[i, j - 2], grid[i, j - 1], grid[i, j]) ||
                    CheckPotentialMatch(grid[i, j - 2], grid[i, j], grid[i, j - 1]) ||
                    CheckPotentialMatch(grid[i, j], grid[i, j - 1], grid[i, j - 2])) 
                    return;
            }
        }
        // Horizontal
        for (int j = 0; j < gridSizeY; j++)
        {
            for (int i = 2; i < gridSizeX; i++)
            {
                if (CheckPotentialMatch(grid[i - 2, j], grid[i - 1, j], grid[i, j]) ||
                    CheckPotentialMatch(grid[i - 2, j], grid[i, j], grid[i - 1, j]) ||
                    CheckPotentialMatch(grid[i, j], grid[i - 1, j], grid[i - 2, j]))
                    return;
            }
        }
    }

    List<Node> hintMatch = new List<Node>();
    bool CheckPotentialMatch(Node first, Node second, Node third)
    {
        if (first.gem.gemColor == second.gem.gemColor)
        {
            List<Node> neighbors = GetNeighbors(third);

            if (neighbors.Contains(first)) neighbors.Remove(first);
            if (neighbors.Contains(second)) neighbors.Remove(second);

            foreach(Node n in neighbors)
            {
                if (n.gem.gemColor == first.gem.gemColor)
                {
                    hintMatch.Clear();
                    hintMatch.Add(first);
                    hintMatch.Add(second);
                    hintMatch.Add(third);
                    hintMatch.Add(n);
                    return true;
                }
            }
        }
        return false;
    }

    public void ShowHint()
    {
        if (hintButtonOff || !player.controlEnabled) return;

        PotentialMatches();
        if (hintMatch.Count == 0) return;

        StartCoroutine(FlashHint());
    }

    [SerializeField] AnimationCurve boink;
    bool hintButtonOff = false;
    IEnumerator FlashHint()
    {
        player.controlEnabled = false;
        hintButtonOff = true;

        float t = 0;
        float boinkTime = 0.5f;

        while (t < boinkTime)
        {
            foreach (Node n in hintMatch)
            {
                n.gem.transform.localScale = Vector3.one * (boink.Evaluate(t / boinkTime) + 1);
            }
            yield return null;

            t += Time.deltaTime;
        }

        player.controlEnabled = true;
        hintButtonOff = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSizeX, gridSizeY, 1));
        if (grid != null)
        {
            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    if (grid[i, j].walkable)
                    {
                        Gizmos.color = Color.white;
                        if (grid[i, j].gem != null)
                        {
                            switch (grid[i, j].gem.gemColor)
                            {
                                case Node.GemColor.Red:
                                    Gizmos.color = Color.red;
                                    break;
                                case Node.GemColor.Blue:
                                    Gizmos.color = Color.blue;
                                    break;
                                case Node.GemColor.Green:
                                    Gizmos.color = Color.green;
                                    break;
                                case Node.GemColor.Orange:
                                    Gizmos.color = Color.cyan;
                                    break;
                                case Node.GemColor.Purple:
                                    Gizmos.color = Color.magenta;
                                    break;
                                case Node.GemColor.Yellow:
                                    Gizmos.color = Color.yellow;
                                    break;
                            }
                        }
                        Gizmos.DrawWireSphere(grid[i, j].worldPosition, nodeRadius);
                    }
                    else
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawWireSphere(grid[i, j].worldPosition, nodeRadius);
                    }
                }
            }
        }
    }
}
