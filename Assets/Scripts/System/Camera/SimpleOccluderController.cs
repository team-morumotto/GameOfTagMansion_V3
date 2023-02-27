using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// トリガーに接触したオブジェクトを（半）透明にする機能を提供する。
/// 半透明にしたいオブジェクトに対しては、マテリアルのシェーダーに「Rendering Mode = Transparent の Standard Shader」など、color で alpha を指定できるものをアサインすること。
/// </summary>
[RequireComponent(typeof(Collider))]
public class SimpleOccluderController : MonoBehaviour
{
    /// <summary>（半）透明状態にする時にどれくらいの alpha にするか指定する</summary>
    [SerializeField, Range(0f, 1f)] float m_transparency = 0.8f;
    /// <summary>（半）透明状態から戻る時にどれくらいの alpha にするか指定する</summary>
    [SerializeField, Range(0f, 1f)] float m_opaque = 1f;

    private void OnTriggerEnter(Collider other)
    {
        Renderer r = other.gameObject.GetComponent<Renderer>();
        ChangeAlpha(r, m_transparency);
    }

    private void OnTriggerExit(Collider other)
    {
        Renderer r = other.gameObject.GetComponent<Renderer>();
        ChangeAlpha(r, m_opaque);
    }

    /// <summary>
    /// alpha を変更する
    /// </summary>
    /// <param name="renderer">alpha を変更する Material を持った Renderer</param>
    /// <param name="targetAlpha">alpha を変更したい値</param>
    void ChangeAlpha(Renderer renderer, float targetAlpha)
    {
        if (renderer)
        {
            Material m = renderer.material;
            Color c = m.color;
            c.a = targetAlpha;
            m.color = c;
        }
    }
}
