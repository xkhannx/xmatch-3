using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    
    public bool walkable;
    
    public enum GemColor { Red, Green, Blue, Yellow, Purple, Orange, Sun }
    public Gem gem;
    public GameObject floorTile;
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        walkable = _walkable;
    }
}
