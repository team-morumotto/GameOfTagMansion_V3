using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SpeedUpItem : MonoBehaviourPunCallbacks
{
    public ParticleSystem particle;
    void Start() {
        StartCoroutine(Destroy());
    }

    void Update(){
        transform.Rotate(0,1,0);           // 回転させる.
    }

    // 生成してから10秒後に消す.
    IEnumerator Destroy(){
        if(PhotonNetwork.IsMasterClient) {
            yield return new WaitForSeconds(10.0f);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}