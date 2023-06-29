using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum PlayerType {
    Chaser,
    Escape
}

public class CharacterPreviewManager : MonoBehaviour {
    [SerializeField] GameObject[] characterPreviewObject; // キャラプレビュー(object)
    [SerializeField] Text characterPreviewNameText;       // キャラ名前(text)
    [SerializeField] Text characterPreviewSkillText;      // キャラスキル(text)
    [SerializeField] Text characterPreviewSpecText;       // キャラ性能(text)
    [SerializeField] Text characterPreviewOtherText;      // キャラ備考(text)
    private PlayerType playerType;                        // PlayerType.Chaser: 鬼, PlayerType.Escape: 逃げ
    // Start is called before the first frame update
    void Start() {
        //初期化
        characterPreview(0);
    }
    /// <summary>
    /// キャラクターのプレビューを切り替える
    /// 0: トラスとウェッジ
    /// 1: リルモア
    /// 2: 水鏡こよみ
    /// 3: NoranekoSeven
    /// 4: シャーロ Pure
    /// 5: ミュリシア
    /// 6: ウェンルイ
    /// 7: ミーシェ
    /// 8: 朝霞やのは
    /// 9: ナユ
    /// </summary>
    public void characterPreview(int cr) {
        for(int i = 0; i < characterPreviewObject.Length; i++) {
            characterPreviewObject[i].SetActive(false);
        }
        switch(cr){
            case 0:
                characterPreviewObject[0].SetActive(true);
                characterPreviewNameText.text = "トラスとウェッジ";
                if(playerType == PlayerType.Chaser) {
                    characterPreviewSkillText.text = "爆発物を前に投げて邪魔する";
                }
                else if(playerType == PlayerType.Escape) {
                    characterPreviewSkillText.text = "爆発物を後ろに設置して邪魔する";
                }
                characterPreviewSpecText.text = "移動速度: 1.5 (ダッシュ時2.0)\n体力: 15.0\n回復速度: 1.0\n";
                characterPreviewOtherText.text = "逃げ切ることも邪魔もできる";
                break;
            case 1:
                characterPreviewObject[1].SetActive(true);
                characterPreviewNameText.text = "リルモア";
                characterPreviewSkillText.text = "高いところへ飛べる他、足音が聞こえない";
                characterPreviewSpecText.text = "移動速度: 1.0 (ダッシュ時3.0)\n体力: 10.0\n回復速度: 2.0\n";
                characterPreviewOtherText.text = "忍び寄ることに特化している分、体力がない";
                break;
            case 2:
                characterPreviewObject[2].SetActive(true);
                if(playerType == PlayerType.Chaser.Escape) {
                    characterPreviewNameText.text = "水鏡こよみ";
                    characterPreviewSkillText.text = "小さくなりステルス性を高める";
                    characterPreviewSpecText.text = "移動速定: 1.0 (ダッシュ時1.5)\n体力: 5.0\n回復速度: 2.0\n";
                    characterPreviewOtherText.text = "隠れている際は体力を使わないが、体力がない";
                }
                break;
            case 3:
                characterPreviewObject[3].SetActive(true);
                characterPreviewNameText.text = "NoranekoSeven";
                if(playerType == PlayerType.Chaser) {
                    characterPreviewSkillText.text = "物に化けて隠れられる";
                }
                else if(playerType == PlayerType.Escape) {
                    characterPreviewSkillText.text = "壁を越えられる";
                }
                characterPreviewSpecText.text = "移動速度: 1.5 (ダッシュ時2.0)\n体力: 10.0\n回復速度: 1.5\n";
                characterPreviewOtherText.text = "物に化けたり走ると体力を消耗";
                break;
            case 4:
                characterPreviewObject[4].SetActive(true);
                characterPreviewNameText.text = "シャーロ";
                if(playerType == PlayerType.Chaser) {
                    characterPreviewSkillText.text = "3回だけ逃げの位置を表示";
                }
                else if(playerType == PlayerType.Escape) {
                    characterPreviewSkillText.text = "3回だけ鬼の位置を表示";
                }
                characterPreviewSpecText.text = "移動速度: 1.0 (ダッシュ時1.5)\n体力: 20.0\n回復速度: 1.0\n";
                characterPreviewOtherText.text = "逃げで使うと味方も鬼の場所が見れる";
                break;
            case 5:
                characterPreviewObject[5].SetActive(true);
                characterPreviewNameText.text = "ミュリシア";
                if(playerType == PlayerType.Chaser) {
                    characterPreviewSkillText.text = "近くに逃げがいると反応する";
                }
                else if(playerType == PlayerType.Escape) {
                    characterPreviewSkillText.text = "一度だけ捕まっても回避できる";
                }
                characterPreviewSpecText.text = "移動速定: 1.5 (ダッシュ時2.5)\n体力: 15.0/20.0\n回復速度: 1.5/2.0\n";
                characterPreviewOtherText.text = "ポテトを食べると性能が上がる";
                break;
            case 6:
                characterPreviewObject[6].SetActive(true);
                characterPreviewNameText.text = "ウェンルイ";
                if(playerType == PlayerType.Chaser) {
                    characterPreviewSkillText.text = "御札を使って範囲に入ってきた逃げの動きを一定時間鈍らせる";
                }
                else if(playerType == PlayerType.Escape) {
                    characterPreviewSkillText.text = "御札を使って範囲に入ってきた鬼の動きを一定時間封じる";
                }
                characterPreviewSpecText.text = "移動速定: 1.5 (ダッシュ時2.0)\n体力: 10.0\n回復速度: 1.0\n";
                characterPreviewOtherText.text = "御札は3回まで。範囲はあまり大きくない";
                break;
            case 7:
                characterPreviewObject[7].SetActive(true);
                characterPreviewNameText.text = "ミーシェ";
                characterPreviewSkillText.text = "アイテムをホールド（持ち運ぶ）できる他、足音が視覚的に見える";
                characterPreviewSpecText.text = "移動速定: 1.0 (ダッシュ時1.5)\n体力: 10.0\n回復速度: 1.0\n";
                characterPreviewOtherText.text = "オールマイティ";
                break;
            case 8:
                characterPreviewObject[8].SetActive(true);
                characterPreviewNameText.text = "朝霞やのは";
                characterPreviewSkillText.text = "アイテムの効果が増幅される";
                characterPreviewSpecText.text = "移動速定: 1.0 (ダッシュ時1.5)\n体力: 10.0\n回復速度: 1.0\n";
                characterPreviewOtherText.text = "組み合わせによっては強い";
                break;
            case 9:
                characterPreviewObject[9].SetActive(true);
                characterPreviewNameText.text = "ナユ";
                if(playerType == PlayerType.Chaser) {
                    characterPreviewSkillText.text = "自分の体力回復が早くなる";
                }
                else if(playerType == PlayerType.Escape) {
                    characterPreviewSkillText.text = "自分と味方の体力回復が早くなる";
                }
                characterPreviewSpecText.text = "移動速度: 1.5 (ダッシュ時2.0)\n体力: 5.0\n回復速度: 2.5\n";
                characterPreviewOtherText.text = "体力回復に長けているので補助に最適";
                break;
            default:
                characterPreviewNameText.text = "";
                characterPreviewSkillText.text = "";
                characterPreviewSpecText.text = "";
                characterPreviewOtherText.text = "";
                break;
        }
    }
    /// <summary>
    /// キャラクターが鬼か逃げかを切り替える
    /// </summary>
    public void SetPlayerType(bool isOni){
        if(isOni) {
            playerType = PlayerType.Chaser;
        }
        else {
            playerType = PlayerType.Escape;
        }
        characterPreview(0);
    } 
}
