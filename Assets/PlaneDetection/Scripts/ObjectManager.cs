using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.XR.ARFoundation;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] GameObject indicator;
    [SerializeField] GameObject showcaseObj;

    ARRaycastManager raycastManager;
    [Range(0f, 100f)] 
    [SerializeField] float rotationSpeed = 50f;
    [Range(0f, 10f)]
    [SerializeField] float repositionTime = 4f;

    void Awake()
    {
        // 임시 프레임 코드
        Application.targetFrameRate = 60;

        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Start()
    {
        showcaseObj.SetActive(false);
    }

    void Update()
    {
        ARRaycastHit hitInfo = CastARRay();
        TouchScreen(hitInfo);
    }

    /// <summary>
    /// 스크린을 터치 조작하여 Object를 생성 혹은 위치나 회전 값을 조정하는 함수
    /// </summary>
    private void TouchScreen(ARRaycastHit hitInfo)
    {
        if (Input.touchCount <= 0) return;

        Touch touch = Input.GetTouch(0);
        Vector2 startTouchPosition = Vector2.zero;
        float touchStartTime = 0;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (hitInfo.trackable)
                {
                    // 최초 생성시 위치 초기화
                    if (!showcaseObj.activeSelf)
                    {
                        showcaseObj.SetActive(true);
                        showcaseObj.transform.position = hitInfo.pose.position;
                    }
                    startTouchPosition = touch.position;
                    touchStartTime = Time.time;
                }
                else
                {
                    showcaseObj.SetActive(false);
                }
                break;

            case TouchPhase.Moved:
                // 오브젝트 회전
                if (hitInfo.trackable)
                {
                    Vector3 rotVec = new Vector3(0, -touch.deltaPosition.x * rotationSpeed * Time.deltaTime, 0);
                    showcaseObj.transform.Rotate(rotVec);
                    startTouchPosition = touch.position;
                }
                break;

            case TouchPhase.Stationary:
                // 2초동안 터치시 위치 변경
                if (Time.time - touchStartTime >= repositionTime)
                {
                    showcaseObj.transform.position = hitInfo.pose.position;
                }
                break;

            case TouchPhase.Ended:
                break;
        }
    }

    /// <summary>
    /// AR Ray를 발사하여 indicator를 위치시킨다.
    /// </summary>
    private ARRaycastHit CastARRay()
    {
        Vector2 screenPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        List<ARRaycastHit> hitInfo = new List<ARRaycastHit>();

        if (raycastManager.Raycast(screenPoint, hitInfo, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
        {
            indicator.SetActive(true);
            indicator.transform.position = hitInfo[0].pose.position;
            indicator.transform.rotation = hitInfo[0].pose.rotation;
            indicator.transform.forward = Vector3.up;
        }
        else
        {
            indicator.SetActive(false);
        }

        return hitInfo[0];
    }
}
