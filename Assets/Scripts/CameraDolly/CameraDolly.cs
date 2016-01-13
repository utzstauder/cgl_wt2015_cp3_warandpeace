using UnityEngine;
using System.Collections;

public class CameraDolly : MonoBehaviour {

	public Vector3 m_speed = new Vector3(10.0f, 0, 0);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (GameManager.s_gameManager.IsPlaying()){
			transform.position += m_speed;
		}
	}
}
