using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class MapSaver : MonoBehaviour
{
    int[] floorX;
    int[] floorY;
    int[] usedColors;
    public void SaveMap()
    {
        FindObjectOfType<CurrentLevel>().newLevel = false;
        MapEditor mapEditor = FindObjectOfType<MapEditor>();

        usedColors = new int[mapEditor.usedColors.Count];
        for (int i = 0; i < mapEditor.usedColors.Count; i++)
        {
            usedColors[i] = mapEditor.usedColors[i];
        }
        List<Node> floorNodes = new List<Node>();

        //for (int i = 0; i < mapEditor.gridSizeX; i++)
        //{
        //    for (int j = 0; j < mapEditor.gridSizeY; j++)
        //    {
        //        if (mapEditor.grid[i, j].walkable)
        //        {
        //            floorNodes.Add(mapEditor.grid[i, j]);
        //        }
        //    }
        //}

        floorX = new int[floorNodes.Count];
        floorY = new int[floorNodes.Count];

        //for (int i = 0; i < floorNodes.Count; i++)
        //{
        //    floorX[i] = floorNodes[i].gridX;
        //    floorY[i] = floorNodes[i].gridY;
        //}

        string mapName = FindObjectOfType<CurrentLevel>().levelName;
        MapData mapSave = new MapData(mapName, mapEditor.gridSizeX, mapEditor.gridSizeY, usedColors, floorX, floorY);
        //mapSave.playable = floorNodes.Count > 0 && pointNodes.Count > 0 && spawnNodes.Count > 0;
        
        SaveSystem.SaveMap(mapSave);
    }

    public void LoadMapInEditor()
    {
        string levelName = FindObjectOfType<CurrentLevel>().levelName;

        MapEditor mapEditor = FindObjectOfType<MapEditor>();

        MapData loadedMap = SaveSystem.Load(levelName);
        if (loadedMap == null) return;

        mapEditor.gridSizeX = loadedMap.gridX;
        mapEditor.gridSizeY = loadedMap.gridY;
        mapEditor.usedColors = loadedMap.usedColors.ToList<int>();

        //floorX = loadedMap.floorX;
        //floorY = loadedMap.floorY;
        //for (int i = 0; i < floorX.Length; i++)
        //{
        //    mapEditor.grid[floorX[i], floorY[i]].walkable = true;
        //}
        
        //mapEditor.ConstructMap();
    }

    public void LoadMapInGame()
    {
        string levelName = FindObjectOfType<CurrentLevel>().levelName;

        GridManager grid = FindObjectOfType<GridManager>();

        MapData loadedMap = SaveSystem.Load(levelName);
        if (loadedMap == null) return;

        grid.gridSizeX = loadedMap.gridX;
        grid.gridSizeY = loadedMap.gridY;
        grid.usedColors = loadedMap.usedColors.ToList<int>();
    }

}
