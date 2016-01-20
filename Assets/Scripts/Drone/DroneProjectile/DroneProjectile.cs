using UnityEngine;
using System.Collections;

public class DroneProjectile : MonoBehaviour {

	private float m_speed = 300.0f;
	private Vector3 m_direction = Vector3.zero;
	private Vector3 m_destination = Vector3.zero;
	private Vector3 m_initialPosition = Vector3.zero;
	public int m_damage = 1;

	private float deathX = 0;

	// Use this for initialization
	void Awake () {
		m_initialPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (GameManager.s_gameManager.IsPlaying()){
			transform.localPosition += m_speed * m_direction * Time.deltaTime;

			if (Vector3.Distance(transform.position, m_initialPosition) >= Vector3.Distance(m_initialPosition, m_destination)){
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

	public void SetDirection(Vector3 _direction){
		m_direction = _direction;
	}

	public void SetDestination(Vector3 _destination){
		m_destination = _destination;
	}

	public void SetSpeed(float _speed){
		m_speed = _speed;
	}

	void OnTriggerEnter2D(Collider2D _other){
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
