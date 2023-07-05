using Photon.Pun;
using System;
using UnityEngine;

public class NewItemScript : MonoBehaviourPunCallbacks
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
    void OnTriggerEnter(Collider other) {
        print(other.name);
        print(other.tag);
        //プレイヤー以外は無視
        if (!other.gameObject.CompareTag("Player")) {
            return;
        }

        if(photonView.IsMine) {
            print("IsMine");
        }else if(!photonView.IsMine) {
            print("Not IsMine");
        }
        //アイテムの列挙型の最大値の中からランダムでアイテムを取得
        var b = UnityEngine.Random.Range(0,itemNameCnt);
        //アイテムの名前を取得
        PlayerBase.ItemName ii = (PlayerBase.ItemName)Enum.ToObject(typeof(PlayerBase.ItemName), b);
        other.gameObject.GetComponent<PlayerBase>().ItemGet(ii);
        Debug.Log("アイテムとれたよ");
        photonView.RPC(nameof(ItemDestroy), RpcTarget.All);
    }

    [PunRPC]
    void ItemDestroy() {
        if(PhotonNetwork.IsMasterClient) {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}