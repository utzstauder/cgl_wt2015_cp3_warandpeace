using UnityEngine;
using System.Collections;

/*
 * This class implements the behaviour of the pixels that make up a letter
 */

[RequireComponent(typeof(SpriteRenderer))]
public class LetterPixel : MonoBehaviour {

	public int maxHp = 5;
	public int m_points = 5;
	private int initialHp;
	private int currentHp;

	private Letter parent;

	// References
	private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// Use this to initialize externally (usually from script that instantiates this object)
	public void Init(float _accuracy, Color _color){
		initialHp = (int)Mathf.Round((float)maxHp * _accuracy);
		currentHp = initialHp;

		spriteRenderer.color = Color.Lerp(Color.black, _color, _accuracy);
	}
	
	// Update is called once per frame
	void Update () {
		//if (transform.position.x < deathX) Destroy(this.gameObject);
	}

	public void SetParent(Letter _parent){
		parent = _parent;
		transform.parent = _parent.transform;
	}

	public void ApplyDamage(int _damage){
		currentHp -= _damage;
		CheckHP();
	}

	private void CheckHP(){
		if (currentHp <= 0){
			GameManager.s_gameManager.AddScore(m_points);
			Destroy(this.gameObject);
		}
	}

	void OnDestroy(){
		if (parent) parent.CheckChildCount();
	}

}
