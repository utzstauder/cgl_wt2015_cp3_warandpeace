using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DroneInput))]
public class Drone : MonoBehaviour {

	private DroneInput m_droneInput;

	public float m_speed = 100.0f;
	public Vector2 m_movementBounds = new Vector2(100.0f, 280.0f);
	private Vector3 m_initialPosition;

	public DroneProjectile m_droneProjectilePrefab;
	private Transform m_droneCannon;
	public int m_shotsPerSecond = 2;
	private float m_waitBetweenShots;
	private float m_shotsTimer = 0;
	public float m_projectileDeathX = 270.0f;

	// Use this for initialization
	void Awake () {
		m_droneCannon = transform.FindChild("DroneCannon").transform;

		m_droneInput = GetComponent<DroneInput>();
		m_initialPosition = transform.localPosition;

		m_waitBetweenShots = 1.0f/m_shotsPerSecond;
	}
	
	// Update is called once per frame
	void Update () {
		if (GameManager.s_gameManager.IsPlaying()){
			transform.localPosition += m_droneInput.GetMovementDirection() * m_speed * Time.deltaTime;

			// Constrain to bounds
			if (transform.localPosition.x < (m_initialPosition.x - m_movementBounds.x/2)){
				transform.localPosition = new Vector3((m_initialPosition.x - m_movementBounds.x/2), transform.localPosition.y, transform.localPosition.z);
			} else if (transform.localPosition.x > (m_initialPosition.x + m_movementBounds.x/2)){
				transform.localPosition = new Vector3((m_initialPosition.x + m_movementBounds.x/2), transform.localPosition.y, transform.localPosition.z);
			}

			if (transform.localPosition.y < (m_initialPosition.y - m_movementBounds.y/2)){
				transform.localPosition = new Vector3(transform.localPosition.x, (m_initialPosition.y - m_movementBounds.y/2), transform.localPosition.z);
			} else if (transform.localPosition.y > (m_initialPosition.y + m_movementBounds.y/2)){
				transform.localPosition = new Vector3(transform.localPosition.x, (m_initialPosition.y + m_movementBounds.y/2), transform.localPosition.z);
			}

			if (m_droneInput.GetFirePressed() && (m_shotsTimer >= m_waitBetweenShots)){
				Shoot();
			}

			m_shotsTimer += Time.deltaTime;}
	}

	private void Shoot(){
		DroneProjectile projectile = Instantiate(m_droneProjectilePrefab, m_droneCannon.position, Quaternion.identity) as DroneProjectile;
		projectile.SetParent(transform.parent);
		projectile.SetDeathX(m_projectileDeathX);

		m_shotsTimer = 0;
		m_waitBetweenShots = 1.0f/m_shotsPerSecond;
	}
}
