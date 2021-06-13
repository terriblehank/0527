using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalLog : ItemData
{
    public static void Register(WorldControllerImpl worldCtrl)
    {
        worldCtrl.AddItemType(nameof(NormalLog));
    }

    public static float Test = 100;

    public NormalLog()
    {
        property = new Properties(Test);
    }
}
