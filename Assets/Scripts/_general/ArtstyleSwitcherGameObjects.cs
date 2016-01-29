using UnityEngine;
using System.Collections;

public class ArtstyleSwitcherGameObjects : MonoBehaviour {

	public bool m_destroyOnSwitch = false;

	[SerializeField]
	private GameObject[] m_gameObject;

	// Use this for initialization
	void Awake () {
		if (m_gameObject.Length < 2) Debug.LogWarning("There are not enough GameObjects objects attached!");
	}

	void Start(){
		ArtstyleManager.OnSwitch += OnArtstyleChange;

		OnArtstyleChange();
	}

	void OnDestroy(){
		ArtstyleManager.OnSwitch -= OnArtstyleChange;
	}

	void OnArtstyleChange(){
		if (ArtstyleManager.s_artstyleManager.GetCurrentStyle() == ArtstyleManager.Style.arcade){
			// arcade
			if (m_gameObject[0]) m_gameObject[0].SetActive(true);
			if (m_destroyOnSwitch) Destroy(m_gameObject[1].gameObject, 0.1f);
			else m_gameObject[1].SetActive(false);
		} else {
			// realistic
			if (m_destroyOnSwitch) Destroy(m_gameObject[0], 0.1f);
			else m_gameObject[0].SetActive(false);
			if (m_gameObject[1]) m_gameObject[1].SetActive(true);
		}
	}

	public GameObject GetCurrentGameObject(){
		return m_gameObject[(int)ArtstyleManager.s_artstyleManager.GetCurrentStyle()];
	}
}
