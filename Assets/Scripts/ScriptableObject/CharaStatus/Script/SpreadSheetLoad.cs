/*
*   Created by Kobayashi Atsuki.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LitJson; // ジャグ配列を扱うため.

public class SpreadSheetLoad : MonoBehaviour
{
    private const string URL = "https://sheets.googleapis.com/v4/spreadsheets/1OpXaK6mU510wPBMVo268AhOjKXPyE8RRH5h1RYXy3CA/values/Sample?key=AIzaSyDBTmUnPKAeyHFV_xYO5ZGpbAWkUuDUQUk";
    private const int SHEET_HORIZONTAL = 7;
    private const int SHEET_VERTICAL = 10;
    public CharacterDatabase characterDatabase;
    void Start() {
        StartCoroutine(connect());
    }

    IEnumerator connect(){
        using (var www = UnityWebRequest.Get(URL)) {
        yield return www.SendWebRequest();

            switch(www.result) {
                case UnityWebRequest.Result.Success:
                    print("Connect");

                    var jsonData = JsonMapper.ToObject(www.downloadHandler.text);
                    CharacterStatusSet(jsonData);
                break;

                default:
                    print("Error");
                    print(www.error);
                break;
            }
        }
    }

    /// <summary>
    /// スプレッドシートからキャラクターごとのステータスを取得し、代入する.
    /// </summary>
    /// <param name="jsonData">スプレッドシートのJson形式データ</param>
    private void CharacterStatusSet(JsonData jsonData) {
        for(int i = 0; i < SHEET_VERTICAL; i++) {
            for(int j = 0; j < SHEET_HORIZONTAL; j++) {
                switch(j) {
                    case 0: characterDatabase.statusList[i].charaName = jsonData["values"][i + 1][j].ToString();
                    break;

                    case 1: characterDatabase.statusList[i].walkSpeed = JsonDataToFloat(jsonData["values"][i + 1][j]);
                    break;
                    
                    case 2: characterDatabase.statusList[i].runSpeed = JsonDataToFloat(jsonData["values"][i + 1][j]);
                    break;

                    case 3: characterDatabase.statusList[i].staminaAmount = JsonDataToFloat(jsonData["values"][i + 1][j]);
                    break;

                    case 4: characterDatabase.statusList[i].staminaHealAmount = JsonDataToFloat(jsonData["values"][i + 1][j]);
                    break;
                    
                    case 5: characterDatabase.statusList[i].overCome = JsonDataToBoolean(jsonData["values"][i + 1][j]);
                    break;
                    
                    case 6: characterDatabase.statusList[i].floating = JsonDataToBoolean(jsonData["values"][i + 1][j]);
                    break;
                    
                    default:
                    break;
                }
            }
        }
    }

    /// <summary>
    /// JsonData型の数値文字列をfloat型に変換する.
    /// </summary>
    /// <param name="jsonData">JsonData</param>
    /// <returns>float に変換した値</returns>
    private float JsonDataToFloat(JsonData jsonData) {
        var tmp = jsonData.ToString();
        return float.Parse(tmp);
    }

    /// <summary>
    /// JsonData型の真偽値文字列をbool型に変換する.
    /// </summary>
    /// <param name="jsonData">JsonData</param>
    /// <returns>boolに変換した値</returns>
    private bool JsonDataToBoolean(JsonData jsonData) {
        var tmp = jsonData.ToString();
        return Convert.ToBoolean(tmp);
    }
}
