using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerController
{
    /// <summary>
    /// ���player
    /// </summary>
    public PlayerData CreatePlayer(string name, float defaultSpeed, Position pos);

    /// <summary>
    /// ɾ��player
    /// </summary>
    public void DelPlayer(string id);

    /// <summary>
    /// ����id��ȡplayer
    /// </summary>
    /// <param name="id"></param>
    public PlayerData GetPlayer(string id);

    /// <summary>
    /// ��ȡ����player
    /// </summary>
    /// <returns></returns>
    public PlayerData[] GetAllPlayers();


}
