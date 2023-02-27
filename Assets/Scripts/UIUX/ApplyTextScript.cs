using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//渡されたテキストを自分に設定するだけのスクリプトです
public class ApplyTextScript : MonoBehaviour
{
    //渡されたテキストを自分に設定する
    public void SetText(string text){
       this.GetComponent<Text>().text = text;
    }
}
