using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class SetFirstButton : MonoBehaviour
{
    public GameObject FirstButton;
    //初期選択ボタンを入れるとコントローラーでUI選択ができる
    //ただしパネルが変わるとまた入れなおさないといけないのでボタンで移動するときは次のパネルの初期選択ボタンを入れる
    void Start(){
        InitButtonSet(FirstButton);
    }
    public void InitButtonSet(GameObject FB){
        //初期化
        EventSystem.current.SetSelectedGameObject(null);
        //初期選択ボタンの再指定
        EventSystem.current.SetSelectedGameObject(FB);
    }

    //パネルがアクティブになったとき
    void OnEnable(){
        InitButtonSet(FirstButton);
    }
}
