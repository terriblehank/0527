using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteSaveBtn : MonoBehaviour
{
    public UISaveListPanle saveListPanle;
    public UIButton deleteSaveBtn;

    void Start()
    {
        UIEventListener.Get(gameObject).onClick = OnClickBtn;
    }

    // Update is called once per frame
    void Update()
    {
        if (saveListPanle.selectedChild != null)
        {
            deleteSaveBtn.isEnabled = true;
        }
        else
        {
            deleteSaveBtn.isEnabled = false;
        }
    }

    public void OnClickBtn(GameObject go)
    {
        if (saveListPanle.selectedChild != null)
        {
            saveListPanle.DelSaveChild(saveListPanle.selectedChild);
        }
    }
}
