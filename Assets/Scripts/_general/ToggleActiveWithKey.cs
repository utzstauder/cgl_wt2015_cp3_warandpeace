using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToggleActiveWithKey : MonoBehaviour {

	public KeyCode m_key;
	public Text m_text;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(m_key)){
			m_text.enabled = !m_text.enabled;
		}
	}
}
