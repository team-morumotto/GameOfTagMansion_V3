using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharaImageSetScript : MonoBehaviour
{
    [SerializeField] Image[] charaImage = new Image[3]; // 0がユニティちゃん,1がのらねこセブン,2が水鏡こよみ
    
    public void SetCharactorImage(int charanumber){
        //カーソル用のスクリプトに入ってましたけど書く場所絶対そこじゃないと思います
        
            // すべて非表示に.
            foreach(Image ch in charaImage) {
                ch.enabled = false;
            }
            switch(charanumber){
                case 0: // ユニティちゃん.
                    charaImage[0].enabled = true;

                    // 他のキャラクターの画像を非表示に.
                    charaImage[1].enabled = false;
                    charaImage[2].enabled = false;
                    break;
                case 1: // のらねこセブン.
                    charaImage[1].enabled = true;

                    // 他のキャラクターの画像を非表示に.
                    charaImage[0].enabled = false;
                    charaImage[2].enabled = false;
                    break;
                case 2: // 水鏡こよみ.
                    charaImage[2].enabled = true;

                    // 他のキャラクターの画像を非表示に.
                    charaImage[0].enabled = false;
                    charaImage[1].enabled = false;
                    break; 
                default:
                    Debug.LogError("【ButtonCursolScript.cs】: 存在しないキャラクターの画像を読み込もうとしています。");
                    break;
            }
            /*
            // ユニティちゃん.
            if(gameObject.name.Contains("Unity")) {
                charaImage[0].enabled = true;
            // のらねこセブン.
            }else if(gameObject.name.Contains("Noraneko")) {
                charaImage[1].enabled = true;
            // 水鏡こよみ.
            }else if(gameObject.name.Contains("Mikagami")) {
                charaImage[2].enabled = true;
            }else{
                Debug.LogError("【ButtonCursolScript.cs】: 存在しないキャラクターの画像を読み込もうとしています。");
            }
            */
        }
    }

