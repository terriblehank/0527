using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISaveListPanle : MonoBehaviour
{
    public UISaveGridChild selectedChild;
    public UIScrollView scrollView;
    public UIGrid grid;
    public GameObject saveGridChildPfb;

    // Start is called before the first frame update
    void Start()
    {
        InitSaves();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }

    public void InitSaves()
    {
        string[] saves = SaveHelper.Instance.GetAllSave();
        foreach (var savePath in saves)
        {
            UISaveGridChild newChild = Instantiate(saveGridChildPfb, grid.transform).GetComponent<UISaveGridChild>();
            newChild.saveListPanle = this;
            newChild.SetPath(savePath);
        }
        StartCoroutine(GridReposition());
    }

    public void AddSaveChild(string saveName)
    {
        SaveHelper.CreateSaveCallback callback = (string savePath) =>
        {
            UISaveGridChild newChild = Instantiate(saveGridChildPfb, grid.transform).GetComponent<UISaveGridChild>();
            newChild.saveListPanle = this;
            newChild.SetPath(savePath);
            StartCoroutine(GridReposition(1));
        };
        // 需要锁住面板，禁用操作，等待创建结束

        //代码等待补齐

        //end
        StartCoroutine(SaveHelper.Instance.CreateSave(saveName, callback));
    }

    public void DelSaveChild(UISaveGridChild child)
    {
        SaveHelper.Instance.DeleteSave(child.filePath);

        int index = child.transform.GetSiblingIndex();

        Destroy(child.gameObject);

        StartCoroutine(MoveTheFocus(index));

        StartCoroutine(GridReposition(0));
    }

    /// <summary>
    /// 移除子对象后调用，更换选中的对象
    /// </summary>
    /// <param name="index"></param>
    IEnumerator MoveTheFocus(int index)
    {
        yield return null;
        if (grid.transform.childCount <= 0)
        {
            selectedChild = null;
            yield break;
        }
        if (index >= grid.transform.childCount)
        {
            index = grid.transform.childCount - 1;
        }
        Transform child = grid.transform.GetChild(index);
        if (child != null)
        {
            child.GetComponent<UISaveGridChild>().Select(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scrollViewSet">1 : set to the bottom , 2 : set to the top</param>
    /// <returns></returns>
    IEnumerator GridReposition(int scrollViewSet = 0)
    {
        yield return null;
        grid.Reposition();
        yield return null;
        if (scrollViewSet == 1)
        {
            scrollView.Scroll(-1 * 100 / scrollView.scrollWheelFactor);
        }
        if (scrollViewSet == 2)
        {
            scrollView.Scroll(1 * 100 / scrollView.scrollWheelFactor);
        }
        scrollView.UpdateScrollbars();
        scrollView.UpdatePosition();
    }
}
