using UnityEngine;
using System.Collections;

public class Detonation : MonoBehaviour {

	public float m_timeUntilDestroyThis = 2.0f;

	// Use this for initialization
	void Start () {
		Destroy(this.gameObject, m_timeUntilDestroyThis);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
