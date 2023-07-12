/*
    プレイヤーの足がFloorタグの付いたオブジェクトに触れたら足音を鳴らす.
*/

using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel.Design.Serialization;

public class Player_LegSE : MonoBehaviourPunCallbacks
{
    private AudioSource audioSource; // AudioSource.
    public PlayerBase playerBase; // PlayerBase.
    void Start() {
        audioSource = GetComponent<AudioSource>(); // AudioSource取得.
        playerBase = transform.root.GetComponent<PlayerBase>(); // 最上位親オブジェクトのPlayerBaseを取得.
        StartCoroutine(GetPlayerBase());
    }

    private IEnumerator GetPlayerBase() {
        while(true) {
            yield return null;

            if(null != playerBase) {
                break;
            }else{
                playerBase = transform.root.GetComponent<PlayerBase>(); // 最上位親オブジェクトのPlayerBaseを取得.
            }
        }
    }

    private void OnTriggerEnter(Collider collider) {
        if(collider.gameObject.tag == "Floor") {
            if(photonView.IsMine) {
                // 浮遊キャラである.
                if(playerBase.floating) {
                    // 走っていない場合は足音を出す.
                    if(playerBase.isRunning) {
                        audioSource.Play(); // 音を鳴らす.
                    }
                }else{
                    audioSource.Play(); // 音を鳴らす.
                }
            }else{
                // 浮遊キャラである.
                if(playerBase.floating) {
                    print("bb");
                    // 走っていない場合は足音を出す.
                    if(playerBase.isRunning) {
                        print("cc");
                        audioSource.Play(); // 音を鳴らす.
                    }
                }else{
                    print("dd");
                    audioSource.Play(); // 音を鳴らす.
                }
            }
        }
    }
}