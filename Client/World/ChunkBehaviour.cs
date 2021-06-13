using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBehaviour : MonoBehaviour
{
    public PositionInt pos;

    private void OnDrawGizmos()
    {
        int halfChunk = WorldControllerImpl.chunkSize / 2;

        if (pos == null)
        {
            return;
        }

        float blockOffset = WorldControllerImpl.blockWidth / 2;

        Vector3 lb = new Vector3(pos.x - halfChunk - blockOffset, pos.y - halfChunk - blockOffset, 0);
        Vector3 lt = new Vector3(pos.x - halfChunk - blockOffset, pos.y + halfChunk + blockOffset, 0);
        Vector3 rt = new Vector3(pos.x + halfChunk + blockOffset, pos.y + halfChunk + blockOffset, 0);
        Vector3 rb = new Vector3(pos.x + halfChunk + blockOffset, pos.y - halfChunk - blockOffset, 0);
        Gizmos.color = Color.red;
        int state = Server.Instance.worldCtrl.GetChunkState(pos);
        if (state == 1)
        {
            Gizmos.color = Color.yellow;
        }
        else if (state == 2)
        {
            Gizmos.color = Color.green;
        }
        else if (state == 0)
        {
            Gizmos.color = Color.gray;
        }
        Gizmos.DrawLine(Vector3.zero, Vector3.one);

        Gizmos.DrawLine(lb, lt);
        Gizmos.DrawLine(lt, rt);
        Gizmos.DrawLine(rt, rb);
        Gizmos.DrawLine(rb, lb);
    }
}
