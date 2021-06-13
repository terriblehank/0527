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

    public const float moveSmoothTime = 0.2f; //ƽ���ƶ���ʱ��
    public const float moveStopSmoothTime = 0.5f; //ֹͣ�ƶ�ƽ����ʱ��
    public const float minMoveTypeKeepTime = 0.3f; //��С�ƶ���ʽ���ֵ�ʱ��
    public const float maxMoveTypeKeepTime = 2f; //����ƶ���ʽ���ֵ�ʱ��
    public string id;
    public string name;
    public float defaultSpeed;
    public float curSpeed;
    public Properties properties = new Properties(100, 100);


    public float moveStartTime; //�ƶ���ʼ��ʱ���
    public Position movestartPos; //�ƶ���ʼλ�ü�¼
    public float moveStopTime; //�ƶ��п�ʼ���е�ʱ���
    public Position moveStopPos; //�ƶ��п�ʼ���е�λ�ü�¼
    public int moveType = 2; //0ˮƽ 1��ֱ 2б��
    public float moveTypeChangeTime; //�ı��ƶ���ʽ��ʱ��
    public float moveTypeKeepTime = 2f; //�ƶ���ʽ���ֵ�ʱ��

    public Position target;
    public string targetBlock;

    public Position pos;
    public PositionInt curChunk;

    public Dictionary<string, List<PositionInt>> blockChunkMemory = new Dictionary<string, List<PositionInt>>();
}
