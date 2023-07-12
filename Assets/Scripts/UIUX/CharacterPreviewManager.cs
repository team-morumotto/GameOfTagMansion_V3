using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPreviewManager : MonoBehaviour {
    [SerializeField] GameObject[] characterPreviewObject; // キャラプレビュー(object)
    [SerializeField] Text characterPreviewName;       // キャラ名前(text)
    [SerializeField] Text characterPreviewSkill;      // キャラスキル(text)
    [SerializeField] Text characterPreviewSpec;       // キャラ性能(text)
    [SerializeField] Text characterPreviewOther;      // キャラ備考(text)
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
                characterPreviewName.text = "トラスとウェッジ";
                if(GoToChooseChara.playMode == 1) {
                    characterPreviewSkill.text = "爆発物を前に投げて邪魔する";
                }
                else {
                    characterPreviewSkill.text = "爆発物を後ろに設置して邪魔する";
                }
                characterPreviewSpec.text = "移動速度: 1.5 (ダッシュ時2.0)\n体力: 15.0\n回復速度: 1.0\n";
                characterPreviewOther.text = "逃げ切ることも邪魔もできる";
                break;
            case 1:
                characterPreviewObject[1].SetActive(true);
                characterPreviewName.text = "リルモワ";
                characterPreviewSkill.text = "高いところへ飛べる他、足音が聞こえない";
                characterPreviewSpec.text = "移動速度: 1.0 (ダッシュ時3.0)\n体力: 10.0\n回復速度: 2.0\n";
                characterPreviewOther.text = "忍び寄ることに特化している分、体力がない";
                break;
            case 2:
                characterPreviewObject[2].SetActive(true);
                characterPreviewName.text = "水鏡こよみ";
                characterPreviewSkill.text = "小さくなりステルス性を高める";
                characterPreviewSpec.text = "移動速定: 1.0 (ダッシュ時1.5)\n体力: 5.0\n回復速度: 2.0\n";
                characterPreviewOther.text = "隠れている際は体力を使わないが、体力がない";
                break;
            case 3:
                characterPreviewObject[3].SetActive(true);
                characterPreviewName.text = "NoranekoSeven";
                if(GoToChooseChara.playMode == 1) {
                    characterPreviewSkill.text = "物に化けて隠れられる";
                }
                else {
                    characterPreviewSkill.text = "壁を越えられる";
                }
                characterPreviewSpec.text = "移動速度: 1.5 (ダッシュ時2.0)\n体力: 10.0\n回復速度: 1.5\n";
                characterPreviewOther.text = "物に化けたり走ると体力を消耗";
                break;
            case 4:
                characterPreviewObject[4].SetActive(true);
                characterPreviewName.text = "シャーロ";
                if(GoToChooseChara.playMode == 1) {
                    characterPreviewSkill.text = "3回だけ逃げの位置を表示";
                }
                else {
                    characterPreviewSkill.text = "3回だけ鬼の位置を表示";
                }
                characterPreviewSpec.text = "移動速度: 1.0 (ダッシュ時1.5)\n体力: 20.0\n回復速度: 1.0\n";
                characterPreviewOther.text = "逃げで使うと味方も鬼の場所が見れる";
                break;
            case 5:
                characterPreviewObject[5].SetActive(true);
                characterPreviewName.text = "ミュリシア";
                if(GoToChooseChara.playMode == 1) {
                    characterPreviewSkill.text = "近くに逃げがいると反応する";
                }
                else {
                    characterPreviewSkill.text = "一度だけ捕まっても回避できる";
                }
                characterPreviewSpec.text = "移動速定: 1.5 (ダッシュ時2.5)\n体力: 15.0/20.0\n回復速度: 1.5/2.0\n";
                characterPreviewOther.text = "ポテトを食べると性能が上がる";
                break;
            case 6:
                characterPreviewObject[6].SetActive(true);
                characterPreviewName.text = "ウェンルイ";
                if(GoToChooseChara.playMode == 1) {
                    characterPreviewSkill.text = "御札を展開し、触れた逃げの動きを一定時間鈍らせる";
                }
                else {
                    characterPreviewSkill.text = "御札を使って範囲に入ってきた鬼の動きを一定時間封じる";
                }
                characterPreviewSpec.text = "移動速定: 1.5 (ダッシュ時2.0)\n体力: 10.0\n回復速度: 1.0\n";
                characterPreviewOther.text = "御札は3回まで。範囲はあまり大きくない";
                break;
            case 7:
                characterPreviewObject[7].SetActive(true);
                characterPreviewName.text = "ミーシェ";
                characterPreviewSkill.text = "アイテムを2つホールドできる";
                characterPreviewSpec.text = "移動速定: 1.0 (ダッシュ時1.5)\n体力: 10.0\n回復速度: 1.0\n";
                characterPreviewOther.text = "オールマイティ";
                break;
            case 8:
                characterPreviewObject[8].SetActive(true);
                characterPreviewName.text = "朝霞やのは";
                characterPreviewSkill.text = "アイテムの効果が増幅される";
                characterPreviewSpec.text = "移動速定: 1.0 (ダッシュ時1.5)\n体力: 10.0\n回復速度: 1.0\n";
                characterPreviewOther.text = "組み合わせによっては強い";
                break;
            case 9:
                characterPreviewObject[9].SetActive(true);
                characterPreviewName.text = "ナユ";
                if(GoToChooseChara.playMode == 1) {
                    characterPreviewSkill.text = "自分の体力回復が早くなる";
                }
                else {
                    characterPreviewSkill.text = "自分と味方の体力回復が早くなる";
                }
                characterPreviewSpec.text = "移動速度: 1.5 (ダッシュ時2.0)\n体力: 5.0\n回復速度: 2.5\n";
                characterPreviewOther.text = "体力回復に長けているので補助に最適";
                break;
            default:
                characterPreviewName.text = "";
                characterPreviewSkill.text = "";
                characterPreviewSpec.text = "";
                characterPreviewOther.text = "";
                break;
        }
    }
}
