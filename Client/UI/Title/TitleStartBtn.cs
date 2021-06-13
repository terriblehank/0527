using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleStartBtn : MonoBehaviour
{
    public UIButton titleStartBtn;
    public UISaveListPanle saveListPanle;

    void Start()
    {
        UIEventListener.Get(gameObject).onClick = OnClickBtn;
    }


    public void OnClickBtn(GameObject go)
    {
        saveListPanle.gameObject.SetActive(true);
    }
}
