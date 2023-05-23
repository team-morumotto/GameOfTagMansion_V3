using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SpeedUpItem : MonoBehaviourPunCallbacks
{
    void Start() {
        StartCoroutine(Destroy());
    }

    void Update(){
        transform.Rotate(0,1,0);           // 回転させる.
    }

    void OnTriggerEnter(Collider collision){
        // 自分でない場合.
        if(!photonView.IsMine) {
            return;
        }

        // 触れたオブジェクトのタグがNigeruかOniの場合.
        if(collision.gameObject.tag == "Nige" || collision.gameObject.tag == "Oni"){
            PhotonNetwork.Destroy(gameObject); // このオブジェクトを消す.
        }
    }

    // 生成してから10秒後に消す.
    IEnumerator Destroy(){
        yield return new WaitForSeconds(10.0f);
        PhotonNetwork.Destroy(gameObject);
    }
}