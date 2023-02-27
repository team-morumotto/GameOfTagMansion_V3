/*
    Atsuki Kobayashi
    2022/12/14 作成者の名前を追記
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerSet : MonoBehaviour
{
    [SerializeField] private Dropdown MemberDD;// Dropdownを入れる変数.
    public static int GamePlayers;             // ゲームのプレイヤー人数.
    void Update(){
        if(MemberDD.value == 0){
            GamePlayers = 2;
        }
        else if(MemberDD.value == 1){
            GamePlayers = 3;
        }
        else if(MemberDD.value == 2){
            GamePlayers = 4;
        }
    }
}