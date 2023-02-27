/*
    プレイヤーの足がFloorタグの付いたオブジェクトに触れたら足音を鳴らす.
*/

using UnityEngine;
using Photon.Pun;

public class Player_LegSE : MonoBehaviourPunCallbacks
{
    private AudioSource audioSource; // AudioSource取得.
    void Start() {
        if(!photonView.IsMine) {
            return;
        }
        audioSource = GetComponent<AudioSource>(); // AudioSourceComponent取得.
    }

    private void OnTriggerEnter(Collider collider) {
        if(!photonView.IsMine) {
            return;
        }
        if(collider.gameObject.tag == "Floor") {
            audioSource.Play(); // 音を鳴らす.
        }
    }
}