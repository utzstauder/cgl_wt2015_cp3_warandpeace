using UnityEngine;
using System.Collections;

public class ArtstyleManager : MonoBehaviour {

	public enum Style {arcade, realistic};

	public static ArtstyleManager s_artstyleManager;

	private Style m_currentStyle;
	public delegate void SwitchMessage();
	public static event SwitchMessage OnSwitch;

	public float m_timeFactor = 0.05f;

	// Use this for initialization
	void Awake () {
		if (s_artstyleManager != null){
			Debug.LogError("There is more than one ArtstyleManager in the scene");
			Destroy(this);
		} else{
			s_artstyleManager = this;
		}
	}

	public void SetArtstyle(Style _style){
		m_currentStyle = _style;

		OnSwitch();
	}

	public void Switch(){
		if (m_currentStyle == Style.arcade) m_currentStyle = Style.realistic;
		else m_currentStyle = Style.arcade;

		OnSwitch();
	}

	public Style GetCurrentStyle(){
		return m_currentStyle;
	}
}
