using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public abstract class BlockData
{
    public BlockData()
    {
        type = GetType().Name;
    }

    public abstract void Init(PositionInt _pos);

    public PositionInt pos;
    public string type;
    public List<string> outputItems = new List<string>();

    public string terrainSpriteName;
    public string modelSpriteName;

    /// <summary>
    /// ����� BlockData���ԣ���BlockDataBehaviour���и���
    /// </summary>
    public abstract void GodUpdate();

    /// <summary>
    /// ����� BlockData���Խ��и��ģ�������BlockDataBehaviour���и���
    /// </summary>
    public abstract void ForceUpdate();

    public abstract BlockData Clone();
}
