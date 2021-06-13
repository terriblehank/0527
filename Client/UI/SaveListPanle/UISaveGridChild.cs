using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UISaveGridChild : MonoBehaviour
{
    public UISaveListPanle saveListPanle;
    public UISprite sprite;
    public UILabel nameLabel;
    public string filePath;

    void Start()
    {
        UIEventListener.Get(gameObject).onClick += OnClickBtn;
    }

    public void OnClickBtn(GameObject go)
    {
        if (saveListPanle.selectedChild)
        {
            if (saveListPanle.selectedChild != this)
            {
                saveListPanle.selectedChild.Select(false);
            }
        }
        Select(true);
    }

    public void Select(bool selected)
    {
        if (selected == true)
        {
            saveListPanle.selectedChild = this;
            sprite.color = new Color(138f / 255f, 138f / 255f, 138f / 255f);
        }
        else
        {
            sprite.color = Color.white;
        }
    }

    public void SetPath(string _path)
    {
        filePath = _path;
        if (File.Exists(filePath))
        {
            FileInfo info = new FileInfo(filePath);
            SetName(info.Name.Split('.')[0]);
        }
    }

    void SetName(string nameText)
    {
        nameLabel.text = nameText;
    }
}
