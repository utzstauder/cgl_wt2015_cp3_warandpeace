using UnityEngine;
using System.Collections;

public class ArtstyleSwitcherSprites : MonoBehaviour {

	[SerializeField]
	private SpriteRenderer[] m_spriteRenderer;

	// Use this for initialization
	void Awake () {
		if (m_spriteRenderer.Length < 2) Debug.LogWarning("There are not enough SpriteRenderer objects attached!");
	}

	void Start(){
		ArtstyleManager.OnSwitch += OnArtstyleChange;

		OnArtstyleChange();
	}

	void OnDestroy(){
		ArtstyleManager.OnSwitch -= OnArtstyleChange;
	}

	void OnArtstyleChange(){
		/*foreach(SpriteRenderer spriteRenderer in m_spriteRenderer){
			spriteRenderer.enabled = !spriteRenderer.enabled;
		}*/

		if (ArtstyleManager.s_artstyleManager.GetCurrentStyle() == ArtstyleManager.Style.arcade){
			// arcade
			m_spriteRenderer[0].enabled = true;
			m_spriteRenderer[1].enabled = false;
		} else {
			// realistic
			m_spriteRenderer[0].enabled = false;
			m_spriteRenderer[1].enabled = true;
		}
	}

	public void SetColor(ArtstyleManager.Style _style, Color _color){
		m_spriteRenderer[(int)_style].color = _color;
	}

	public SpriteRenderer GetCurrentSpriteRenderer(){
		return m_spriteRenderer[(int)ArtstyleManager.s_artstyleManager.GetCurrentStyle()];
	}
}
