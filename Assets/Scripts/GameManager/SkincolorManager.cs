using UnityEngine;
using System.Collections;

public class SkincolorManager : MonoBehaviour {

	public static SkincolorManager s_skincolorManager;

	public Color[] m_skincolors;

	void Awake(){
		if (s_skincolorManager != null){
			Debug.LogError("There is more than one SkincolorManager in the scene");
			Destroy(this);
		} else{
			s_skincolorManager = this;
		}
	}

	public Color GetRandomSkincolor(){
		if (m_skincolors.Length <= 0) return Color.white;

		return m_skincolors[Random.Range(0, m_skincolors.Length)];
	}
}
