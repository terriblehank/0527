using Mono.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

//git
class Server : Singleton<Server>
{
    const int serverUpdateFrame = 120;
    int msPerFrame { get { return 1000 / serverUpdateFrame; } }

    const float fixedTime = 0.02f;

    public bool Inited { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;

    public bool Stopping { get; private set; } = false;

    public float Time { get; private set; } = 0f;
    public float DeltaTime { get; private set; } = 0f;

    public int Fps { get; private set; } = 0;

    public Client client;
    public PlayerControllerImpl playerCtrl = PlayerControllerImpl.CreatePlayerController();
    public WorldControllerImpl worldCtrl = WorldControllerImpl.CreateWorldController();

    public string saveFilePath;
    public SqliteConnection conn;

    public int spawnPlayerNum = 20;

    public static List<string> serverMsgs = new List<string>();

    public ConcurrentBag<Action> clientRequests = new ConcurrentBag<Action>();

    #region Threads
    Thread initThread;
    Thread updateThread;
    Thread fixedUpdateThread;
    #endregion
    public static void AddMsg(string msg)
    {
        serverMsgs.Add(msg);
    }

    public void InitAsync(string _saveFilePath)
    {
        saveFilePath = _saveFilePath;
        conn = DbHelper.Instance.OpenConnect(saveFilePath);
        ThreadStart method = new ThreadStart(Init);
        initThread = new Thread(method);
        initThread.Start();
    }

    void Init()
    {
        UILoadingDataShare.onDone = ScenesLoader.LoadMainScene; //设置数据记载完毕的回调为切换场景

        UILoadingDataShare.SetLoaidngText("注册物品信息...");
        UILoadingDataShare.SetLoadingProgress(0);
        worldCtrl.RegisterItems();
        UILoadingDataShare.SetLoadingProgress(1);

        UILoadingDataShare.SetLoadingProgress(0);
        UILoadingDataShare.SetLoaidngText("注册Blocks...");
        worldCtrl.RegisterBlocks();
        UILoadingDataShare.SetLoadingProgress(1);

        worldCtrl.InitChunkBlockList();
        worldCtrl.InitChunkPlayerMap();

        worldCtrl.InitPropertyItemMappingList();

        worldCtrl.InitItemBlockMappingList();
        UILoadingDataShare.SetLoadingProgress(1);

        if (true) //世界未被生成过
        {
            worldCtrl.SpawnWorld();
        }

        //服务器初始化时就要加载一次上帝区块，这样客户端初始化时才能拿得到数据
        UILoadingDataShare.SetLoadingProgress(0);
        UILoadingDataShare.SetLoaidngText("加载上帝区块...");
        worldCtrl.SetGodChunks(new PositionInt(0, 0));
        UILoadingDataShare.SetLoadingProgress(1);

        UILoadingDataShare.Done(); //设置加载状态为完成，加载UI将会调用设置好的回调函数

        AddMsg("Exited server init thread.");
        Inited = true;
    }

    public void Start(Client _client)
    {
        if (_client.Inited)
        {
            client = _client;
            StartUpdates();
        }
    }

    void StartUpdates()
    {
        ThreadStart updateMethod = new ThreadStart(ServerUpdate);
        Thread updateThread = new Thread(updateMethod);
        updateThread.Start();
        ThreadStart fixedUpdateMethod = new ThreadStart(ServerFiexdUpdate);
        Thread fixedUpdateThread = new Thread(fixedUpdateMethod);
        fixedUpdateThread.Start();
    }

    void ServerUpdate()
    {
        while (!Stopping)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (!IsPaused)
            {
                Update();
            }
            sw.Stop();
            double ms = sw.Elapsed.TotalMilliseconds;
            double timeSpacing = msPerFrame - ms;
            int sleepTime = (int)Math.Round(timeSpacing);
            double deltaTimeTemp = (ms / 1000d);
            if (timeSpacing > 0)
            {
                Thread.Sleep(sleepTime);
                Fps = 1000 / msPerFrame;
                deltaTimeTemp = msPerFrame / 1000d;
            }
            else
            {
                Fps = 1000 / (int)Math.Round(ms);
            }

            DeltaTime = (float)deltaTimeTemp;
        }

        conn.Close();
    }

    void ServerFiexdUpdate()
    {
        while (!Stopping)
        {
            if (!IsPaused)
            {
                FixedUpdate();
                Time += fixedTime;
            }
            Thread.Sleep((int)(fixedTime * 1000));
        }
    }

    //Update 和 FixedUpdate不属于同一线程 ，注意线程安全。
    void Update()
    {
        InvokeClientRequests();
        playerCtrl.PlayerActionUpdate();
        worldCtrl.ForceUpdate();
        worldCtrl.ForceChunkTimerCheck();
    }

    void FixedUpdate()
    {
        playerCtrl.PlayerPropertyUpdate();
    }

    public void Pause()
    {
        AddMsg("服务器暂停");
        IsPaused = true;
    }

    public void Resume()
    {
        AddMsg("服务器继续");
        IsPaused = false;
    }

    public void Stop()
    {
        AddMsg("服务器关闭");
        Stopping = true;
    }

    public void SendCommand(Action action)
    {
        if (Stopping)
        {
            return;
        }
        if (client == null)
        {
            throw new Exception("客户端为空！");
        }
        if (!client.Inited)
        {
            throw new Exception("客户端未初始化完毕！");
        }
        client.AddServerCommand(action);
    }

    public void Request(Action action)
    {
        clientRequests.Add(action);
    }
    void InvokeClientRequests()
    {
        Action command;
        while (clientRequests.TryTake(out command))
        {
            if (command != null)
            {
                command.Invoke();
            }
        }
    }
}

