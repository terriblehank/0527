using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : SingletonMono<CameraController>
{
    public DragCamera2D dragCamera2D;

    public delegate void CameraOverChunk(int cX, int cY);
    public CameraOverChunk cameraOverChunkCallback;

    public PositionInt chunkInside;


    // Start is called before the first frame update
    void Start()
    {
        float far = Server.Instance.worldCtrl.WorldLength;
        Camera.main.farClipPlane = far + 10;
        transform.position = new Vector3(transform.position.x, transform.position.y, -far / 2f);

        chunkInside = Server.Instance.worldCtrl.GetChunkCenterPos(new Position(transform.position.x, transform.position.y));

        dragCamera2D = GetComponent<DragCamera2D>();
#if UNITY_EDITOR || UNITY_STANDALONE
        dragCamera2D.dragEnabled = true;
        dragCamera2D.touchEnabled = false;
        dragCamera2D.dragSpeed = -1f;
        dragCamera2D.zoomStepSize = 0.68f;
#elif UNITY_ANDROID
        dragCamera2D.dragEnabled = false;
        dragCamera2D.touchEnabled = true;
        dragCamera2D.dragSpeed = -0.01f;
        dragCamera2D.zoomStepSize = 0.015f;
#endif
        float halfWorldLength = Server.Instance.worldCtrl.WorldLength / 2f;
        float blockOffset = WorldControllerImpl.blockWidth / 2;
        dragCamera2D.bounds.pointa = new Vector3(halfWorldLength + blockOffset, halfWorldLength + blockOffset, 0);
        dragCamera2D.bounds.transform.position = new Vector3(-(halfWorldLength + blockOffset), -(halfWorldLength + blockOffset), 0);
    }

    private void Update()
    {
        float chunkSize = WorldControllerImpl.chunkSize;
        if (Screen.width > Screen.height)
        {
            float heightSize = chunkSize / 2; //5.5

            float width = Screen.width;
            float height = Screen.height;
            float percent = width / height; // 2.05555

            float widthSize = heightSize * percent; //11.3055525

            float trueSize = heightSize / (widthSize / chunkSize); //1.0277777

            dragCamera2D.maxZoom = trueSize - 0.1f;
        }
        else
        {
            dragCamera2D.maxZoom = chunkSize - 0.1f;
        }
        PositionInt chunkInsideNow = Server.Instance.worldCtrl.GetChunkCenterPos(new Position(transform.position.x, transform.position.y));
        if (chunkInsideNow.x != chunkInside.x || chunkInsideNow.y != chunkInside.y)
        {
            cameraOverChunkCallback.Invoke(chunkInsideNow.x, chunkInsideNow.y);
            chunkInside = chunkInsideNow;
        }
    }

    public float GetCamareMaxSize()
    {
        return dragCamera2D.maxZoom;
    }

    private void OnDrawGizmos()
    {
        if (chunkInside == null)
        {
            return;
        }

        Vector3 lb = new Vector3(chunkInside.x - 1, chunkInside.y - 1, 0);
        Vector3 lt = new Vector3(chunkInside.x - 1, chunkInside.y + 1, 0);
        Vector3 rt = new Vector3(chunkInside.x + 1, chunkInside.y + 1, 0);
        Vector3 rb = new Vector3(chunkInside.x + 1, chunkInside.y - 1, 0);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, Vector3.one);

        Gizmos.DrawLine(lb, lt);
        Gizmos.DrawLine(lt, rt);
        Gizmos.DrawLine(rt, rb);
        Gizmos.DrawLine(rb, lb);
    }

}
