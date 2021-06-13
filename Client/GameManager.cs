using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static bool isEditorPause = false;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += EditorPause;
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
#if UNITY_EDITOR
    private static void EditorPause(PauseState state)
    {
        if (Server.Instance.Inited)
        {
            if (state == PauseState.Paused)
            {
                isEditorPause = true;
                Server.Instance.Pause();
            }
            else
            {
                isEditorPause = false;
                Server.Instance.Resume();
            }
        }
    }
#endif

    private void OnApplicationQuit()
    {
        Server.Instance.Stop();
        Debug.Log("#ÍË³ö#");
    }

    private void OnApplicationPause(bool pause)
    {
        if (Server.Instance.Inited)
        {
            if (pause)
            {
                Server.Instance.Pause();
            }
            else
            {
                if (isEditorPause) return;
                Server.Instance.Resume();
            }
        }
    }
}
