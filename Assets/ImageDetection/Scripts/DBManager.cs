using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;

// Firebase Database에 접근하여 데이터를 쓰고 / 받아온다
public class DBManager : MonoBehaviour
{
    [SerializeField] string dataURL;
    GPSManager gpsManager;
    DatabaseReference dbReference;

    void Awake()
    {
        gpsManager = FindAnyObjectByType<GPSManager>();
    }

    void Start()
    {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dataURL);

        StartCoroutine(SendData());
    }

    private IEnumerator SendData()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;


        //foreach (var gps in gpsInfo)
        //{
        //    string json = JsonUtility.ToJson(gps);
        //    dbReference.Child("gps").Child(gps.name).SetRawJsonValueAsync(json);
        //}

        // GPS 정보 저장하기
        foreach (var gps in gpsManager.gps)
        {
            string json = JsonUtility.ToJson(gps);
            var task = dbReference.Child(gps.name).SetRawJsonValueAsync(json);
            print(json);

            yield return new WaitUntil(() => task.IsCompleted);
        }

        yield return RequestData();
    }

    private IEnumerator RequestData()
    {
        var task = dbReference.GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        DataSnapshot snapShot = task.Result;

        foreach (var data in snapShot.Children)
        {
            string json = data.GetRawJsonValue();

            print(json);
        }
    }
}
