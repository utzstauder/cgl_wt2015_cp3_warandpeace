using UnityEngine;
using System.Collections;

public class DroneProjectile : MonoBehaviour {

	public Vector3 m_speed = new Vector3(300.0f, 0, 0);
	public int m_damage = 1;

	private float deathX = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (GameManager.s_gameManager.IsPlaying()){
			transform.localPosition += m_speed * Time.deltaTime;

			if (transform.localPosition.x >= deathX){
				Destroy(this.gameObject);
			}
		}
	}

	public void SetParent(Transform _parent){
		transform.parent = _parent;
	}

	public void SetDeathX(float _x){
		deathX = _x;
	}

	void OnTriggerEnter2D(Collider2D _other){
		// Debug.Log("TriggerEnter2D");
		if (_other.CompareTag("LetterPixel")){
			LetterPixel letterPixel = _other.GetComponent<LetterPixel>();
			letterPixel.ApplyDamage(m_damage);
			Destroy(this.gameObject);
		}
	}

	void OnDestroy(){
		//TODO: play animation or something like that
	}
}
