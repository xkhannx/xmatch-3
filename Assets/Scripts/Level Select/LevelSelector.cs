using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] RectTransform levelButtonPrefab;
    [SerializeField] RectTransform levelOptions;
    [SerializeField] RectTransform renameWindow;
    [SerializeField] RectTransform deleteWindow;
    string[] mapNames;
    LevelButton[] levels;

    void Start()
    {
        LoadFiles();
    }

    // Level Options 
    int currentLevelID;
    public void OpenLevelOptions(int buttonId)
    {
        bool buttonsActive;
        currentLevelID = buttonId;
        if (buttonId == -1)
        {
            levelOptions.anchoredPosition = Vector2.left * 2000;
            buttonsActive = true;
        }
        else 
        {
            if (buttonId == -2)
            {
                RenameWindow(true);
                buttonsActive = false;
            }
            else
            {
                levelOptions.anchoredPosition = Vector2.zero;

                string currentLevelName = levels[buttonId].GetComponentInChildren<Text>().text;
                levelOptions.GetComponentInChildren<Text>().text = currentLevelName;
                buttonsActive = false;
            }
        }

        foreach (LevelButton b in levels)
        {
            b.buttonActive = buttonsActive;
        }
    }

    public void RenameWindow(bool turnOn)
    {
        if (turnOn)
        {
            renameWindow.anchoredPosition = Vector2.zero;
            levelOptions.anchoredPosition = Vector2.left * 2000;

            if (currentLevelID >= 0)
            {
                renameWindow.GetComponentInChildren<InputField>().text = levels[currentLevelID].GetComponentInChildren<Text>().text;
                renameWindow.GetComponentInChildren<InputField>().Select();
            }
            else
            {
                OpenLevelOptions(-1);
                renameWindow.GetComponentInChildren<InputField>().text = "";
            }
        }
        else
        {
            if (currentLevelID >= 0)
            {
                OpenLevelOptions(currentLevelID);
            }
            renameWindow.anchoredPosition = Vector2.left * 2500;
        }
    }

    public void RenameConfirm()
    {
        if (currentLevelID >= 0)
        {
            string newName = renameWindow.GetComponentInChildren<InputField>().text;

            string path = Application.persistentDataPath + "/maps/" + levels[currentLevelID].GetComponentInChildren<Text>().text;
            string newPath = Application.persistentDataPath + "/maps/" + newName;

            levels[currentLevelID].GetComponentInChildren<Text>().text = newName;

            File.Move(path, newPath);
            renameWindow.anchoredPosition = Vector2.left * 2500;
        }
        else // new level
        {
            EditLevel(true);
        }

        OpenLevelOptions(currentLevelID);
    }

    public void EditLevel(bool newLevel)
    {
        if (newLevel && renameWindow.GetComponentInChildren<InputField>().text.Length == 0)
        {
            return;
        }

        FindObjectOfType<CurrentLevel>().newLevel = newLevel;

        string newName;
        if (newLevel)
        {
            newName = renameWindow.GetComponentInChildren<InputField>().text;
        } else
        {
            newName = levels[currentLevelID].GetComponentInChildren<Text>().text;
        }
        FindObjectOfType<CurrentLevel>().levelName = newName;
        SceneManager.LoadScene("Level editor");
    }

    public void DeleteWindow (bool delete)
    {
        if (delete)
        {
            deleteWindow.anchoredPosition = Vector2.zero;
            levelOptions.anchoredPosition = Vector2.left * 2000;
        }
        else
        {
            OpenLevelOptions(currentLevelID);
            deleteWindow.anchoredPosition = Vector2.left * 3500;
        }
    }

    public void DeleteConfirm()
    {
        string path = Application.persistentDataPath + "/maps/" + levels[currentLevelID].GetComponentInChildren<Text>().text;
        File.Delete(path);

        deleteWindow.anchoredPosition = Vector2.left * 3500;
        OpenLevelOptions(-1);
        LoadFiles();
    }

    public void PlayLevel()
    {
        FindObjectOfType<CurrentLevel>().newLevel = false;
        FindObjectOfType<CurrentLevel>().levelName = levels[currentLevelID].GetComponentInChildren<Text>().text;
        SceneManager.LoadScene("Game scene");
    }

    // Initiate menu
    public void LoadFiles()
    {
        string path = Application.persistentDataPath + "/maps";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        DirectoryInfo info = new DirectoryInfo(path);
        FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();

        mapNames = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            mapNames[i] = files[i].FullName;
        }

        if (levels != null)
        {
            foreach (LevelButton button in levels)
            {
                Destroy(button.gameObject);
            }
        }
        levels = new LevelButton[mapNames.Length + 1];

        for (int i = 0; i < mapNames.Length; i++)
        {
            string temp = mapNames[i].Substring(path.Length + 1, mapNames[i].Length - path.Length - 1);
            mapNames[i] = temp;
        }

        RectTransform newButton;
        for (int i = 0; i < mapNames.Length; i++)
        {
            newButton = Instantiate(levelButtonPrefab, transform);
            newButton.anchoredPosition = new Vector2(0, -50 - i * 120);
            newButton.GetComponentInChildren<Text>().text = mapNames[i];
            levels[i] = newButton.GetComponent<LevelButton>();
            levels[i].buttonActive = true;
            levels[i].buttonId = i;
        }

        newButton = Instantiate(levelButtonPrefab, transform);
        newButton.anchoredPosition = new Vector2(0, -50 - mapNames.Length * 120);
        newButton.GetComponentInChildren<Text>().text = "New level";
        levels[mapNames.Length] = newButton.GetComponent<LevelButton>();
        levels[mapNames.Length].buttonActive = true;
        levels[mapNames.Length].buttonId = -2;
    }
}
