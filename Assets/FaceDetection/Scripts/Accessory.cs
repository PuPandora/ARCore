using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class Accessory : MonoBehaviour
{
    public int vertexIndex;

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void InitSetting()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 정해진 Vertex 위치로 이동하는 함수
    /// </summary>
    public void Locate(ARFacesChangedEventArgs args)
    {
        Vector3 verPos = args.updated[0].vertices[vertexIndex];
        Vector3 worldVerPos = args.updated[0].transform.TransformPoint(verPos);
        gameObject.transform.position = worldVerPos;
    }
}
