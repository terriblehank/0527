using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTree : BlockData
{
    public static void Register(WorldControllerImpl worldCtrl)
    {
        worldCtrl.AddBlockType(nameof(NormalTree));
    }

    public override BlockData Clone()
    {
        return new NormalTree();
    }

    public override void ForceUpdate()
    {
        
    }

    public override void GodUpdate()
    {
        /*if (Server.Instance.Time > time)
        {
            System.Random random = new System.Random(new NormalLand().GetHashCode());
            timeSpacing = random.Next(0, 20) / 10f;
            time = Server.Instance.Time + timeSpacing;
            flag = !flag;
            string spriteName = flag ? "log_oak_top" : "lime_terracotta";
            Server.Instance.SendCommand(() =>
            {
                BlockBehaviour blockBehaviour = Client.Instance.FindBlockGO(pos);
                blockBehaviour.ChangeSprite(spriteName);
            });
        }*/
    }

    public override void Init(PositionInt _pos)
    {
        pos = _pos;
        outputItems.Add(nameof(NormalLog));
        terrainSpriteName = "green_wool";
        modelSpriteName = "NormalTree";
    }
}
