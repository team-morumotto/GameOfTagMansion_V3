using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraRotator3rdPersonPov : MonoBehaviour
{
    //------ static変数 ------//
    public static GameObject playerObject; // 注視するオブジェクト(今回はプレイヤー).

    //------ public変数 ------//
    public Camera Camera;
    public Toggle reverseToggle;                // カメラの回転をReverseするかどうか.
    public float rotateSpeed = 2.0f;            // 回転の速さ.

    //------ private変数 ------//
    private Vector3 totalAngle = Vector3.zero;
    private float maxAngle = 40; // y軸上方向の最大角度.
    private float minAngle = -60;// y軸下方向の最大角度.
    private Vector3 firstPos;
    private Quaternion firstRot;

    void Start() {
        Camera = gameObject.GetComponent<Camera>();
        firstPos = transform.position;
        firstRot = Quaternion.identity;
    }

    void Update() {
        if(playerObject) {
            if(!reverseToggle.isOn) {
                /*
                if(Input.GetKeyDown(KeyCode.Escape)) {
                    if(Cursor.visible) {
                        Cursor.visible = false; // ゲームウィンドウ選択中はカーソルが非表示.
                    }else if(!Cursor.visible) {
                        Cursor.visible = true; // ゲームウィンドウ選択中はカーソルが表示.
                    }
                }*/

                /*
                    カメラの移動向きを指定.
                */
                // Vector3でX,Y方向の回転の度合いを定義.
                Vector3 angle = new Vector3(Input.GetAxis("Mouse X") * rotateSpeed,Input.GetAxis("Mouse Y") * rotateSpeed, 0);

                if(angle != Vector3.zero) {
                    totalAngle += angle;
                    if(totalAngle.y > maxAngle) {
                        totalAngle.y = maxAngle;
                    }else if(totalAngle.y < minAngle) {
                        totalAngle.y = minAngle;
                    }else{
                        Camera.transform.RotateAround(playerObject.transform.position, transform.right, -angle.y);
                    }
                }

                Camera.transform.RotateAround(playerObject.transform.position, Vector3.up, angle.x);
            }else{
                /*
                if(Input.GetKeyDown(KeyCode.Escape)) {
                    if(Cursor.visible) {
                        Cursor.visible = false; // ゲームウィンドウ選択中はカーソルが非表示.
                    }else if(!Cursor.visible) {
                        Cursor.visible = true; // ゲームウィンドウ選択中はカーソルが表示.
                    }
                }*/

                /*
                    カメラの移動向きを指定.
                */
                // Vector3でX,Y方向の回転の度合いを定義.
                Vector3 angle = new Vector3(Input.GetAxis("Mouse X") * rotateSpeed,Input.GetAxis("Mouse Y") * rotateSpeed, 0);

                if(angle != Vector3.zero) {
                    totalAngle += angle;
                    if(totalAngle.y > maxAngle) {
                        totalAngle.y = maxAngle;
                    }else if(totalAngle.y < minAngle) {
                        totalAngle.y = minAngle;
                    }else{
                        Camera.transform.RotateAround(playerObject.transform.position, transform.right, angle.y);
                    }
                }

                Camera.transform.RotateAround(playerObject.transform.position, Vector3.up, -angle.x);
            }
        }
    }

    public void TraReset() {
        transform.position = firstPos;
        transform.rotation = firstRot;
    }
}
