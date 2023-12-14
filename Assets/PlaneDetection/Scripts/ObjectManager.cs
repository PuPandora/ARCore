using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.XR.ARFoundation;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] GameObject indicator;
    [SerializeField] GameObject objectPrefab;
    ARRaycastManager raycastManager;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Start()
    {
        objectPrefab.SetActive(false);
    }

    void Update()
    {
        CastARRay();
    }

    /// <summary>
    /// AR Ray를 발사하여 indicator를 위치시킨다.
    /// </summary>
    private void CastARRay()
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
    }
}
