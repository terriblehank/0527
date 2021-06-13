using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewSaveBtn : MonoBehaviour
{
    public UISaveListPanle saveListPanle;
    public UIButton newSaveBtn;

    // Start is called before the first frame update
    void Start()
    {
        UIEventListener.Get(gameObject).onClick = OnClickBtn;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickBtn(GameObject go)
    {
        saveListPanle.AddSaveChild("Save." + System.Guid.NewGuid().ToString());
    }
}
