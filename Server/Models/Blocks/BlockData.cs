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
    /// 允许对 BlockData属性，和BlockDataBehaviour进行更改
    /// </summary>
    public abstract void GodUpdate();

    /// <summary>
    /// 允许对 BlockData属性进行更改，不允许BlockDataBehaviour进行更改
    /// </summary>
    public abstract void ForceUpdate();

    public abstract BlockData Clone();
}
