using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class SerialInfo
{
	public Vector2 vector2 = Random.onUnitSphere;
	public Vector3 vector3 = Random.onUnitSphere;
	public Vector4 vector4 = Random.onUnitSphere;
	public Bounds bounds = new Bounds(Random.onUnitSphere, Random.onUnitSphere);
	public Color color = new Color(Random.value, Random.value, Random.value);
	public Color32 color32 = new Color32();
	public Rect rect = new Rect(Random.value, Random.value, Random.value, Random.value);
	public Ray ray = new Ray(Random.onUnitSphere, Random.onUnitSphere);
	public Quaternion quaternion = new Quaternion(Random.value, Random.value, Random.value, Random.value);
	public List<float> listFloat = new List<float>(){1,2,3,4,5,6};
	public Dictionary<string, float> dictStr = new Dictionary<string, float>(){ {"hello", 1}, {"world", 2}};
}

public class LitJSONTest : MonoBehaviour {

	private string m_str1 = "";
	private LitJson.JsonWriter m_PrettyWriter;
	private Vector2 m_ScrollPos;
	
	private enum Format{ Pretty, Plain };
	private Format m_Format = Format.Pretty;

	// Use this for initialization
	void Start () {
		m_PrettyWriter = new LitJson.JsonWriter();
		m_PrettyWriter.PrettyPrint = true;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()
	{
		GUILayout.BeginHorizontal();
			GUI.enabled = m_Format != Format.Pretty;			
			if( GUILayout.Button ("PrettyFormat") )
			{
				m_Format = Format.Pretty;
			}
			
			GUI.enabled = m_Format != Format.Plain;					
			if( GUILayout.Button ("Plain Format") )
			{
				m_Format = Format.Plain;
			}
		GUILayout.EndHorizontal();
		
		GUI.enabled = true;
		if( GUILayout.Button("Serialize", GUILayout.Height (100)) )
		{
			_TestSerialInfo();
		}
		
		m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos, GUILayout.Width (Screen.width-200));
		if( m_str1.Length > 0)
		{
			GUILayout.TextArea(m_str1);
		}
		GUILayout.EndScrollView();
	}
	
	void _TestSerialInfo()
	{
		SerialInfo v1 = new SerialInfo();
		v1.vector2 = new Vector2(0.5f, 0.5f);
		
		if (m_Format == Format.Pretty)
		{
			m_PrettyWriter.Reset();
			LitJson.JsonMapper.ToJson(v1, m_PrettyWriter);
			m_str1 = m_PrettyWriter.ToString();
		}
		else
		{
			m_str1 = LitJson.JsonMapper.ToJson(v1); //serialize	object to string		
		}
		
		LitJson.JsonMapper.ToObject<SerialInfo>(m_str1); //de-serialize string back to object			
	}

}
