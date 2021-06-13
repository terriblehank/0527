using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 20210613 接入Git做版本管理
/// 更改测试提交
/// </summary>
public class PlayerControllerImpl : IPlayerController
{
    private const float slippingDistance = 0.5f;//距离小于开始滑动
    private const float stopMoveDistance = 0.01f;//距离小于停止移动

    private PlayerControllerImpl() { }

    public static PlayerControllerImpl CreatePlayerController() { return new PlayerControllerImpl(); }

    private Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();

    public bool ContainsPlayer(string id)
    {
        return players.ContainsKey(id);
    }

    public PlayerData CreatePlayer(string name, float defaultSpeed, Position pos)
    {
        PlayerData player = new PlayerData(name, defaultSpeed, pos);
        player.curChunk = Server.Instance.worldCtrl.GetChunkCenterPos(player.pos);

        players.Add(player.id, player);
        PositionInt chunkInside = Server.Instance.worldCtrl.GetChunkCenterPos(player.pos);
        Server.Instance.worldCtrl.SetForceChunk(chunkInside, WorldControllerImpl.rendererChunkBounds);
        Server.Instance.worldCtrl.AddPlayerToChunkMap(player.id, player.curChunk);
        UpdatePlayerBlockChunkMemory(player, chunkInside);

        return player;
    }

    public void DelPlayer(string id)
    {
        players[id] = null;
        players.Remove(id);
    }

    public PlayerData GetPlayer(string id)
    {
        if (ContainsPlayer(id))
        {
            return players[id];
        }
        return null;
    }

    public PlayerData[] GetAllPlayers()
    {
        PlayerData[] playerArray = new PlayerData[players.Count];
        players.Values.CopyTo(playerArray, 0);
        return playerArray;
    }

    public void SetPlayerTarget(PlayerData player, Position value)
    {
        if (value == null)
        {
            player.target = value;
            return;
        }
        if (player.target == null)
        {
            Position target = new Position(value.x, value.y);
            System.Random random = new System.Random(new HashBody().GetHashCode());
            target.x = value.x - 0.4f + (float)random.NextDouble() * 0.81f;
            target.y = value.y - 0.4f + (float)random.NextDouble() * 0.81f;

            player.target = target;
            player.movestartPos = player.pos;
            player.moveStartTime = Server.Instance.Time;
        }
    }

    /// <summary>
    /// 更新player的地块记忆
    /// </summary>
    /// <param name="player"></param>
    /// <param name="chunk"></param>
    void UpdatePlayerBlockChunkMemory(PlayerData player, PositionInt chunk)
    {
        List<string> blockTypeList = Server.Instance.worldCtrl.GetLoadedChunkBlockTypeList(chunk);

        foreach (var blockType in player.blockChunkMemory.Keys)
        {
            if (player.blockChunkMemory[blockType].Contains(chunk))
            {
                if (!blockTypeList.Contains(blockType))
                {
                    player.blockChunkMemory[blockType].Remove(chunk);
                }
            }

        }

        foreach (var blockType in blockTypeList)
        {
            if (!player.blockChunkMemory.ContainsKey(blockType))
            {
                player.blockChunkMemory.Add(blockType, new List<PositionInt>());
            }
            if (!player.blockChunkMemory[blockType].Contains(chunk))
            {
                player.blockChunkMemory[blockType].Add(chunk);
            }
        }
    }

