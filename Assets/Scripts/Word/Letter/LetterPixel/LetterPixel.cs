using UnityEngine;
using System.Collections;

/*
 * This class implements the behaviour of the pixels that make up a letter
 */

[RequireComponent(typeof(SpriteRenderer))]
public class LetterPixel : MonoBehaviour {

	private int maxHp = 5;
	private int initialHp;
	private int currentHp;

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
		
	}

	// Call this when the pixel is hit by any damage source
	void OnHit(int _damage){
		
	}

	// TODO: implement
	void OnTriggerEnter2D(Collider2D _other){
		Debug.Log("TriggerHit");

		// call OnHit(...) when hit
		if (_other.CompareTag("WordDestroyer")){
			Destroy(this.gameObject);
		}
	}
}
