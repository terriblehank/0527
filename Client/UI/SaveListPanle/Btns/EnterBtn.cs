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
            Debug.Log("����ѡ��Ĵ浵");
            #region ���������С������player������
            int size;
            int.TryParse(GameObject.Find("MapSize").GetComponent<UIInput>().value, out size);
            Server.Instance.worldCtrl.WorldSize = size;
            int count;
            int.TryParse(GameObject.Find("PlayerCount").GetComponent<UIInput>().value, out count);
            Server.Instance.spawnPlayerNum = count;
            #endregion


            UILoadingPanle.ShowLoadingPanle();//��ʾ���ؽ���


            Server.Instance.InitAsync(saveListPanle.selectedChild.filePath); //��ʼ�첽���ء������������ݣ������������ݼ�����Ϻ�Ż��л�����
        }
    }
}
