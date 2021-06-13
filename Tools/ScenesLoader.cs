using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesLoader : SingletonMono<ScenesLoader>
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static void LoadMainScene()
    {
        SceneManager.LoadScene(1);
    }
}
