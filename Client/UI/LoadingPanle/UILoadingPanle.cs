using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILoadingPanle : MonoBehaviour
{
    public UIProgressBar bar;
    public UILabel label;
    public static string loadingPanleTagName = "LoadingPanle";

    public static UILoadingPanle ShowLoadingPanle()
    {
        if (GameObject.FindGameObjectWithTag(loadingPanleTagName) != null)
        {
            throw new Exception("UILoadingPanle已经存在，请勿重复调用");
        }
        GameObject loadingPanlePrefab = Resources.Load<GameObject>("Prefabs/UI/LoadingPanle/Loading");
        return Instantiate(loadingPanlePrefab, GameObject.FindGameObjectWithTag("UIRoot").transform).GetComponent<UILoadingPanle>();
    }

    public IEnumerator AsyncRefresh()
    {
        while (!UILoadingDataShare.IsDone)
        {
            label.text = UILoadingDataShare.loadingText;
            bar.value = UILoadingDataShare.loadingProgress;
            yield return null;
        }
        Close();
    }

    private void Start()
    {
        StartCoroutine(AsyncRefresh());
    }

    /// <summary>
    /// 慎用
    /// </summary>
    public void Close()
    {
        StartCoroutine(ReadyToClose());
    }

    IEnumerator ReadyToClose()
    {
        bar.value = 1;
        label.text = "准备完毕！";
        yield return new WaitForSeconds(0.5f);
        if (UILoadingDataShare.onDone != null)
        {
            Action callback = UILoadingDataShare.onDone;
            UILoadingDataShare.Reset();
            Debug.Log("invoke");
            callback.Invoke();
        }
        else
        {
            throw new Exception("On done callback is null");
        }
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}

public class UILoadingDataShare
{
    public static string loadingText = "";
    public static float loadingProgress = 0;

    public static bool IsDone { get; private set; } = false;

    public static Action onDone;

    public static void Reset()
    {
        loadingText = "";
        loadingProgress = 0;
        IsDone = false;
        onDone = null;
        Debug.Log("Loading data reset.");
    }

    public static void SetLoaidngText(string _text)
    {
        loadingText = _text;
        Debug.Log(_text);
    }

    public static void SetLoadingProgress(float _loadingProgress)
    {
        loadingProgress = _loadingProgress;
    }

    public static void Done()
    {
        IsDone = true;
    }


}