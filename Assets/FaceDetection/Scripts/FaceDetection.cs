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
    public List<GameObject> masks = new List<GameObject>(); // Mask
    public int maskIndex;

    List<GameObject> regionFeatures = new List<GameObject>(); // Region fetures
    public bool regionFeatureActive;

    List<GameObject> totalFeatures = new List<GameObject>(); // Total 486 features
    public bool totalFeatureActive;

    List<TMP_Text> featureTexts = new List<TMP_Text>();
    [SerializeField] GameObject pointPrefab;
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

        if (args.updated.Count > 0)
        {
            for (int i = 0; i < args.updated[0].vertices.Length; i++)
            {
                Vector3 verPos = args.updated[0].vertices[i];
                Vector3 worldVertPos = args.updated[0].transform.TransformPoint(verPos);

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
        masks[(index - 1 < 0 ? masks.Count - 1 : index - 1) % masks.Count].SetActive(false);
        masks[index].SetActive(true);
        faceManager.facePrefab = masks[index];
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
}
