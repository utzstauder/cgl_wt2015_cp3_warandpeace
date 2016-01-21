using UnityEngine;
using System.Collections;

/*
 * This class implements the behaviour of the pixels that make up a letter
 */

public class LetterPixel : MonoBehaviour {

	public int maxHp = 1;
	public int m_points = 5;
	private int initialHp;
	private int currentHp;

	public SpriteRenderer m_spriteRendererArcade;
	public SpriteRenderer m_spriteRendererRealisticFaceless;
	public SpriteRenderer m_spriteRendererRealisticFacemore;

	private Letter parent;

	// Use this for initialization
	void Awake() {
		
	}

	// Use this to initialize externally (usually from script that instantiates this object)
	public void Init(float _accuracy, Color _color){
		//initialHp = (int)Mathf.Round((float)maxHp * _accuracy);
		initialHp = maxHp;

		currentHp = initialHp;

		if (m_spriteRendererArcade) m_spriteRendererArcade.color = Color.Lerp(Color.black, _color, _accuracy);
		if (m_spriteRendererRealisticFaceless) m_spriteRendererRealisticFaceless.color = Color.Lerp(Color.black, _color, _accuracy);
		if (m_spriteRendererRealisticFacemore) m_spriteRendererRealisticFacemore.color = SkincolorManager.s_skincolorManager.GetRandomSkincolor();
	}

	public void SetParent(Letter _parent){
		parent = _parent;
		transform.parent = _parent.transform;
	}

	public void ApplyDamage(int _damage){
		currentHp -= _damage;
		GameManager.s_gameManager.TriggerArtstyleChange(_damage * ArtstyleManager.s_artstyleManager.m_timeFactor);
		CheckHP();
	}

	private void CheckHP(){
		if (currentHp <= 0){
			GameManager.s_gameManager.AddScore(m_points);
			Destroy(this.gameObject);
		}
	}

	void OnTriggerEnter2D(Collider2D _other){
		if (_other.gameObject.CompareTag("Deadly")){
			ApplyDamage(maxHp);
		}
	}

	void OnDestroy(){
		if (parent) parent.CheckChildCount();
	}

}
