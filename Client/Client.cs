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

        //������Ϸ����˵�������������Ѿ���ȡ���
        //��ʼ���÷���������������Ϸ����
        StartCoroutine(Init());
    }

    /// <summary>
    /// ����Initʱ�����������Ѿ���ȡ���
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
                    RefreshChunks(cX, cY); //������� ������ص� ����ʱ ˢ��������ʾ});
                });
            });
        };

        yield return null;

        for (int i = 0; i < Server.Instance.spawnPlayerNum; i++)
        {
            CreatePlayer("A", 2f, new Position(Vector2.zero));
        }

        //�ͻ��˳�ʼ����ϣ�����״̬����������ʼ����
        Inited = true;
        if (Server.Instance.Inited)
        {
            Server.Instance.Start(this);
        }
        else
        {
            throw new Exception("Server ��δ����ʼ����");
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
    ///<para> ˢ��������ʾ </para>
    /// <para>�ͻ�����ʾ������ʼ��ֻ�� godChunkChunkBounds * godChunkChunkBounds ��</para>
    /// <para>������ڵ����齫������������</para>
    /// <para>û�����ɹ���������Ҫ����ʾʱ���ᱻ����</para>
    /// <para>�Ѿ����ɹ����������豻��ʾʱ���ᱻ����Ϊdisable�������½�����ʾ��Χʱ���ᱻ����Ϊenable</para>
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


        #region �Ա��Ѿ����õ����飬���뿪��ʾ��Χ����������Ϊ����
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
    /// �������飨����GameObject��
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
    /// �����Ѿ������������Ƿ���ʾ
    /// </summary>
    /// <param name="cX"></param>
    /// <param name="cY"></param>
    /// <param name="active"></param>
    void ChunkSetActive(int cX, int cY, bool active)
    {
        if (!launchedChunks.ContainsKey(new PositionInt(cX, cY)))
        {
            throw new Exception("x:" + cX + "  " + "y:" + cY + " ���鲻�������Ѿ����ɵ������б���!");
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
