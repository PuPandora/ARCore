using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

public class FaceDetection : MonoBehaviour
{
    [Header("Feature Group")]
    // Mask
    [SerializeField] List<GameObject> masks = new List<GameObject>();
    [SerializeField] int maskIndex;

    // Region fetures
    List<GameObject> regionFeatures = new List<GameObject>();
    public bool regionFeatureActive;

    // Total 486 features
    List<GameObject> totalFeatures = new List<GameObject>();
    public bool totalFeatureActive;

    // Points
    List<TMP_Text> featureTexts = new List<TMP_Text>();
    [SerializeField] GameObject pointPrefab;

    [Header("Christmas Group")]
    [SerializeField] Accessory[] christmasAccessories;
    [SerializeField] Accessory[] currentAccessories;

    ARCoreFaceSubsystem faceSubSystem;
    ARFaceManager faceManager;
    NativeArray<ARCoreFaceRegionData> faceRegions;

    void Awake()
    {
        faceManager = GetComponent<ARFaceManager>();
    }

    void Start()
    {
        for (int i = 0; i < 3; i++) // region features
        {
            GameObject featureObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            featureObj.transform.SetParent(transform);
            featureObj.name = "RegionPoint " + i;
            featureObj.transform.localScale = Vector3.one * 0.02f;

            regionFeatures.Add(featureObj);
            featureObj.SetActive(false);
        }

        for (int i = 0; i < 468; i++) // total 468 features
        {
            GameObject featureObj = Instantiate(pointPrefab, transform);
            featureObj.name = "Point " + i;

            var tmp = featureObj.GetComponentInChildren<TMP_Text>();
            tmp.text = "0";
            featureTexts.Add(tmp);

            totalFeatures.Add(featureObj);
            featureObj.SetActive(false);
        }

        faceSubSystem = (ARCoreFaceSubsystem)faceManager.subsystem;

        faceManager.facesChanged += OnDetectedRegionPos;
        faceManager.facesChanged += OnDetectedTotalFeaturePos;
        faceManager.facesChanged += LocateAccessories;
    }

    /// <summary>
    /// 코, 이마에 Region Feature를 위치시키는 함수
    /// </summary>
    private void OnDetectedRegionPos(ARFacesChangedEventArgs args)
    {
        if (!regionFeatureActive) return;

        if (args.updated.Count > 0)
        {
            faceSubSystem.GetRegionPoses(args.updated[0].trackableId, Allocator.Persistent, ref faceRegions);

            // 코(0), 왼쪽 이마(1), 오른쪽 이마 (2)
            for (int i = 0; i < faceRegions.Length; i++)
            {
                regionFeatures[i].transform.position = faceRegions[i].pose.position;
                regionFeatures[i].transform.rotation = faceRegions[i].pose.rotation;
                //regionFeatures[i].SetActive(true);
            }
        }
        else if (args.removed.Count > 0)
        {
            for (int i = 0; i < faceRegions.Length; i++)
            {
                regionFeatures[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Total Feature 들을 얼굴의 각 Vertex에 위치 시키는 함수
    /// </summary>
    private void OnDetectedTotalFeaturePos(ARFacesChangedEventArgs args)
    {
        if (!totalFeatureActive) return;

        Vector3 verPos = Vector3.zero;
        Vector3 worldVertPos = Vector3.zero;

        if (args.updated.Count > 0)
        {
            for (int i = 0; i < args.updated[0].vertices.Length; i++)
            {
                verPos = args.updated[0].vertices[i];
                worldVertPos = args.updated[0].transform.TransformPoint(verPos);

                totalFeatures[i].transform.position = worldVertPos;
                //totalFeatures[i].SetActive(true);

                featureTexts[i].text = i.ToString();
            }
        }
        else if (args.removed.Count >= 0)
        {
            for (int i = 0; i < totalFeatures.Count; i++)
            {
                totalFeatures[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// 다음 마스크로 교체하는 함수
    /// </summary>
    public void ChangeMask()
    {
        int index = ++maskIndex % masks.Count;
        masks[index - 1 < 0 ? masks.Count - 1 : index - 1].SetActive(false);
        masks[index].SetActive(true);
        faceManager.facePrefab = masks[index];

        Destroy(FindObjectOfType<ARFace>().gameObject);
        faceManager.enabled = false;
        faceManager.enabled = true;
    }

    /// <summary>
    /// Region Feature를 전부 켜고 끄는 함수
    /// </summary>
    public void ToggleRegionFeatures()
    {
        regionFeatureActive = !regionFeatureActive;

        foreach (var feature in regionFeatures)
        {
            feature.SetActive(regionFeatureActive);
        }
    }

    /// <summary>
    /// Total Feature를 전부 켜고 끄는 함수
    /// </summary>
    public void ToggleTotalFeatures()
    {
        totalFeatureActive = !totalFeatureActive;

        foreach (var feature in totalFeatures)
        {
            feature.SetActive(totalFeatureActive);
        }
    }

    /// <summary>
    /// 악세서리를 각 위치로 이동시키는 함수
    /// </summary>
    private void LocateAccessories(ARFacesChangedEventArgs args)
    {
        if (currentAccessories == null) return;

        if (args.updated.Count > 0)
        {
            foreach (var item in currentAccessories)
            {
                item.InitSetting();
                item.Locate(args);
            }
        }
        else
        {
            foreach (var item in currentAccessories)
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 크리스마스 악세서리 토글
    /// </summary>
    public void ToggleChristmasAccessories()
    {
        if (currentAccessories == null)
        {
            currentAccessories = christmasAccessories;
            foreach (var item in currentAccessories)
            {
                item.InitSetting();
            }
        }
        else
        {
            foreach (var item in currentAccessories)
            {
                item.gameObject.SetActive(false);
            }
            currentAccessories = null;
        }
    }
}
