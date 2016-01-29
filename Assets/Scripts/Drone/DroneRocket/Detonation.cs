using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class Detonation : MonoBehaviour {

	public float m_timeUntilDestroyThis = 5.0f;

	public float m_killtime = .1f;

	private CircleCollider2D m_collider;

	void Awake(){
		m_collider = GetComponent<CircleCollider2D>();
	}

	// Use this for initialization
	void Start () {
		Invoke("DisableCollider", m_killtime);
		Destroy(this.gameObject, m_timeUntilDestroyThis);
	}
	
	private void DisableCollider(){
		m_collider.enabled = false;
	}
}