    public void MovePlayer(PlayerData player)
    {
        ///理论需要移动的距离
        float absoluteDistance = Vector2.Distance(player.movestartPos.ToVector2(), player.target.ToVector2());
        ///已经移动了的距离
        float movedDistance = Vector2.Distance(player.movestartPos.ToVector2(), player.pos.ToVector2());

        if (movedDistance > absoluteDistance) //如果已经移动的距离超过了理论需要移动的距离
        {
            player.pos = player.target;//那么直接将位置设置到目标位置上 （为了防止异常的超距离移动）
        }

        float curDistance = Vector2.Distance(player.pos.ToVector2(), player.target.ToVector2());

        if (curDistance <= slippingDistance)
        {
            player.curSpeed = 0f;

            if (player.moveStopPos == null)
            {
                player.moveStopPos = player.pos;
                player.moveStopTime = Server.Instance.Time;
            }

            float x = Mathf.Lerp(player.moveStopPos.x, player.target.x, (Server.Instance.Time - player.moveStopTime) / PlayerData.moveStopSmoothTime);
            float y = Mathf.Lerp(player.moveStopPos.y, player.target.y, (Server.Instance.Time - player.moveStopTime) / PlayerData.moveStopSmoothTime);

            player.pos = new Position(x, y); //如果距离目标地点距离小于0.5，停止利用速度计算移动，开始坐标插值缓慢靠近

            if (curDistance <= stopMoveDistance)
            {
                PositionInt blockPos = player.pos.ToPositionInt();
                PositionInt chunk = Server.Instance.worldCtrl.GetChunkCenterPos(blockPos);

                //临时的callback，没有实现未加载地块更换的逻辑
                if (Server.Instance.worldCtrl.GetLoadedBlockData(chunk, blockPos).type == player.targetBlock)
                {
                    player.properties += (new NormalLog()).property;
                    NormalLand normalLand = new NormalLand();
                    normalLand.Init(blockPos);
                    Server.Instance.worldCtrl.ChangeLoadedBlockData(normalLand);
                    UpdatePlayerBlockChunkMemory(player, chunk);
                }

                player.target = null; //停止移动重置属性
                player.targetBlock = null;
                player.movestartPos = null;
                player.moveStartTime = 0f;
                player.moveStopPos = null;
                player.moveStopTime = 0f;
                player.moveType = 2;
                player.moveTypeChangeTime = 0f;
                player.moveTypeKeepTime = 0f;
            }
            return;
        }

        #region 运动逻辑
        Position dir = player.target - player.pos;

        #region 运动模式计算
        if (Server.Instance.Time - player.moveTypeChangeTime >= player.moveTypeKeepTime) //每随机一段时间后随机一次运动模式
        {
            System.Random random = new System.Random(new HashBody().GetHashCode());
            player.moveType = random.Next(0, 3);
            player.moveTypeChangeTime = Server.Instance.Time;
            int randomNum = random.Next((int)(PlayerData.minMoveTypeKeepTime * 10), (int)(PlayerData.maxMoveTypeKeepTime * 10));
            player.moveTypeKeepTime = randomNum / 10f;
        }

        if (Mathf.Abs(dir.x) <= 0.5f)//如果位置已经靠近X,Y方向上的边界，则强制采取斜向移动(2)
        {
            player.moveType = 2;
        }
        if (Mathf.Abs(dir.y) <= 0.5f)
        {
            player.moveType = 2;
        }

        if (player.moveType == 0)
        {
            dir.x = Mathf.Sign(dir.x); //水平移动
            dir.y = 0;
        }
        else if (player.moveType == 1)
        {
            dir.x = 0;
            dir.y = Mathf.Sign(dir.y); //垂直移动
        }
        #endregion

        player.curSpeed = Mathf.Lerp(0, player.defaultSpeed, (Server.Instance.Time - player.moveStartTime) / PlayerData.moveSmoothTime);
        Vector2 dirVector2 = new Vector2(dir.x, dir.y).normalized * Server.Instance.DeltaTime * player.curSpeed;
        dir.x = dirVector2.x;
        dir.y = dirVector2.y;

        PositionInt chunkInside = Server.Instance.worldCtrl.GetChunkCenterPos(new Position(player.pos.x, player.pos.y));

        player.pos += dir;
        player.pos = PlayerPostionClamp(player.pos);

        PositionInt chunkInsideNow = Server.Instance.worldCtrl.GetChunkCenterPos(new Position(player.pos.x, player.pos.y));
        if (chunkInsideNow.x != chunkInside.x || chunkInsideNow.y != chunkInside.y)
        {
            Server.Instance.worldCtrl.RefreshPlayerChunkMap(player.id, player.curChunk, chunkInsideNow);
            Server.Instance.worldCtrl.SetForceChunk(chunkInsideNow, WorldControllerImpl.rendererChunkBounds);
            player.curChunk = chunkInsideNow;
            UpdatePlayerBlockChunkMemory(player, chunkInsideNow);
        }
        #endregion
    }

