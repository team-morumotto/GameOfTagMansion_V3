using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]

public class PivotColliderController : MonoBehaviour
{
    public static Transform m_start;    // コライダーの始点.
    public static Transform m_end;      // コライダーの終点.
    public static Transform cameraPos;  // 視点リセットの座標.
    public static bool isBack = false;
    public float distance;              // カメラと注視オブジェクト(プレイヤー)の距離.
    private enum HitState {
        myX,
        myZ,
        hitX,
        hitZ,
        none
    }

    HitState hitState = HitState.none;
    private CapsuleCollider col;        // コライダーの情報を取得するための変数.
    private Vector3 lastTargetPosition; // 前フレームの注視オブジェクト(プレイヤー)の位置.
    public float zoomSpeed = 16.0f;    // ズームの速さ.
    private bool isToMove = false;      // コライダーに触れたあとdistanceが9.0以上になったかどうかのフラグ.
    private bool isHit =false;          // コライダーにあたっているか.
    private bool a = false;             // 一回だけ実行するためのフラグ.

    void Start()
    {
        if (!m_start || !m_end)
        {
            Debug.LogError(name + " needs both Start and End.");
        }
        m_start = GameObject.Find("PlayerCamera").gameObject.transform;
        distance = 0.0f; // 距離を初期化.
        col = GetComponent<CapsuleCollider>();//コライダーの情報を取得.
    }

    void Update() {
        /*
            var clampPos = transform.position;
            switch(hitState) {
                case HitState.myX:
                print("aaa");
                    clampPos.x = Mathf.Clamp(clampPos.x, transform.position.x, transform.position.x + 20.0f);
                    transform.position = clampPos;
                break;

                case HitState.hitX:
                print("bbb");
                    clampPos.x = Mathf.Clamp(clampPos.x, transform.position.x - 20.0f, transform.position.x);
                    transform.position = clampPos;
                break;

                case HitState.myZ:
                print("ccc");
                    Mathf.Clamp(transform.position.z, transform.position.z, transform.position.z + 20.0f);
                break;

                case HitState.hitZ:
                print("ddd");
                    Mathf.Clamp(transform.position.z, transform.position.z - 20.0f, transform.position.z);
                break;

                default:
                break;
            }
            */
            // 視点リセット
            if(Input.GetKeyDown(KeyCode.R)) {
                CameraReset();
            }
    }

    void FixedUpdate() {
        // 初期化
        if(m_start && m_end && !a && cameraPos.transform != null) {
            transform.position =  m_end.transform.position + (transform.forward * -5);         // 自分の位置をプレイヤーの後ろに設定.
            distance = Vector3.Distance(transform.position, m_end.transform.position);          // 最初のdistance.
            a = true;                                                                           // 初期化フラグをtrue.
        }

        if(m_start && m_end && a)  {
            // カメラとプレイヤーの距離が10以上の場合.
            if(7 < Vector3.Distance(transform.position, m_end.transform.position )) {
                transform.position = Vector3.MoveTowards(transform.position, m_end.transform.position, 100 * Time.deltaTime);  // カメラを注視オブジェクトに近づける.
            }

            if(1 < distance) {
                transform.LookAt(m_end);                                                                                           // 注視オブジェクトを見る.
            }

            // カメラとプレイヤーの距離が400以上の場合.
            if(400 < Vector3.Distance(transform.position, m_end.transform.position )) {
                CameraReset();
            }

            var x = m_end.transform.position.x - lastTargetPosition.x;
            var y = m_end.transform.position.y - lastTargetPosition.y;
            var z = m_end.transform.position.z - lastTargetPosition.z;
            var pos = new Vector3(x, y, z);                                                                             // プレイヤーの移動量.

            transform.position += pos;                                                                                  // プレイヤーが移動した分自分も移動する.
            lastTargetPosition = m_end.transform.position;                                                              // 移動後のプレイヤーの位置.

            distance = Vector3.Distance(transform.position, m_end.transform.position);                                  // 距離を計算.
        }
    }

    /*
    void OnTriggerStay(Collider other) {
        if(other.gameObject.tag != "Floor" && other.gameObject.tag != "Nige" && other.gameObject.tag != "Oni"
            && other.gameObject.tag !="Untagged" && other.gameObject.tag != "Item") {
            if(Mathf.Max(transform.position.x, other.gameObject.transform.position.x) == transform.position.x) {
                hitState = HitState.myX;
            }else if(Mathf.Max(transform.position.x, other.gameObject.transform.position.x) == other.gameObject.transform.position.x) {
                hitState = HitState.hitX;
            }else if(Mathf.Max(transform.position.z, other.gameObject.transform.position.z) == transform.position.z) {
                hitState = HitState.myZ;
            }else if(Mathf.Max(transform.position.z, other.gameObject.transform.position.z) == other.gameObject.transform.position.z) {
                hitState = HitState.hitZ;
            }
        }
    }
    */

    void OnTriggerExit(Collider other) {
        // 床/プレイヤー/Untaggedオブジェクト/アイテムでない場合.
        if(other.gameObject.tag != "Floor" && other.gameObject.tag != "Nige" && other.gameObject.tag != "Oni"
            && other.gameObject.tag !="Untagged" && other.gameObject.tag != "Item") {
            /* tex = "接触していない";*/
            hitState = HitState.none;
        }
    }

    public void CameraReset() {
        print("CameraReset");
        transform.position = cameraPos.position;
        transform.LookAt(m_end);
    }
}