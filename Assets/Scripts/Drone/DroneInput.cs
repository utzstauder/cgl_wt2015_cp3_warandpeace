using UnityEngine;
using System.Collections;

public class DroneInput : MonoBehaviour {

	private float m_input_x;
	private float m_input_y;
	private bool m_input_fire;
	private bool m_input_fireAlt;
	private bool m_idle = true;

	public bool m_allowAutoFire = true;
	public bool m_autoPilot = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		m_input_x = Input.GetAxis("Horizontal");
		m_input_y = Input.GetAxis("Vertical");

		if (m_allowAutoFire){
			m_input_fire = Input.GetButton("Fire1");
		} else{
			m_input_fire = Input.GetButtonDown("Fire1");
		}

		m_input_fireAlt = Input.GetButtonDown("Fire2");

		if (m_input_x == 0 &&
			m_input_y == 0 &&
			m_input_fire == false &&
			m_input_fireAlt == false){
			m_idle = true;
		} else m_idle = false;
	}

	public Vector3 GetMovementDirection(){
		return new Vector3(m_input_x, m_input_y, 0);
	}

	public bool GetFirePressed(){
		return m_input_fire;
	}

	public bool GetFireAltPressed(){
		return m_input_fireAlt;
	}

	public bool IsInAutoPilot(){
		return m_autoPilot;
	}

	public bool IsIdle(){
		return m_idle;
	}
}
