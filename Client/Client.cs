using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class Client : SingletonMono<Client>
{
    public GameObject playerPrefab;
    public GameObject blockPrefab;
    public Transform mapRoot;

    public bool Inited { get; private set; } = false;

    Dictionary<PositionInt, List<GameObject>> launchedChunks = new Dictionary<PositionInt, List<GameObject>>();
    List<PositionInt> enableChunks = new List<PositionInt>();

    ConcurrentBag<Action> serverCommands = new ConcurrentBag<Action>();

    // Start is called before the first frame update
    void Start()
    {
        mapRoot = GameObject.FindGameObjectWithTag("MapRoot").transform;

        //进入游戏场景说明服务器数据已经读取完毕
        //开始利用服务器数据生成游戏对象
        StartCoroutine(Init());
    }

    /// <summary>
    /// 调用Init时服务器数据已经读取完毕
    /// </summary>
    /// <returns></returns>
    public IEnumerator Init()
    {
        SpawnChunkGO();
        RefreshChunks(0, 0);

        CameraController.Instance.cameraOverChunkCallback += (int cX, int cY) =>
        {
            Server.Instance.Request(() =>
            {
                Server.Instance.worldCtrl.SetGodChunks(new PositionInt(cX, cY));
                Server.Instance.SendCommand(() =>
                {
                    RefreshChunks(cX, cY); //设置相机 跨区快回调 调用时 刷新区块显示});
                });
            });
        };

        yield return null;

        for (int i = 0; i < Server.Instance.spawnPlayerNum; i++)
        {
            CreatePlayer("A", 2f, new Position(Vector2.zero));
        }

        //客户端初始化完毕，设置状态，服务器开始更新
        Inited = true;
        if (Server.Instance.Inited)
        {
            Server.Instance.Start(this);
        }
        else
        {
            throw new Exception("Server 尚未被初始化！");
        }
    }
    // Update is called once per frame
    void Update()
    {
        ShowServerMsg();
        InvokeServerCommands();

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerData[] players = Server.Instance.playerCtrl.GetAllPlayers();
            for (int i = 0; i < players.Length; i++)
            {
                DeletePlayer(players[i].id);
            }
        }
    }

    public bool ContainsPlayer(string id)
    {
        return Server.Instance.playerCtrl.ContainsPlayer(id);
    }

    public void CreatePlayer(string name, float defaultSpeed, Position pos)
    {
        PlayerData data = Server.Instance.playerCtrl.CreatePlayer(name, defaultSpeed, pos);
        Instantiate(playerPrefab, pos.ToVector3(), Quaternion.identity).GetComponent<PlayerBehaviour>().id = data.id;
    }

    void SpawnChunkGO()
    {
        WorldControllerImpl worldCtrl = Server.Instance.worldCtrl;
        PositionInt leftBottomChunkPos = worldCtrl.GetLeftBottomChunkCenterPos();
        for (int i = 0; i < worldCtrl.WorldSize; i++)
        {
            for (int j = 0; j < worldCtrl.WorldSize; j++)
            {
                PositionInt chunkPos = new PositionInt(leftBottomChunkPos.x + j * WorldControllerImpl.chunkSize, leftBottomChunkPos.y + i * WorldControllerImpl.chunkSize);
                GameObject chunkGO = new GameObject();
                chunkGO.name = PositionSpawnName(chunkPos.x, chunkPos.y);
                chunkGO.transform.position = new Vector3(chunkPos.x, chunkPos.y, blockPrefab.transform.position.z);
                chunkGO.transform.parent = mapRoot;
                ChunkBehaviour chunkBehaviour = chunkGO.AddComponent<ChunkBehaviour>();
                chunkBehaviour.pos = chunkPos;
            }
        }
    }

    /// <summary>
    ///<para> 刷新区块显示 </para>
    /// <para>客户端显示的区块始终只有 godChunkChunkBounds * godChunkChunkBounds 个</para>
    /// <para>相机所在的区块将会是中心区块</para>
    /// <para>没有生成过的区块需要被显示时将会被生成</para>
    /// <para>已经生成过的区块无需被显示时将会被设置为disable，当重新进入显示范围时将会被设置为enable</para>
    /// </summary>
    /// <param name="cX"></param>
    /// <param name="cY"></param>
    public void RefreshChunks(int cX, int cY)
    {
        WorldControllerImpl worldCtrl = Server.Instance.worldCtrl;
        List<PositionInt> chunksTemp = worldCtrl.GetChunksCenterPos(new Position(cX, cY), WorldControllerImpl.godChunkChunkBounds);
        foreach (var pos in chunksTemp)
        {
            if (launchedChunks.ContainsKey(new PositionInt(pos.x, pos.y)))
            {
                ChunkSetActive(pos.x, pos.y, true);
            }
            else
            {
                LaunchChunk(pos.x, pos.y);
            }
        }


        #region 对比已经启用的区块，将离开显示范围的区块设置为禁用
        foreach (var pos in enableChunks)
        {
            if (chunksTemp.Contains(pos))
            {
                continue;
            }
            else
            {
                List<GameObject> blocks = new List<GameObject>();
                if (launchedChunks.ContainsKey(pos))
                {
                    blocks = launchedChunks[pos];
                }
                else
                {
                    continue;
                }

                foreach (var block in blocks)
                {
                    block.SetActive(false);
                }
            }
        }

        enableChunks = chunksTemp;
        #endregion
    }

    /// <summary>
    /// 生成区块（创建GameObject）
    /// </summary>
    /// <param name="cX"></param>
    /// <param name="cY"></param>
    void LaunchChunk(int cX, int cY)
    {
        PositionInt chunkPos = new PositionInt(cX, cY);
        GameObject chunkGO = mapRoot.Find(PositionSpawnName(chunkPos)).gameObject;
        List<GameObject> blockGOs = new List<GameObject>();
        Server.Instance.worldCtrl.TraversalChunk(cX, cY, (int x, int y) =>
        {
            GameObject blockGO = Instantiate(blockPrefab, new Vector3(x, y, blockPrefab.transform.position.z), Quaternion.identity, chunkGO.transform);
            blockGO.name = PositionSpawnName(x, y);
            BlockBehaviour behaviour = blockGO.GetComponent<BlockBehaviour>();
            behaviour.pos = new PositionInt(x, y);
            BlockData blockData = Server.Instance.worldCtrl.GetLoadedBlockData(new PositionInt(cX, cY), new PositionInt(x, y));
            behaviour.ChangeTerrain(blockData.terrainSpriteName);
            if (blockData.modelSpriteName != null)
            {
                behaviour.ChangeModel(blockData.modelSpriteName);
            }
            blockGOs.Add(blockGO);
        });

        launchedChunks.Add(chunkPos, blockGOs);
    }

    public BlockBehaviour FindBlockGO(PositionInt pos)
    {
        PositionInt chunkCenter = Server.Instance.worldCtrl.GetChunkCenterPos(pos);
        string chunkGameObjectName = PositionSpawnName(chunkCenter);
        string blockGameObjectName = PositionSpawnName(pos);

        Transform chunkT = mapRoot.Find(chunkGameObjectName);

        if (chunkT != null)
        {
            Transform blockT = chunkT.Find(blockGameObjectName);
            if (blockT != null)
            {
                return blockT.gameObject.GetComponent<BlockBehaviour>();
            }
            return null;
        }
        return null;
    }


    /// <summary>
    /// 设置已经被生成区块是否显示
    /// </summary>
    /// <param name="cX"></param>
    /// <param name="cY"></param>
    /// <param name="active"></param>
    void ChunkSetActive(int cX, int cY, bool active)
    {
        if (!launchedChunks.ContainsKey(new PositionInt(cX, cY)))
        {
            throw new Exception("x:" + cX + "  " + "y:" + cY + " 区块不存在于已经生成的区块列表中!");
        }
        foreach (var item in launchedChunks[new PositionInt(cX, cY)])
        {
            item.SetActive(active);
        }
    }

    public string PositionSpawnName(PositionInt pos)
    {
        return PositionSpawnName(pos.x, pos.y);
    }

    public string PositionSpawnName(int x, int y)
    {
        return x + ":" + y;
    }

    public void DeletePlayer(string id)
    {
        Server.Instance.playerCtrl.DelPlayer(id);
    }

    public void AddServerCommand(Action command)
    {
        serverCommands.Add(command);
    }

    void InvokeServerCommands()
    {
        Action command;
        while (serverCommands.TryTake(out command))
        {
            if (command != null)
            {
                command.Invoke();
            }
        }
    }

    void ShowServerMsg()
    {
        for (int i = 0; i < Server.serverMsgs.Count; i++)
        {
            Debug.Log(Server.serverMsgs[i]);
        }
        Server.serverMsgs.Clear();
    }
}
