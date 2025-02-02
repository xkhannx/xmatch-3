using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentLevel : MonoBehaviour
{
    public string levelName;
    public bool newLevel;
    private void Awake()
    {
        if (FindObjectsOfType<CurrentLevel>().Length > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }
}
