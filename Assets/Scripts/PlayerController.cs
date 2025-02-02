using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    GridManager grid;
    LevelManager levelManager;
    Camera cam;
    void Start()
    {
        grid = GetComponent<GridManager>();
        levelManager = GetComponent<LevelManager>();
        cam = Camera.main;
    }

    bool firstClick = false;
    bool firstDrag = false;
    Node firstNode, secondNode;
    List<Node> neighbors;

    public bool controlEnabled = true;
    void Update()
    {
        if (!controlEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            if (!firstClick)
            {
                firstNode = grid.NodeFromWorldPoint(mousePos, false);
                if (firstNode == null)
                {
                    return;
                }

                firstDrag = true;
                firstClick = true;
                firstNode.gem.transform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                neighbors = grid.GetNeighbors(firstNode);
                secondNode = grid.NodeFromWorldPoint(mousePos, false);
                if (secondNode == null)
                {
                    return;
                }

                firstNode.gem.transform.localScale = Vector3.one;

                if (secondNode == firstNode)
                {
                    firstClick = false;
                    return;
                }
                if (neighbors.Contains(secondNode))
                {
                    StartCoroutine(SwapTiles());

                    firstClick = false;
                }
                else
                {
                    secondNode.gem.transform.localScale = Vector3.one * 1.5f;
                    firstNode = secondNode;
                    firstDrag = true;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                firstDrag = false;
            }
            if (firstDrag)
            {
                Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                secondNode = grid.NodeFromWorldPoint(mousePos, true);
                
                if (secondNode != firstNode)
                {
                    neighbors = grid.GetNeighbors(firstNode);
                    if (neighbors.Contains(secondNode))
                    {
                        firstNode.gem.transform.localScale = Vector3.one;
                        StartCoroutine(SwapTiles());

                        firstClick = false;
                        firstDrag = false;
                    }
                }
            }
        }
    }

    IEnumerator SwapTiles()
    {
        controlEnabled = false;

        Gem tempGem = firstNode.gem;
        firstNode.gem = secondNode.gem;
        secondNode.gem = tempGem;

        float t = 0;
        float swapDur = 0.15f;

        Vector3 firstPos, secondPos;
        firstPos = firstNode.gem.transform.position;
        secondPos = secondNode.gem.transform.position;

        while (t < swapDur)
        {
            t += Time.deltaTime;
            firstNode.gem.transform.position = firstPos + t / swapDur * (firstNode.worldPosition - firstPos);
            secondNode.gem.transform.position = secondPos + t / swapDur * (secondNode.worldPosition - secondPos);
            yield return null;
        }
        firstNode.gem.transform.position = firstNode.worldPosition;
        secondNode.gem.transform.position = secondNode.worldPosition;
        
        if (!grid.CheckForLines())
        {
            tempGem = firstNode.gem;
            firstNode.gem = secondNode.gem;
            secondNode.gem = tempGem;

            t = 0;

            firstPos = firstNode.gem.transform.position;
            secondPos = secondNode.gem.transform.position;

            while (t < swapDur)
            {
                t += Time.deltaTime;
                firstNode.gem.transform.position = firstPos + t / swapDur * (firstNode.worldPosition - firstPos);
                secondNode.gem.transform.position = secondPos + t / swapDur * (secondNode.worldPosition - secondPos);
                yield return null;
            }
            firstNode.gem.transform.position = firstNode.worldPosition;
            secondNode.gem.transform.position = secondNode.worldPosition;
            
            controlEnabled = true;
        } else
        {
            levelManager.UpdateMoveCount();
        }
    }
    // states:
    // Idle, first tile selected, second tile selected
}
