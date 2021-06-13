using System;
using System.Collections.Generic;
using UnityEngine;


public class PlayerData
{
    private PlayerData() { }

    public PlayerData(string _name, float _defaultSpeed, Position _pos)
    {
        id = System.Guid.NewGuid().ToString();
        name = _name;
        defaultSpeed = _defaultSpeed;
        pos = _pos;
    }

    public const float moveSmoothTime = 0.2f; //平滑移动的时间
    public const float moveStopSmoothTime = 0.5f; //停止移动平滑的时间
    public const float minMoveTypeKeepTime = 0.3f; //最小移动方式保持的时间
    public const float maxMoveTypeKeepTime = 2f; //最大移动方式保持的时间
    public string id;
    public string name;
    public float defaultSpeed;
    public float curSpeed;
    public Properties properties = new Properties(100, 100);


    public float moveStartTime; //移动开始的时间点
    public Position movestartPos; //移动开始位置记录
    public float moveStopTime; //移动中开始滑行的时间点
    public Position moveStopPos; //移动中开始滑行的位置记录
    public int moveType = 2; //0水平 1垂直 2斜向
    public float moveTypeChangeTime; //改变移动方式的时间
    public float moveTypeKeepTime = 2f; //移动方式保持的时间

    public Position target;
    public string targetBlock;

    public Position pos;
    public PositionInt curChunk;

    public Dictionary<string, List<PositionInt>> blockChunkMemory = new Dictionary<string, List<PositionInt>>();
}
