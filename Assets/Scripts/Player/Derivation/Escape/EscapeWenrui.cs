using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeWenrui : PlayerEscape
{
    private List<GameObject> billList = new List<GameObject>(); // 御札のリスト.
    private int billAmount = 6; // 同時に展開する御札の枚数.
    private float radius = 5.0f; // 展開する距離.
    private float abilityTime = 20.0f; // 固有能力の発動時間.
    private float rotationSpeed = 100.0f; // 展開した御札が回転する速度.
    private float coolTime = 10.0f; // クールタイム.
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(WenruiES), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update () {
        if(!photonView.IsMine) {
            return;
        }
        if(Input.GetKeyDown(KeyCode.I) && !isUseAvility && !isCoolTime) {
            isUseAvility = true;
            StartCoroutine(BillCircle());
        }

        if(isUseAvility) {
            // キャラが移動した場合でも正確に追いかけてくるようにする.
        }
        BaseUpdate();
    }

    void OnTriggerEnter(Collider collider) {
        // 当たったオブジェクトがアイテムボックスなら.
        if(collider.CompareTag("ItemBox")) {

        }

        // すでにスタンしているなら処理しない.
        if(isStan) {
            print("スタン済み");
            return;
        }

        // 当たったオブジェクトが障害物なら.
        if(collider.CompareTag("Obstruct")) {
            isHit++;
            Destroy(collider.gameObject); // 破壊.
            StartCoroutine(Stan());
        }

        // 当たったオブジェクトが御札なら.
        if(collider.CompareTag("Bill")) {
            // 自分が生成した御札に触れてスタンするのを防ぐ.
            foreach(var bills in billList) {
                if(collider.gameObject == bills) {
                    return;
                }
            }

            isSlow = true; // 移動速度低下状態に.
            ChangeFlg(isSlow, 5.0f); // 5秒間移動速度低下.
        }
    }

    [PunRPC]
    private void WenruiES() {
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
    private IEnumerator BillCircle() {
        var deltaTime = 0.0f;
        billList = new List<GameObject>();

        for(int i = 0; i < billAmount; i++) {
            float angle = i * (360f / billAmount);  // 角度を計算

            // 角度に基づいて配置する位置を計算
            Vector3 position = transform.position + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 0.1f;

            // オブジェクトを配置
            var tmpBill = PhotonNetwork.Instantiate("Bill", position, Quaternion.identity);
            billList.Add(tmpBill);
        }

        var tmpRadius = 0.0f; // 膨張する展開距離.

        while(deltaTime < abilityTime) {
            // 指定の範囲まで展開されるまで広がる.
            if(tmpRadius < radius) {
                tmpRadius += 0.1f;
            }
            for(int i = 0; i < billAmount; i++) {
                var relative = billList[i].transform.position - transform.position; // 方向ベクトル.
                relative = relative.normalized; // 正規化.
                billList[i].transform.position = transform.position + (relative * tmpRadius); // 方向ベクトルに距離を乗算し展開.
                billList[i].transform.RotateAround(transform.position, Vector3.up, (i + 1) * rotationSpeed * Time.deltaTime); // 回転.
            }
            deltaTime += Time.deltaTime;
            yield return null;
        }

        foreach(var bill in billList) {
            Destroy(bill); // 展開した御札を破棄.
        }

        // 発動終了.
        isUseAvility = false;
        StartCoroutine(AvillityCoolTime(10.0f));
    }
}