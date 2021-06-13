using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalLand : BlockData
{
    public static void Register(WorldControllerImpl worldCtrl)
    {
        worldCtrl.AddBlockType(nameof(NormalLand));
    }

    public override void Init(PositionInt _pos)
    {
        pos = _pos;
        terrainSpriteName = "lime_terracotta";
    }

    public override void GodUpdate()
    {
        
    }
    public override void ForceUpdate()
    {
        
    }

    public override BlockData Clone()
    {
        return new NormalLand();
    }
}
