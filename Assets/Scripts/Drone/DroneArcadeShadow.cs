using UnityEngine;
using System.Collections;

public class DroneArcadeShadow : MonoBehaviour {

	public Transform m_parent;
	public float m_speed;

	// Use this for initialization
	void Awake () {
		if (!m_parent) m_parent = GameObject.Find("Sprite_arcade").transform;
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log(m_parent.position + transform.position);
		transform.position = Vector3.Lerp(transform.position, m_parent.position, Time.deltaTime * m_speed);

		gameObject.SetActive(m_parent.gameObject.activeSelf);
	}
}
