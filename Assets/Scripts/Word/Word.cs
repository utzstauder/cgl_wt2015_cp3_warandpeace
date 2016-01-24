using UnityEngine;
using System.Collections;

public class Word : MonoBehaviour {

	//public Vector3 m_speed = new Vector3(-10.0f, 0, 0);

	public float m_wordSpacing = 20.0f;

	private Vector3 m_targetPosition = Vector3.zero;

	private WordSpawner m_wordSpawner;

	// Use this for initialization
	void Start () {
		InvokeRepeating("CheckChildCount", 0, 5f);
	}
	
	// Update is called once per frame
	void Update () {
		//m_targetPosition = transform.position + m_speed;
		//transform.position = Vector3.Lerp(transform.position, m_targetPosition, m_speed.magnitude * Time.deltaTime);
		/*if (GameManager.s_gameManager.IsPlaying()){
			transform.position += m_speed * Time.deltaTime;
		}*/

		//if (transform.position.x <= m_wordSpawner.GetDeathX()) Destroy(this.gameObject);
	}

	public void CheckChildCount(){
		if (transform.childCount <= 0) Destroy(this.gameObject);
	}

	public void SetWordSpawnerReference(WordSpawner _reference){
		m_wordSpawner = _reference;
	}
		
}
