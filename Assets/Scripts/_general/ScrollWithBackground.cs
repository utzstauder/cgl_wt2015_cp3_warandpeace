using UnityEngine;
using System.Collections;

public class ScrollWithBackground : MonoBehaviour {

	private Vector3 m_directionalSpeed;

	private float m_multiplier;

	void Awake(){
		m_directionalSpeed = new Vector3(-10.0f, 0, 0);
	}

	void Start(){
		m_multiplier = ScrollingManager.s_scrollingManager.GetMultiplier();
	}

	// Update is called once per frame
	void Update () {
		if (GameManager.s_gameManager.IsPlaying()) transform.position += 	m_directionalSpeed *
																			Time.deltaTime *
																			ScrollingManager.s_scrollingManager.GetSpeed() *
																			m_multiplier;
	}
}
