using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ChaserMulicia : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    public Sprite chaserAvilityImage;
    void Start() {
        if(photonView.IsMine) {
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(MuliciaCS), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        EffectDatabase = GameObject.Find("EffectList").GetComponent<EffectDatabase>();
        GetStatus(); // ステータスの取得.
        if(photonView.IsMine) {
            if(GoToChooseChara.GetPlayMode() == 1) {
                avilityImage.sprite = chaserAvilityImage;
            }
        }
    }

    void Update () {
        print(chaserAvilityImage.name);
        if(!photonView.IsMine) {
            return;
        }

        // 固有能力が使用可能か.
        if(isCanUseAbility) {
            GetPlayersPos();
        }
        BaseUpdate();
    }

    [PunRPC]
    private void MuliciaCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//

    private float detectionRange = 50.0f; // 探知範囲.
    /// <summary>
    /// 自分とほかキャラとの相対位置を計算し、一定範囲内なら反応する.
    /// ※ミュリシア(鬼)の固有性能.
    /// </summary>
    public void GetPlayersPos() {
        foreach(var players in escapeList) {
            var tmpDistance = (players.transform.position - transform.position).magnitude; // 自分とほかキャラの相対位置を計算.
            // 探知範囲内なら.
            if(tmpDistance < detectionRange) {
                StartCoroutine(TimeEffectLoop(EffectDatabase.avilityEffects[4], 1.0f));
            }
        }
    }
}
