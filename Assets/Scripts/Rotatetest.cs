using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotatetest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // transformを取得
        Transform myTransform = this.transform;
 
        // ワールド座標を基準に、回転を取得
        Vector3 worldAngle = myTransform.eulerAngles;
        worldAngle.x = 0.0f; // ワールド座標を基準に、x軸を軸にした回転を10度に変更
        worldAngle.y += 1.0f; // ワールド座標を基準に、y軸を軸にした回転を10度に変更
        worldAngle.z = 0.0f; // ワールド座標を基準に、z軸を軸にした回転を10度に変更
        myTransform.eulerAngles = worldAngle; // 回転角度を設定
    }
}
