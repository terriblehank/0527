using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SaveHelper : Singleton<SaveHelper>
{
    public delegate void CreateSaveCallback(string path);

    public IEnumerator CreateSave(string dbName, CreateSaveCallback callback = null)
    {
        string path = "";
#if UNITY_EDITOR || UNITY_STANDALONE ||UNITY_ANDROID

        path = PathHelper.SaveDatabasePath + "/" + dbName + ".db";
        if (!File.Exists(path))
        {
            string uri = PathHelper.TemplateDatabasePath;
            UnityWebRequest www = UnityWebRequest.Get(uri);
            yield return www.SendWebRequest();
            while (!www.downloadHandler.isDone) { Debug.Log("Downloading the db..."); }
            if (!Directory.Exists(PathHelper.SaveDatabasePath))
            {
                Directory.CreateDirectory(PathHelper.SaveDatabasePath);
            }
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] data = www.downloadHandler.data;
            Debug.Log(data.Length);
            fs.Write(data, 0, data.Length);
            fs.Close();
            Debug.Log("创建新的DB拷贝");
        }
        if (callback != null)
        {
            callback.Invoke(path);
        }
#endif
    }

    public void DeleteSave(string path)
    {
        FileInfo saveInfo = new FileInfo(path);
        saveInfo.Delete();
    }

    public string[] GetAllSave()
    {
        if (!Directory.Exists(PathHelper.SaveDatabasePath))
        {
            return new string[0];
        }
        string[] files = Directory.GetFiles(PathHelper.SaveDatabasePath, "*.db");


        string temp; //按创建时间排序
        for (int i = 0; i < files.Length - 1; i++)
        {
            for (int j = 0; j < files.Length - 1 - i; j++)
            {
                FileInfo infoA = new FileInfo(files[j]);
                FileInfo infoB = new FileInfo(files[j + 1]);
                if (infoA.CreationTime > infoB.CreationTime)
                {
                    temp = files[j + 1];
                    files[j + 1] = files[j];
                    files[j] = temp;
                }
            }
        }
        return files;
    }
}
