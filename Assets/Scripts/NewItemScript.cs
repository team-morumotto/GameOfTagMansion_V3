using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class NewItemScript : MonoBehaviour
{
    [SerializeField] private Vector3 rotateSpeed = new Vector3(5,5,5);
    [SerializeField] private GameObject rotateObject;
    private int itemNameCnt = 0; // アイテムの種類の列挙体の数.
    void Start() {
        //アイテムの列挙型の最大値を取得
        itemNameCnt = System.Enum.GetValues(typeof(PlayerBase.ItemName)).Length;
    }
    void Update() {
        //まわす
        rotateObject.transform.Rotate(rotateSpeed);
        ItemLookCamera();
    }

    ///<sammary>
    ///ビルボードみたいにカメラに向ける
    ///</sammary>
    void ItemLookCamera() {
        //カメラに向ける
        var c = Camera.main.transform.position;
        var p = transform.position;
        c.y = p.y;
        transform.LookAt(2 * p - c);
    }

    ///<sammary>
    ///プレイヤーがアイテムを取得したときの処理
    ///</sammary>
    void OnCollisionEnter(Collision collision) {
        //プレイヤー以外は無視
        if (collision.gameObject.tag != "Player") {
            return;
        }
        //アイテムの列挙型の最大値の中からランダムでアイテムを取得
        var b = UnityEngine.Random.Range(0,itemNameCnt);
        //アイテムの名前を取得
        PlayerBase.ItemName ii = (PlayerBase.ItemName)Enum.ToObject(typeof(PlayerBase.ItemName), 8);
        collision.gameObject.GetComponent<PlayerBase>().ItemGet(ii);
        Debug.Log("アイテムとれたよ");
        Destroy(this.gameObject);
    }
}