    /// <summary>
    /// 限制玩家移动范围在地图区域内
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    Position PlayerPostionClamp(Position pos)
    {
        float x = pos.x;
        float y = pos.y;
        float bounds = Server.Instance.worldCtrl.WorldLength / 2 + 0.5f;
        if (x > bounds)
        {
            x = bounds;
        }
        if (x < -bounds)
        {
            x = -bounds;
        }
        if (y > bounds)
        {
            y = bounds;
        }
        if (y < -bounds)
        {
            y = -bounds;
        }

        return new Position(x, y);
    }

    /// <summary>
    /// 玩家属性更新
    /// </summary>
    public void PlayerPropertyUpdate()
    {
        foreach (var player in GetAllPlayers())
        {
            player.properties.Test -= 0.1f;
        }
    }

    /// <summary>
    /// 玩家行为更新
    /// </summary>
    public void PlayerActionUpdate()
    {
        //System.Data.Common.DbTransaction trans = Server.Instance.conn.BeginTransaction(); //加载速度问题
        foreach (var player in GetAllPlayers())
        {
            if (player.target != null)
            {
                MovePlayer(player);
            }
            else if (player.properties.Test <= 98)
            {
                System.Random random = new System.Random(new HashBody().GetHashCode());

                string blockType = Server.Instance.worldCtrl.FindTargetBlockType(nameof(player.properties.Test));

                if (blockType == null) continue;

                player.targetBlock = blockType;
                PositionInt target = null;

                List<PositionInt> memoryChunks = player.blockChunkMemory[blockType];


                if (memoryChunks.Count <= 0)
                {
                    PlayerGoExplore(player);
                    return;
                }
                else
                {
                    target = memoryChunks[0]; //暂时总是回去下标为0的区块
                }

                if (Server.Instance.worldCtrl.InChunk(player.pos, target))
                {
                    Dictionary<PositionInt, BlockData> blocks = Server.Instance.worldCtrl.GetLoadedBlockDatas(target);
                    List<PositionInt> targetList = new List<PositionInt>();
                    foreach (var block in blocks.Values)
                    {
                        if (blockType == block.type)
                        {
                            targetList.Add(block.pos);
                        }
                    }
                    if (targetList.Count <= 0)
                    {
                        PlayerGoExplore(player); //如果到区块发现没有目标地块了，重新开始探索 -临时-
                        return;
                    }
                    else
                    {
                        int index = random.Next(0, targetList.Count);
                        try
                        {
                            SetPlayerTarget(player, targetList[index].ToPosition());
                        }
                        catch (System.Exception)
                        {
                            throw new System.Exception("count:" + targetList.Count + "， index:" + index);
                        }
                    }
                }
                else
                {
                    SetPlayerTarget(player, target.ToPosition());
                }
            }
        }
        //trans.Commit();
    }

    void PlayerGoExplore(PlayerData player)
    {
        System.Random random = new System.Random(new HashBody().GetHashCode());

        int xOffset = random.Next(-1, 2);
        int yOffset = random.Next(-1, 2);
        PositionInt chunk = Server.Instance.worldCtrl.GetChunkSortNum(player.pos);
        PositionInt targetChunkSortNum = new PositionInt(chunk.x + xOffset, chunk.y + yOffset);
        SetPlayerTarget(player, Server.Instance.worldCtrl.ChunkSortNumToPosition(targetChunkSortNum).ToPosition());
    }

}
