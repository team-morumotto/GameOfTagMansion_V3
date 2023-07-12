using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeNoranekoSeven : PlayerEscape
{
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(NoranekoES), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    float distance = 0.0f;
    Vector3 bb = Vector3.zero;

    void Update () {
        if(!photonView.IsMine) {
            return;
        }
        if(Input.GetKeyDown(KeyCode.Space)) {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) {
                isUseAvility = true;
            }
            bb = hit.point;
            distance = (hit.point - transform.position).magnitude;
        }
        BaseUpdate();

        print(bb);
        print(distance);
        if(isUseAvility) {
            OverCome();
        }
    }

    [PunRPC]
    private void NoranekoES() {
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
    void OverCome() {
        var aa = (Time.deltaTime * 1.0f) / distance;
        transform.position = Vector3.Slerp(transform.position, transform.forward, aa);

        if((bb - transform.position).magnitude < 1.0f) {
            isUseAvility = false;
        }
    }
}