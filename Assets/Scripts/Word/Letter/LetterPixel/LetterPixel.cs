using UnityEngine;
using System.Collections;

/*
 * This class implements the behaviour of the pixels that make up a letter
 */

[RequireComponent(typeof(ArtstyleSwitcherSprites))]
public class LetterPixel : MonoBehaviour {

	public int maxHp = 1;
	public int m_points = 5;
	private int initialHp;
	private int currentHp;

	private Letter parent;

	// References
	private ArtstyleSwitcherSprites artstyleSwitcher;
	//private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Awake() {
		//spriteRenderer = GetComponent<SpriteRenderer>();
		artstyleSwitcher = GetComponent<ArtstyleSwitcherSprites>();
	}

	// Use this to initialize externally (usually from script that instantiates this object)
	public void Init(float _accuracy, Color _color){
		//initialHp = (int)Mathf.Round((float)maxHp * _accuracy);
		initialHp = maxHp;

		currentHp = initialHp;

		artstyleSwitcher.SetColor(ArtstyleManager.Style.arcade, Color.Lerp(Color.black, _color, _accuracy));
		artstyleSwitcher.SetColor(ArtstyleManager.Style.realistic, Color.Lerp(Color.black, _color, _accuracy));
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
