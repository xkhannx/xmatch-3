using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapParameters : MonoBehaviour
{
    [SerializeField] InputField inputX, inputY;
    [Header("Buttons")]
    [SerializeField] RectTransform[] gemButtons;
    MapEditor grid;

    private void Start()
    {
        grid = FindObjectOfType<MapEditor>();
        for (int i = 0; i < gemButtons.Length; i++)
        {
            gemButtons[i].GetComponentsInChildren<Image>()[1].sprite = grid.gemPrefab[i].GetComponent<SpriteRenderer>().sprite;
        }
    }

    List<int> usedColors;
    public void OpenParamsWindow(bool turnOn)
    {
        if (turnOn)
        {
            OpenSavesWindow(false);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            
            inputX.text = grid.gridSizeX.ToString();
            inputY.text = grid.gridSizeY.ToString();

            usedColors = new List<int>(grid.usedColors);
            for (int i = 0; i < gemButtons.Length; i++)
            {
                gemButtons[i].localScale = usedColors.Contains(i) ? Vector3.one : 0.75f * Vector3.one;
            }
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition = Vector2.left * 2000;
        }
    }

    public void ApplyParams()
    {
        int gridX = System.Int32.Parse(inputX.text);
        int gridY = System.Int32.Parse(inputY.text);
        
        grid.gridSizeX = Mathf.Clamp(gridX, 3, 9);
        grid.gridSizeY = Mathf.Clamp(gridY, 3, 13);

        grid.usedColors = new List<int>(usedColors);

        grid.CreateGrid();
        GetComponent<RectTransform>().anchoredPosition = Vector2.left * 2000;
    }

    public void AddColor(int colorId)
    {
        if (usedColors.Contains(colorId))
        {
            if (usedColors.Count > 3)
            {
                usedColors.Remove(colorId);
                gemButtons[colorId].localScale = 0.75f * Vector3.one;
            }
        } else
        {
            usedColors.Add(colorId);
            gemButtons[colorId].localScale = Vector3.one;
        }
    }

    [SerializeField] RectTransform savesWindow;
    public void OpenSavesWindow(bool turnOn)
    {
        if (turnOn)
        {
            OpenParamsWindow(false);
            savesWindow.anchoredPosition = Vector2.zero;
        } else
        {
            savesWindow.anchoredPosition = Vector2.left * 2500;
        }
    }
}
