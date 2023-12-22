using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 지자계 센서를 이용하여 북쪽으로 부터 얼마나 회전했는지 확인
public class CompassManager : MonoBehaviour
{
    [SerializeField] GameObject compassObj;
    [SerializeField] TextMeshProUGUI angleText;
    public int Angle { get => angle; }
    private int angle;

    void Start()
    {
        Input.compass.enabled = true;

        angle = angle = Mathf.RoundToInt(Input.compass.trueHeading);
    }

    void Update()
    {
        angle = Mathf.RoundToInt(Input.compass.trueHeading);
        angleText.text = angle.ToString() + "˚";

        compassObj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
