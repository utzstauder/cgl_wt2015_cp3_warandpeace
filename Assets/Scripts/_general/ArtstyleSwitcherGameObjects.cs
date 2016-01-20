using UnityEngine;
using System.Collections;

public class ArtstyleSwitcherGameObjects : MonoBehaviour {

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
			m_gameObject[0].SetActive(true);
			m_gameObject[1].SetActive(false);
		} else {
			// realistic
			m_gameObject[0].SetActive(false);
			m_gameObject[1].SetActive(true);
		}
	}

	public GameObject GetCurrentGameObject(){
		return m_gameObject[(int)ArtstyleManager.s_artstyleManager.GetCurrentStyle()];
	}
}
