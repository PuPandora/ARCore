using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class GPSManager : MonoBehaviour
{
    [SerializeField] TMP_Text latitudeText;
    [SerializeField] TMP_Text longtitudeText;
    [SerializeField] TMP_Text logText;
    [SerializeField] TMP_Text stateText;
    [SerializeField] float gpsRenewalTime = 1f;
    public List<GPS> gps;
    [SerializeField] float minDistance;

    double latitude = 0;
    double longtitude = 0;

    CompassManager compassManager;
    List<GameObject> locationObjects = new List<GameObject>();

    private short testCounter = short.MinValue;
    [SerializeField] TMP_Text testCounterText;

    void Awake()
    {
        compassManager = FindAnyObjectByType<CompassManager>();
    }

    void Start()
    {
#if UNITY_EDITOR
        CreateLocationObject();
#endif
        StartCoroutine(TurnOnGPS());
    }

    /// <summary>
    /// Meter 단위로 계산된 거리를 지자계 센서의 각도 기준으로 회전 변환하여
    /// <br></br>
    /// 오브젝트를 생성하는 함수
    /// </summary>
    private void CreateLocationObject()
    {
        string log = string.Empty;

        foreach (var location in gps)
        {
            float scaledPosX = (float)(location.latitude - latitude) * 100_000;
            float scaledPosY = (float)(location.longtitude - longtitude) *100_000;
            log += $"\n{location.latitude} - {latitude:F6} : {location.latitude - latitude}";
            log += $"\n{location.longtitude} - {longtitude:F6} : {location.longtitude - longtitude}";
            log += $"\nInput.location.lastData.longitude : {Input.location.lastData.longitude}\n";

            float newX = Mathf.Cos(compassManager.Angle) * scaledPosX + (-Mathf.Sin(compassManager.Angle) * scaledPosX);
            float newY = Mathf.Sin(compassManager.Angle) * scaledPosY + Mathf.Cos(compassManager.Angle) * scaledPosY;
            Vector3 objPos = new Vector3(newX, 0, newY);

            log += $"{location.name} \n";
            log += $"scaledPosX : {scaledPosX} / scaledPosY : {scaledPosY}\n";
            // 3751365, 12703062 -> 100m, 20m
            log += $"newX : {newX} / newY : {newY}\n";

            GameObject locationObject = null;
            try
            {
                GameObject prefab = Resources.Load<GameObject>(location.resourceName);

                if (prefab != null)
                {
                    locationObject = Instantiate(prefab);
                    locationObject.name = location.name;
                    locationObject.transform.position = objPos;
                    locationObject.SetActive(false);
                }
                else
                {
                    logText.text += "No prefab exsist.";
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("지정된 GPS Location 정보의 resourceName으로 Resources.Load 호출 중 에러가 발생했습니다." +
                    "Resources 폴더에 해당 이름의 프리팹이 있는지 확인해주세요.\n" +
                    $"시도된 Resource 이름 : {location.resourceName}");
            }

            if (locationObjects != null)
            {
                locationObjects.Add(locationObject);
            }
            else
            {
                throw new System.NullReferenceException("locationObjects 리스트가 Null 상태입니다.");
            }

            log += "\n";
            logText.text = log;
        }

        logText.text += testCounter;
    }

    /// <summary>
    /// GPS의 위치 정보들과 현재 위치 사이의 거리를 계산하는 함수
    /// </summary>
    private void CalculateDistance()
    {
        string totalDistance = string.Empty;
        foreach (var location in gps)
        {
            float distance = CalculateDistance(latitude, longtitude, location.latitude, location.longtitude);

            GameObject locationObj = locationObjects.Find(obj => obj.name == location.name);

            // 거리에 따라 오브젝트 활성화
            if (distance < minDistance)
            {
                locationObj.SetActive(true);
            }
            else
            {
                locationObj.SetActive(false);
            }

            totalDistance += $"{location.name} {distance:F1}m, \n";
        }

        logText.text = totalDistance;
    }

    private float CalculateDistance(double fromX, double fromY, double toX, double toY)
    {
        // 피타고라스 정리 : x제곱, y제곱의 제곱근
        float distance = Mathf.Sqrt(Mathf.Pow((float)(toX - fromX), 2) + Mathf.Pow((float)(toY - fromY), 2));
        // meter 단위로 변환
        distance *= 100_000;

        return distance;
    }

    /// <summary>
    /// GPS 권한을 받아 위도, 경도 위치를 갱신하는 함수
    /// </summary>
    private IEnumerator TurnOnGPS()
    {
        // 위치 정보 요청
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);

            if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                break;
            }
        }

        if (!Input.location.isEnabledByUser)
        {
            stateText.text = "GPS access failed";

            yield return new WaitForSeconds(3f);

            Application.Quit();

            yield break;
        }

        Input.location.Start(0.1f);

        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            stateText.text = "Location Service Initializing";

            latitude = Input.location.lastData.latitude;
            longtitude = Input.location.lastData.longitude;

            yield return new WaitForSeconds(1f);
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            stateText.text = "Initialization failed";

            yield return new WaitForSeconds(3f);

            Application.Quit();

            yield break;
        }

        CreateLocationObject();

        // 위도, 경도 갱신
        while (Input.location.status == LocationServiceStatus.Running)
        {
            yield return new WaitForSeconds(gpsRenewalTime);

            latitude = Input.location.lastData.latitude;
            longtitude = Input.location.lastData.longitude;

            latitudeText.text = $"{latitude:F6}";
            longtitudeText.text = $"{longtitude:F6}";

            CalculateDistance();

            testCounter++;
            testCounterText.text = testCounter.ToString();
            stateText.text = "Location Service Running";
        }

        stateText.text = "Location Service Disabled";
    }
}
