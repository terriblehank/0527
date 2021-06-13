using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WorldControllerImpl : IWorldController
{
    public const int blockWidth = 1;
    public const int minWorldSize = 3;
    public const int rendererChunkBounds = 3;
    public const int godChunkChunkBounds = 3;
    public const int chunkSize = 11;
    private int worldSize;
    public int WorldSize
    {
        get { return worldSize; }
        set
        {
            if (value < minWorldSize)
            {
                value = minWorldSize;
            }
            worldSize = value;
        }
    }

    public int WorldLength { get { return worldSize * chunkSize; } }

    private float chunkForceUpdateTime = 60f;

    private WorldControllerImpl() { }
    public static WorldControllerImpl CreateWorldController() { return new WorldControllerImpl(); }

    private Dictionary<string, BlockData> blockList = new Dictionary<string, BlockData>();

    private List<string> itemTypeList = new List<string>();

    /// <summary>
    /// 摄像机所在区块 和周围 rendererChunkBounds * rendererChunkBounds 个区块
    /// </summary>

    private Dictionary<PositionInt, Dictionary<PositionInt, BlockData>> godChunks = new Dictionary<PositionInt, Dictionary<PositionInt, BlockData>>();

    private Dictionary<PositionInt, Dictionary<PositionInt, BlockData>> forceUpdateChunks = new Dictionary<PositionInt, Dictionary<PositionInt, BlockData>>();
    private Dictionary<PositionInt, float> forceUpdateChunkTimers = new Dictionary<PositionInt, float>();

    private Dictionary<PositionInt, Dictionary<PositionInt, BlockData>> weakChunks = new Dictionary<PositionInt, Dictionary<PositionInt, BlockData>>();

    private Dictionary<PositionInt, List<string>> chunkPlayerMap = new Dictionary<PositionInt, List<string>>();

    /// <summary>
    /// 应该在区块被加载，区块中的地块被更改时更新
    /// </summary>
    private Dictionary<PositionInt, List<string>> chunkBlockList = new Dictionary<PositionInt, List<string>>();

    private Dictionary<string, List<string>> propertiyItemMap = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> itemBlockMap = new Dictionary<string, List<string>>();

    #region Delegates
    public delegate void TraversalChunkCallback(int x, int y);
    #endregion

    /// <summary>
    /// 注册Items，反射所有ItemData的子类，自动调用Register函数
    /// </summary>
    public void RegisterItems()
    {
        ReflectionHelper.FindChildsToDo(
            typeof(ItemData), "Register",
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.InvokeMethod,
            null, null, new object[] { this });
    }

    /// <summary>
    /// 注册Blocks，反射所有BlockData的子类，自动调用Register函数
    /// </summary>
    public void RegisterBlocks()
    {
        ReflectionHelper.FindChildsToDo(
            typeof(BlockData), "Register",
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.InvokeMethod, null, null, new object[] { this });
    }

    /// <summary>
    /// 世界生成的逻辑
    /// </summary>
    public void SpawnWorld()
    {
        LT("重新生成世界...");//UI
        LP(0);//UI
        DbHelper.Instance.ExecuteQuery(Server.Instance.conn, "Delete from World");
        LP(1);//UI

        PositionInt leftBottomPoint = new PositionInt(WorldLength / -2, WorldLength / -2);
        LT("生成地块中...");//UI

        float spawnCountTemp = 0;

        PositionInt leftBottomChunkCenterPos = GetLeftBottomChunkCenterPos();
        for (int cY = 0; cY < worldSize; cY++)
        {
            for (int cX = 0; cX < worldSize; cX++)
            {
                PositionInt chunkTempPos = new PositionInt(leftBottomChunkCenterPos.x + cX * chunkSize, leftBottomChunkCenterPos.y + cY * chunkSize);
                PositionInt chunkLeftBottomBlockPos = new PositionInt(chunkTempPos.x - (chunkSize / 2), chunkTempPos.y - (chunkSize / 2));

                string tableName = chunkTempPos.ToString();

                DbHelper.Instance.ExecuteQuery(Server.Instance.conn, "Drop table  if exists '" + tableName + "';");
                DbHelper.Instance.ExecuteQuery(Server.Instance.conn, "CREATE TABLE '" + tableName + "' AS SELECT * FROM World;");

                System.Data.Common.DbTransaction trans = Server.Instance.conn.BeginTransaction();
                try
                {
                    for (int y = 0; y < chunkSize; y++)
                    {
                        for (int x = 0; x < chunkSize; x++, spawnCountTemp++)
                        {
                            PositionInt blockTempPos = new PositionInt(chunkLeftBottomBlockPos.x + x, chunkLeftBottomBlockPos.y + y);
                            System.Random random = new System.Random(new HashBody().GetHashCode());
                            BlockData data;
                            if (random.Next(0, 10) < 1)
                            {
                                data = new NormalTree();
                            }
                            else
                            {
                                data = new NormalLand();
                            }

                            data.Init(blockTempPos);
                            DbHelper.Instance.ExecuteQuery(Server.Instance.conn, "INSERT INTO '" + tableName + "' VALUES (" + blockTempPos.x + "," + blockTempPos.y + ",'" + data.type + "');");
                            LP(spawnCountTemp / (WorldLength * WorldLength));//UI
                        }
                    }
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        LT("地块生成完毕");//UI
        LP(1);
    }

    public void AddItemType(string type)
    {
        itemTypeList.Add(type);
    }

    public void AddBlockType(string type)
    {
        BlockData blockData = (BlockData)Activator.CreateInstance(Type.GetType(type));
        blockData.Init(null);
        blockList.Add(type, blockData);
    }

    /// <summary>
    /// 初始化区块玩家地图
    /// </summary>
    public void InitChunkPlayerMap()
    {
        LT("初始化区块玩家地图...");//UI
        LP(0);
        int count = 0;
        PositionInt leftBottomChunk = GetLeftBottomChunkCenterPos();
        for (int y = 0; y < worldSize; y++)
        {
            for (int x = 0; x < worldSize; x++, count++)
            {
                PositionInt chunkTemp = new PositionInt(leftBottomChunk.x + x * chunkSize, leftBottomChunk.y + y * chunkSize);
                chunkPlayerMap.Add(chunkTemp, new List<string>());
                LP(count / (float)(worldSize * worldSize));
            }
        }
        LP(1);
    }

    /// <summary>
    /// 初始化区块物品列表
    /// </summary>
    public void InitChunkBlockList()
    {
        LT("初始化区块物品列表...");//UI
        LP(0);
        int count = 0;
        PositionInt leftBottomChunk = GetLeftBottomChunkCenterPos();
        for (int y = 0; y < worldSize; y++)
        {
            for (int x = 0; x < worldSize; x++, count++)
            {
                PositionInt chunkTemp = new PositionInt(leftBottomChunk.x + x * chunkSize, leftBottomChunk.y + y * chunkSize);
                chunkBlockList.Add(chunkTemp, new List<string>());
                LP(count / (float)(worldSize * worldSize));
            }
        }
        LP(1);
    }

    /// <summary>
    /// 设置 属性 和 物品 之间的“映射关系”，每一次加载服务器数据都会重新设置
    /// </summary>
    public void InitPropertyItemMappingList()
    {
        LT("初始化属性物品对应...");//UI

        PropertyInfo[] properties = typeof(Properties).GetProperties();

        for (int i = 0; i < properties.Length; i++)
        {
            string pName = properties[i].Name;
            propertiyItemMap.Add(pName, new List<string>());

            LT("初始化" + pName + "对应..."); //UI
            for (int j = 0; j < itemTypeList.Count; j++)
            {
                string item = itemTypeList[j];
                Type type = Type.GetType(item);
                var fieldInfo = type.GetField(pName);
                if (fieldInfo != null)
                {
                    propertiyItemMap[pName].Add(item);
                }
                LP(j / (float)itemTypeList.Count);//UI
            }
        }
    }

    /// <summary>
    /// 设置 物品 和 地块 之间的“映射关系”，每一次加载服务器数据都会重新设置
    /// </summary>
    public void InitItemBlockMappingList()
    {
        LT("初始化地块物品对应...");//UI
        foreach (var blockType in blockList.Keys)
        {
            var outputItems = blockList[blockType].outputItems;

            LT("初始化" + blockType + "对应..."); //UI
            foreach (var item in outputItems)
            {
                if (!itemBlockMap.ContainsKey(item)) itemBlockMap.Add(item, new List<string>());
                if (itemBlockMap[item].Contains(blockType)) continue;
                itemBlockMap[item].Add(blockType);
            }
        }
    }

    public bool InChunk(Position pos, PositionInt chunk)
    {
        return GetChunkCenterPos(pos) == chunk;
    }

    public PositionInt GetLeftBottomChunkCenterPos()
    {
        int num = -(worldSize / 2 * chunkSize);
        return new PositionInt(num, num);
    }

    /// <summary>
    /// 获取所在区块的中心坐标
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public PositionInt GetChunkCenterPos(Position pos)
    {
        float x = pos.x;
        float y = pos.y;

        float xTempA = x / (chunkSize / 2f);
        xTempA = Mathf.Sign(xTempA) > 0 ? Mathf.Floor(xTempA) : Mathf.Ceil(xTempA);

        float cXTemp = xTempA / 2;
        int cX = Mathf.Sign(cXTemp) > 0 ? Mathf.CeilToInt(cXTemp) : Mathf.FloorToInt(cXTemp);

        float yTempA = y / (chunkSize / 2f);
        yTempA = Mathf.Sign(yTempA) > 0 ? Mathf.Floor(yTempA) : Mathf.Ceil(yTempA);

        float cYTemp = yTempA / 2;
        int cY = Mathf.Sign(cYTemp) > 0 ? Mathf.CeilToInt(cYTemp) : Mathf.FloorToInt(cYTemp);

        return new PositionInt(cX * chunkSize, cY * chunkSize);
    }
    public PositionInt GetChunkCenterPos(PositionInt pos)
    {
        return GetChunkCenterPos(new Position(pos.x, pos.y));
    }

    /// <summary>
    /// 获取所在区块的排序编号
    /// <para>以0，0为中心向四周开始排序</para>
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public PositionInt GetChunkSortNum(Position pos)
    {
        float x = pos.x;
        float y = pos.y;

        float xTempA = x / (chunkSize / 2f);
        xTempA = Mathf.Sign(xTempA) > 0 ? Mathf.Floor(xTempA) : Mathf.Ceil(xTempA);

        float cXTemp = xTempA / 2;
        int noX = Mathf.Sign(cXTemp) > 0 ? Mathf.CeilToInt(cXTemp) : Mathf.FloorToInt(cXTemp);

        float yTempA = y / (chunkSize / 2f);
        yTempA = Mathf.Sign(yTempA) > 0 ? Mathf.Floor(yTempA) : Mathf.Ceil(yTempA);

        float cYTemp = yTempA / 2;
        int noY = Mathf.Sign(cYTemp) > 0 ? Mathf.CeilToInt(cYTemp) : Mathf.FloorToInt(cYTemp);

        return new PositionInt(noX, noY);
    }

    public PositionInt ChunkSortNumToPosition(PositionInt chunkSortNum)
    {
        PositionInt chunkPos = new PositionInt(chunkSortNum.x * chunkSize, chunkSortNum.y * chunkSize);
        return chunkPos;
    }

    /// <summary>
    /// 以pos为中心，获取一个范围内的区块
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="radius">范围直径</param>
    /// <returns></returns>
    public List<PositionInt> GetChunksCenterPos(Position pos, int diam)
    {
        if (diam <= 0)
        {
            throw new Exception("获取范围区块时，直径设置不能小于等于0！");
        }
        List<PositionInt> chunks = new List<PositionInt>();
        PositionInt center = GetChunkCenterPos(pos);
        PositionInt leftBottom = new PositionInt(center.x - (diam / 2) * chunkSize, center.y - (diam / 2) * chunkSize);
        for (int i = 0; i < diam; i++)
        {
            for (int j = 0; j < diam; j++)
            {
                PositionInt temp = new PositionInt(leftBottom.x + j * chunkSize, leftBottom.y + i * chunkSize);
                #region 世界大小限制
                PositionInt chunkSortNum = GetChunkSortNum(new Position(temp.x, temp.y));
                int halfWorldSize = worldSize / 2;
                if (Mathf.Abs(chunkSortNum.x) > halfWorldSize || Mathf.Abs(chunkSortNum.y) > halfWorldSize)
                {
                    continue;
                }
                #endregion
                chunks.Add(temp);
            }
        }
        return chunks;
    }

    public List<PositionInt> GetChunksCenterPos(PositionInt pos, int diam)
    {
        return GetChunksCenterPos(new Position(pos.x, pos.y), diam);
    }

    /// <summary>
    ///  用于遍历一个区块内的所有地块
    ///  <para>回调参数为地块坐标，用于定位地块，然后对其进行操作</para>
    /// </summary>
    /// <param name="cX"></param>
    /// <param name="cY"></param>
    /// <param name="callback"></param>
    public void TraversalChunk(int cX, int cY, TraversalChunkCallback callback)
    {
        int offset = chunkSize / 2;
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                int x = j - offset + cX;
                int y = i - offset + cY;
                callback(x, y);
            }
        }
    }

    /// <summary>
    /// 更换已经加载过的地块
    /// </summary>
    /// <param name="initedData">初始化完成的BlockData</param>
    public void ChangeLoadedBlockData(BlockData initedData)
    {
        PositionInt blockPos = initedData.pos;
        PositionInt chunk = GetChunkCenterPos(blockPos);
        Dictionary<PositionInt, BlockData> blockDatas = GetLoadedBlockDatas(chunk);
        blockDatas[blockPos] = initedData;

        UpdateLoadedChunkBlockList(chunk);
        Server.Instance.SendCommand(() =>
        {
            BlockBehaviour behaviour = Client.Instance.FindBlockGO(blockPos);
            if (behaviour == null)
            {
                return;
            }
            behaviour.ChangeTerrain(initedData.terrainSpriteName);
            if (initedData.modelSpriteName != null)
            {
                behaviour.ChangeModel(initedData.modelSpriteName);
            }
            else
            {
                behaviour.HideModel();
            }
        });
    }

    /// <summary>
    /// 如果有玩家在区块周围
    /// </summary>
    /// <param name="chunk"></param>
    /// <returns></returns>
    public bool HasPlayerAroundChunk(PositionInt chunk)
    {
        List<PositionInt> checkChunks = GetChunksCenterPos(chunk, rendererChunkBounds);
        foreach (var checkChunk in checkChunks)
        {

            if (chunkPlayerMap[checkChunk].Count > 0)
            {
                return true;
            }

        }
        return false;

        /**PlayerData[] players = Server.Instance.playerCtrl.GetAllPlayers();
        List<PositionInt> checkChunks = GetChunksCenterPos(chunk, rendererChunkBounds);

        foreach (var checkChunk in checkChunks)
        {
            foreach (var player in players)
            {
                if (player.curChunk == checkChunk)
                {
                    return true;
                }
            }
        }
        return false;**/
    }

    public void AddPlayerToChunkMap(string id, PositionInt chunk)
    {
        try
        {
            if (chunkPlayerMap[chunk].Contains(id))
            {
                throw new Exception(chunk + "区块已经添加了" + id + "对应");
            }
            chunkPlayerMap[chunk].Add(id);
        }
        catch (Exception)
        {
            throw new Exception(chunk + "在 chunkPlayerMap中存在 ？ " + chunkPlayerMap.ContainsKey(chunk));
        }
    }

    public void RefreshPlayerChunkMap(string id, PositionInt old, PositionInt newChunk)
    {
        if (chunkPlayerMap[old].Contains(id))
        {
            chunkPlayerMap[old].Remove(id);
        }
        AddPlayerToChunkMap(id, newChunk);
    }

    /// <summary>
    /// 设置区块为上帝区块
    /// </summary>
    /// <param name="pos"></param>
    public void SetGodChunks(PositionInt pos)
    {
        List<PositionInt> godChunksTemp = GetChunksCenterPos(pos, godChunkChunkBounds);

        #region 将不再处于摄像机渲染范围的区块移至强更新区块
        List<PositionInt> removeList = new List<PositionInt> { };
        foreach (var chunk in godChunks.Keys)
        {
            if (!godChunksTemp.Contains(chunk))
            {
                removeList.Add(chunk);
            }
        }

        foreach (var chunk in removeList)
        {
            if (HasPlayerAroundChunk(chunk))
            {
                godChunks.Remove(chunk);
                //移动这一个区块至强更新区块
                SetForceChunk(chunk, 1);
            }
            else
            {
                weakChunks.Add(chunk, godChunks[chunk]);
                godChunks.Remove(chunk);
            }
        }
        #endregion

        foreach (var chunk in godChunksTemp)
        {
            if (godChunks.ContainsKey(chunk)) continue;

            if (forceUpdateChunks.ContainsKey(chunk))
            {
                forceUpdateChunks.Remove(chunk);
                forceUpdateChunkTimers.Remove(chunk);
            }
            if (weakChunks.ContainsKey(chunk))
            {
                weakChunks.Remove(chunk);
            }

            Dictionary<PositionInt, BlockData> chunkBlocks = GetChunkBlockDatasInDB(chunk);

            godChunks.Add(chunk, chunkBlocks);

            UpdateLoadedChunkBlockList(chunk);
        }
    }

    /// <summary>
    /// 设置区块为强加载
    /// </summary>
    /// <param name="pos"></param>
    public void SetForceChunk(PositionInt pos, int bounds)
    {
        List<PositionInt> chunksTemp = GetChunksCenterPos(pos, bounds);

        foreach (var chunk in chunksTemp)
        {
            if (godChunks.ContainsKey(chunk)) continue;
            if (forceUpdateChunks.ContainsKey(chunk))
            {
                ResetForceChunkTimer(chunk);
                continue;
            }
            Dictionary<PositionInt, BlockData> chunkBlocks;
            if (weakChunks.ContainsKey(chunk))
            {
                chunkBlocks = weakChunks[chunk];
                weakChunks.Remove(chunk);
            }
            else
            {
                chunkBlocks = GetChunkBlockDatasInDB(chunk);
            }

            forceUpdateChunks.Add(chunk, chunkBlocks);

            ResetForceChunkTimer(chunk);

            UpdateLoadedChunkBlockList(chunk);
        }
    }

    public int GetChunkState(PositionInt chunk)
    {
        if (godChunks.ContainsKey(chunk))
        {
            return 1;
        }
        if (forceUpdateChunks.ContainsKey(chunk))
        {
            return 2;
        }
        if (weakChunks.ContainsKey(chunk))
        {
            return 0;
        }
        return -1;
    }

    public Dictionary<PositionInt, BlockData> GetLoadedBlockDatas(PositionInt chunk)
    {
        int state = GetChunkState(chunk);
        if (state == 1)
        {
            return godChunks[chunk];
        }
        else if (state == 2)
        {
            return forceUpdateChunks[chunk];
        }
        else if (state == 0)
        {
            return weakChunks[chunk];
        }
        else
        {
            throw new Exception(chunk + "还未被加载过！");
        }
    }

    public BlockData GetLoadedBlockData(PositionInt chunk, PositionInt blockPos)
    {
        return GetLoadedBlockDatas(chunk)[blockPos];
    }

    public List<string> CountLoadedChunkBlockTypeList(PositionInt chunk)
    {
        List<string> blockTypes = new List<string>();
        Dictionary<PositionInt, BlockData> blockDatas = GetLoadedBlockDatas(chunk);

        foreach (var block in blockDatas.Values)
        {
            if (blockTypes.Contains(block.type)) continue;
            blockTypes.Add(block.type);
        }

        return blockTypes;
    }

    /// <summary>
    /// 更新已经加载的区块地块类型列表
    /// </summary>
    /// <param name="chunk"></param>
    public void UpdateLoadedChunkBlockList(PositionInt chunk)
    {
        chunkBlockList[chunk] = CountLoadedChunkBlockTypeList(chunk);
    }

    public List<string> GetLoadedChunkBlockTypeList(PositionInt chunk)
    {
        if (chunkBlockList[chunk].Count <= 0) throw new Exception(chunk + "中的地块类型列表还未被设置!");
        return chunkBlockList[chunk];
    }

    /// <summary>
    /// 从数据库拉取单个地块数据
    /// <para>禁止通过这个方法遍历大量地块，效率低</para>
    /// </summary>
    /// <param name="blockPos"></param>
    /// <returns></returns>
    public BlockData GetBlockDataInDB(PositionInt blockPos)
    {
        if (blockPos == null)
        {
            throw new Exception("blockPos不可为空！");
        }
        PositionInt chunkCenterPos = GetChunkCenterPos(blockPos);

        SqliteDataReader reader = DbHelper.Instance.ExecuteQuery(Server.Instance.conn, "select * from '" + chunkCenterPos + "' where X = " + blockPos.x + " and Y =" + blockPos.y);
        reader.Read();
        return BuildBlockData(reader);
    }

    /// <summary>
    /// 从数据库拉取一个区块的地块数据
    /// </summary>
    /// <param name="chunkCenterPos"></param>
    /// <returns></returns>
    public Dictionary<PositionInt, BlockData> GetChunkBlockDatasInDB(PositionInt chunkCenterPos)
    {
        Dictionary<PositionInt, BlockData> blockDatas = new Dictionary<PositionInt, BlockData>();
        string command = "select * from '" + chunkCenterPos.ToString() + "'";
        SqliteDataReader reader = DbHelper.Instance.ExecuteQuery(Server.Instance.conn, command);
        while (reader.Read())
        {
            BlockData blockData = BuildBlockData(reader);
            blockDatas.Add(blockData.pos, blockData);
        }

        return blockDatas;
    }

    public string FindTargetBlockType(string property)
    {
        string targetItem = null;
        float valueTemp = 0;

        List<string> items = SearchItems(property);

        #region 临时选择逻辑

        foreach (var item in items)
        {
            Type itemType = Type.GetType(item);
            float value = (float)itemType.GetField(property).GetValue(itemType);
            if (value > valueTemp)
            {
                targetItem = itemType.Name;
            }
        }
        if (targetItem == null) return null;

        List<string> blocks = SearchBlocks(targetItem);

        if (blocks.Count <= 0) return null;

        #endregion

        return blocks[0];
    }

    public List<string> SearchItems(string property)
    {
        return propertiyItemMap[property];
    }

    public List<string> SearchBlocks(string item)
    {
        return itemBlockMap[item];
    }

    /// <summary>
    /// 数据库结构已经变动，勿用
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    [Obsolete]
    public List<PositionInt> FindBlocksInWorld(string block)
    {

        SqliteDataReader reader = DbHelper.Instance.ExecuteQuery(Server.Instance.conn, "select * from World where Block = '" + block + "'");
        List<PositionInt> blocks = new List<PositionInt>();

        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();//开始或继续测量某个时间间隔的运行时间。
        while (reader.Read())
        {
            PositionInt blockPos = BuildBlockPos(reader);

            blocks.Add(blockPos);
        }
        stopWatch.Stop();//停止测量某个时间间隔的运行时间。
        TimeSpan ts = stopWatch.Elapsed; //获取当前实例测量得出的总运行时间。
                                         //Server.Instance.AddMsg("寻找地块耗时：" + ts.TotalMilliseconds + "ms");
        return blocks;
    }

    /// <summary>
    /// 数据库结构已经变动，勿用
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    [Obsolete]
    public List<BlockData> GetAllBlockDatas()
    {
        SqliteDataReader reader = DbHelper.Instance.ExecuteQuery(Server.Instance.conn, "select * from World");
        List<BlockData> blocks = new List<BlockData>();
        while (reader.Read())
        {
            BlockData blockData = BuildBlockData(reader);

            blocks.Add(blockData);
        }
        return blocks;
    }

    private PositionInt BuildBlockPos(SqliteDataReader reader)
    {
        return new PositionInt(reader.GetInt32(0), reader.GetInt32(1));
    }
    private BlockData BuildBlockData(SqliteDataReader reader)
    {
        Type blockType = Type.GetType(reader.GetString(2));
        BlockData blockData = blockList[blockType.Name].Clone();
        blockData.Init(new PositionInt(reader.GetInt32(0), reader.GetInt32(1)));
        return blockData;
    }


    void LT(string text) { UILoadingDataShare.SetLoaidngText(text); }
    void LP(float value) { UILoadingDataShare.SetLoadingProgress(value); }

    /// <summary>
    /// 强更新，更新上帝区块和强更新区块列表中的区块
    /// </summary>
    public void ForceUpdate()
    {
        foreach (var blocks in godChunks.Values)
        {
            foreach (var block in blocks.Values)
            {
                block.GodUpdate();
            }
        }

        foreach (var blocks in forceUpdateChunks.Values)
        {
            foreach (var block in blocks.Values)
            {
                block.ForceUpdate();
            }
        }
    }

    public void ForceChunkTimerCheck()
    {
        List<PositionInt> removeList = new List<PositionInt>();
        List<PositionInt> refreshList = new List<PositionInt>();

        foreach (var chunk in forceUpdateChunkTimers.Keys)
        {
            if (Server.Instance.Time >= forceUpdateChunkTimers[chunk])
            {
                PlayerData[] players = Server.Instance.playerCtrl.GetAllPlayers();
                List<PositionInt> checkChunks = GetChunksCenterPos(chunk, rendererChunkBounds);

                bool hasPlayerAroundChunk = HasPlayerAroundChunk(chunk);

                if (hasPlayerAroundChunk)
                {
                    refreshList.Add(chunk);
                    continue;
                }

                weakChunks.Add(chunk, forceUpdateChunks[chunk]);
                forceUpdateChunks.Remove(chunk);
                removeList.Add(chunk);
                Server.AddMsg(chunk + "退出强更新");
            }
        }

        foreach (var chunk in removeList)
        {
            forceUpdateChunkTimers.Remove(chunk);
        }
        foreach (var chunk in refreshList)
        {
            ResetForceChunkTimer(chunk);
        }
    }

    void ResetForceChunkTimer(PositionInt chunk)
    {
        if (forceUpdateChunkTimers.ContainsKey(chunk))
        {
            forceUpdateChunkTimers[chunk] = Server.Instance.Time + chunkForceUpdateTime;
        }
        else
        {
            forceUpdateChunkTimers.Add(chunk, Server.Instance.Time + chunkForceUpdateTime);
        }
    }
}
