using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterBtn : MonoBehaviour
{
    public UISaveListPanle saveListPanle;
    public UIButton enterBtn;

    void Start()
    {
        UIEventListener.Get(gameObject).onClick = OnClickBtn;
    }

    // Update is called once per frame
    void Update()
    {
        if (saveListPanle.selectedChild != null)
        {
            enterBtn.isEnabled = true;
        }
        else
        {
            enterBtn.isEnabled = false;
        }
    }

    public void OnClickBtn(GameObject go)
    {
        if (saveListPanle.selectedChild != null)
        {
            Debug.Log("进入选择的存档");
            #region 设置世界大小和生成player的数量
            int size;
            int.TryParse(GameObject.Find("MapSize").GetComponent<UIInput>().value, out size);
            Server.Instance.worldCtrl.WorldSize = size;
            int count;
            int.TryParse(GameObject.Find("PlayerCount").GetComponent<UIInput>().value, out count);
            Server.Instance.spawnPlayerNum = count;
            #endregion


            UILoadingPanle.ShowLoadingPanle();//显示加载界面


            Server.Instance.InitAsync(saveListPanle.selectedChild.filePath); //开始异步加载“服务器”数据，当服务器数据加载完毕后才会切换场景
        }
    }
}
