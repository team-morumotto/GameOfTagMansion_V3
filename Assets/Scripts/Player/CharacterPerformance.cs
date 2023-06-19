/*
*   Created by Kobayashi Atsuki.
*   キャラの固有性能まとめ(キャラごとに扱えるものが違う).
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPerformance : MonoBehaviour
{









    /// <summary>
    /// 任意の時間遅延.
    /// </summary>
    /// <param name="time">遅延する時間</param>
    private IEnumerator Delay(float time) {
        yield return new WaitForSeconds(time);
    }
}