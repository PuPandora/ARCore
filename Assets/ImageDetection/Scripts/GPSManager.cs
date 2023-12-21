using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class GPSManager : MonoBehaviour
{
    [SerializeField] TMP_Text latitudeText;
    [SerializeField] TMP_Text longtitudeText;
    [SerializeField] float gpsRenewalTime = 1f;
    [SerializeField] List<GPS> gps;

    double latitude = 0;
    double longtitude = 0;

    void Start()
    {
        StartCoroutine(TurnOnGPS());
    }

    private void CalculateDistance()
    {
        foreach (var location in gps)
        {
            float distance = CalculateDistance(latitude, longtitude, location.latitude, location.longtitude);
        }
    }

    private float CalculateDistance(double fromX, double fromY, double toX, double toY)
    {
        // 피타고라스 정리 : x제곱, y제곱의 제곱근
        float distance = Mathf.Sqrt(Mathf.Pow((float)(toX - fromX), 2) + Mathf.Pow((float)(toY - fromY), 2));
        // meter 단위로 변환
        distance *= 100000;

        return distance;
    }

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
            latitudeText.text = "GPS access failed";

            yield return new WaitForSeconds(3f);

            Application.Quit();

            yield break;
        }

        Input.location.Start();

        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            latitudeText.text = "Location Service Initializing";

            yield return new WaitForSeconds(1f);
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            latitudeText.text = "Initialization failed";

            yield return new WaitForSeconds(3f);

            Application.Quit();

            yield break;
        }

        // 위도, 경도 갱신
        while (Input.location.status == LocationServiceStatus.Running)
        {
            yield return new WaitForSeconds(gpsRenewalTime);

            latitude = Input.location.lastData.latitude;
            longtitude = Input.location.lastData.longitude;

            latitudeText.text = $"{latitude:F6}";
            longtitudeText.text = $"{longtitude:F6}";

            CalculateDistance();
        }
    }
}
