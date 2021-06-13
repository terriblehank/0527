using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    static T mInstance;

    public static T Instance
    {
        get
        {
            if (null == mInstance)
            {
                mInstance = FindObjectOfType(typeof(T)) as T;  //查询T类型的对象

                if (null == mInstance)
                {
                    if (Application.isPlaying && !Application.isEditor)
                    {
                        Debug.LogError(typeof(T) + " 没有实例化！");
                    }
                }

            }
            else if (mInstance.gameObject == null)
            {
                Debug.LogError(typeof(T) + " 没有清理干净！");
            }

            return mInstance;
        }
    }
}

/// <summary> 普通单例基类(没有MonoBehaviour) </summary>
public class Singleton<T> where T : new()
{
    protected static T mInstance;  //通过静态变量获取类实例

    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new T();
            }
            return mInstance;
        }
    }

    public static void New()
    {
        mInstance = new T();
    }
}
