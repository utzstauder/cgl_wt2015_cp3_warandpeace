using UnityEngine;
using System.Collections;

public class DroneRocket : MonoBehaviour {

	public float m_speed = 1.0f;
	public float m_acceleration = 1.0f;
	public float m_detonationDistance = 5.0f;
	public GameObject m_detonationPrefab;

	private Vector3 m_target;

	private Vector3 m_direction;
	private float m_angle;
	private Quaternion m_rotation;
	private float m_distance;

	// Use this for initialization
	void Start () {
	
	}

	public void Init(Vector3 _target){
		m_target = _target;

		// Rotation
		m_direction = (transform.position - m_target).normalized;
		m_angle = Vector3.Angle(Vector3.right, m_direction);

		if (transform.position.y > m_target.y){
			m_rotation = Quaternion.Euler(0, 0, m_angle);
		} else {
			m_rotation = Quaternion.Euler(0, 0, 360.0f-m_angle);
		}

		//transform.localRotation = Quaternion.Lerp(transform.localRotation, m_rotation, m_speed * Time.deltaTime);
		transform.localRotation = m_rotation;
	}
	
	// Update is called once per frame
	void Update () {
		// Rotation
		m_direction = (transform.position - m_target).normalized;
		m_angle = Vector3.Angle(Vector3.right, m_direction);

		if (transform.position.y > m_target.y){
			m_rotation = Quaternion.Euler(0, 0, m_angle);
		} else {
			m_rotation = Quaternion.Euler(0, 0, 360.0f-m_angle);
		}

		//transform.localRotation = Quaternion.Lerp(transform.localRotation, m_rotation, m_speed * Time.deltaTime);
		transform.localRotation = m_rotation;

		// Move
		//transform.localPosition = Vector3.Lerp(transform.localPosition, m_target, Time.deltaTime * m_speed);
		transform.localPosition -= m_direction * Time.deltaTime * m_speed;

		// Accelerate
		m_speed += m_acceleration * Time.deltaTime;

		if (m_target != null){
			m_distance = Vector3.Distance(transform.localPosition, m_target);

			if (m_distance <= m_detonationDistance){
				Detonate();
			}
		}
	}

	private void Detonate(){
		if (m_detonationPrefab) Instantiate(m_detonationPrefab, m_target, Quaternion.identity);
		Destroy(this.gameObject);
	}
}
