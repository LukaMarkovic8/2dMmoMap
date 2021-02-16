using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public GameObject followObject;
    public CinemachineVirtualCamera virtualCamera;

    public float desiredCameraSize;

    public float minimumCameraSize = 1f;
    public float maximumCameraSize = 8f;

    Vector2 startPos;

    Vector2 dragStartPos;
    Vector2 dragNewPos;
    Vector2 finger0Pos;

    float fingersDist;
    bool zooming;
    public float zoomSpeed = 2f;
    public bool adjustingCameraSize = false;

    void Awake()
    {
        //setting the to the initial camera sieze
        desiredCameraSize = virtualCamera.m_Lens.OrthographicSize;
    }

    void Update()
    {
        if (Input.touchCount == 0 && zooming)
        {
            zooming = false;
        }
        //lerping the zoom value for a smooth zoom
        if (virtualCamera.m_Lens.OrthographicSize != desiredCameraSize)
        {
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, desiredCameraSize, Time.smoothDeltaTime * zoomSpeed);
        }

        //MOVING
        if (Input.touchCount == 1)
        {
            if (!zooming)
            {
                Move();
            }
        }
        //ZOOMING
        else if (Input.touchCount == 2)
        {
            Zoom();
        }
    }

    void LateUpdate()
    {
        //Restricting camera movement so it doesn't go out of map
        ClampCamera();
    }

    //Moving Around the map
    void Move()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 newPos = GetWorldPosition();
            Vector2 posDiff = newPos - startPos;
            followObject.transform.Translate(-posDiff);
        }
        startPos = GetWorldPosition();
    }

    //Zooming by changing camera size
    void Zoom()
    {
        if (Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            zooming = true;
            dragNewPos = GetWorldPositionOfFinger(1);
            Vector2 diff = dragNewPos - dragStartPos;

            if (Vector2.Distance(dragNewPos, finger0Pos) < fingersDist)
            {
                desiredCameraSize += (diff.magnitude);
            }

            if (Vector2.Distance(dragNewPos, finger0Pos) >= fingersDist)
            {
                desiredCameraSize -= (diff.magnitude);
            }

            desiredCameraSize = Mathf.Clamp(desiredCameraSize, minimumCameraSize, maximumCameraSize);
            fingersDist = Vector2.Distance(dragNewPos, finger0Pos);
        }
        dragStartPos = GetWorldPositionOfFinger(1);
        finger0Pos = GetWorldPositionOfFinger(0);
    }

    void ClampCamera()
    {
        //using the zoom value  and aspect ratio so the edge value is responsive
        float ascpet = Camera.main.aspect;
        float x = Mathf.Clamp(followObject.transform.position.x, 1 + desiredCameraSize * ascpet, DataController.dc.mapData.map_width - 1 - desiredCameraSize * ascpet);
        float y = Mathf.Clamp(followObject.transform.position.y, 1 + desiredCameraSize, DataController.dc.mapData.map_width - 1 - desiredCameraSize);
        followObject.transform.position = new Vector3(x, y, 0);
    }

    Vector2 GetWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    Vector2 GetWorldPositionOfFinger(int pFingerIndex)
    {
        return Camera.main.ScreenToWorldPoint(Input.GetTouch(pFingerIndex).position);
    }
}
