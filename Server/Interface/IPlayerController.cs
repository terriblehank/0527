using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerController
{
    /// <summary>
    /// 添加player
    /// </summary>
    public PlayerData CreatePlayer(string name, float defaultSpeed, Position pos);

    /// <summary>
    /// 删除player
    /// </summary>
    public void DelPlayer(string id);

    /// <summary>
    /// 根据id获取player
    /// </summary>
    /// <param name="id"></param>
    public PlayerData GetPlayer(string id);

    /// <summary>
    /// 获取所有player
    /// </summary>
    /// <returns></returns>
    public PlayerData[] GetAllPlayers();


}
