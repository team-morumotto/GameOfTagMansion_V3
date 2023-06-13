using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VirtualCameraManager : MonoBehaviour
{
    [SerializeField]
    Transform resetLookAt;
    [SerializeField]
    CinemachineFreeLook cf;
    private Vector3 firstPos;
    private Quaternion firstRot;
    private GameObject virtualCamera;

    void Start()
    {
        firstPos = transform.position; // 初期位置を記憶.
        firstRot = Quaternion.identity; // 初期回転を記憶.
    }

    /// <summary>
    /// マッチ画面でカメラが映す位置と向きをリセット.
    /// </summary>
    public void TraReset() {
        cf.enabled = false; // 位置や向きが制限されてしまうため無効化.
        transform.position = firstPos;
        transform.rotation = firstRot;
    }
}
