using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refix_Resolutions_And_Ratios : MonoBehaviour {
    [SerializeField]
    private Vector2 aspectVec; //目的解像度

    Camera camera;

    void Start () {
        //カメラのアスペクト比を設定
        camera = Camera.main;//カメラ情報を取得
    }

    private void Update() {
        fixAspectRatio();
    }

    void fixAspectRatio() {
        var screenAspect = Screen.width / (float)Screen.height; //画面のアスペクト比
        var targetAspect = aspectVec.x / aspectVec.y; //目的のアスペクト比

        var magRate = targetAspect / screenAspect; //目的アスペクト比にするための倍率

        var viewportRect = new Rect(0, 0, 1, 1); //Viewport初期値でRectを作成
        viewportRect.width = magRate; //使用する横幅を変更
        camera.rect = viewportRect; //カメラのViewportに適用
    }
}