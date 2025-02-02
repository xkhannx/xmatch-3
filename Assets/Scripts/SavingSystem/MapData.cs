using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData 
{
    public string mapName;
    public bool playable;

    public int gridX;
    public int gridY;
    public int[] usedColors;
    public int[] colorWinCounts;

    public int[] floorX;
    public int[] floorY;

    public MapData(string _mapName, int _gridX, int _gridY, int[] _usedColors, int[] _floorX, int[] _floorY)
    {
        mapName = _mapName;
        playable = false;

        gridX = _gridX;
        gridY = _gridY;
        usedColors = _usedColors;

        floorX = _floorX;
        floorY = _floorY;
    }
}
