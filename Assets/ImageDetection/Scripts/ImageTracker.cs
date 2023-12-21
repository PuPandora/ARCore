using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageTracker : MonoBehaviour
{
    ARTrackedImageManager imageManager;

    void Awake()
    {
        imageManager = GetComponent<ARTrackedImageManager>();

        imageManager.trackedImagesChanged += OnImageTrackedEvent;
    }

    private void OnImageTrackedEvent(ARTrackedImagesChangedEventArgs args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            string imageName = trackedImage.referenceImage.name;

            GameObject objPrefab = Resources.Load<GameObject>(imageName);

            if (objPrefab != null)
            {
                GameObject obj = Instantiate(
                    objPrefab, 
                    trackedImage.transform.position, 
                    trackedImage.transform.rotation);
            }
        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            if (trackedImage.transform.childCount > 0)
            {
                trackedImage.transform.GetChild(0).gameObject.SetActive(true);
                trackedImage.transform.GetChild(0).position = trackedImage.transform.position;
                trackedImage.transform.GetChild(0).rotation = trackedImage.transform.rotation;
            }
        }

        foreach (ARTrackedImage trackedImage in args.removed)
        {
            //if (trackedImage.)
        }
    }
}
