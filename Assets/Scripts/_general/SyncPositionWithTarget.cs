using UnityEngine;
using System.Collections;

public class SyncPositionWithTarget : MonoBehaviour {

	public Transform m_target;

	// Use this for initialization
	void Awake () {
		if (!m_target) Destroy(this);	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = m_target.position;
	}
}
