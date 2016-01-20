using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DroneInput))]
public class Drone : MonoBehaviour {

	private DroneInput m_droneInput;

	public Transform m_droneCrosshair;
	public BoxCollider2D m_droneCrosshairKillzone;

	public float m_crosshairSpeed = 100.0f;
	public float m_maxCrosshairDistance = 500.0f;
	private float m_currentCrosshairDistance;
	public float m_droneSpeed = 10.0f;
	public Vector2 m_movementBounds = new Vector2(100.0f, 280.0f);
	private Vector3 m_initialPosition;

	public DroneProjectile m_droneProjectilePrefab;
	public LineRenderer m_droneProjectile;

	public float m_droneProjectileSpeed = 300.0f;
	private Transform m_droneCannon;
	public float m_damageTime = .1f;
	public int m_shotsPerSecond = 2;
	private float m_waitBetweenShots;
	private float m_shotsTimer = 0;
	public float m_projectileDeathX = 270.0f;

	private bool m_shooting = false;

	// Use this for initialization
	void Awake () {
		m_droneCannon = transform.FindChild("DroneCannon").transform;
		if (!m_droneCrosshair) m_droneCrosshair = GameObject.Find("DroneCrosshair").transform;
		if (!m_droneCrosshairKillzone && m_droneCrosshair) m_droneCrosshairKillzone = m_droneCrosshair.transform.FindChild("DroneCrosshairKillzone").GetComponent<BoxCollider2D>();
		if (!m_droneProjectile) m_droneProjectile = GameObject.Find("DroneProjectile").GetComponent<LineRenderer>();
		m_droneProjectile.enabled = false;

		m_droneInput = GetComponent<DroneInput>();
		m_initialPosition = transform.localPosition;

		m_waitBetweenShots = 1.0f/m_shotsPerSecond;
	}
	
	// Update is called once per frame
	void Update () {
		if (GameManager.s_gameManager.IsPlaying()){
			/*
			 * OLD DRONE MOVEMENT
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
			*/

			// NEW DRONE/CROSSHAIR MOVEMENT
			m_droneCrosshair.localPosition += m_droneInput.GetMovementDirection() * m_crosshairSpeed * Time.deltaTime;

			m_currentCrosshairDistance = Vector3.Distance(transform.position, m_droneCrosshair.position);

			// Move drone into position
			if (m_currentCrosshairDistance > m_maxCrosshairDistance){
				//transform.localPosition += (m_droneCrosshair.localPosition - transform.localPosition) * m_droneSpeed * Time.deltaTime;
				transform.localPosition = Vector3.Lerp(transform.localPosition, m_droneCrosshair.localPosition, m_droneSpeed * Time.deltaTime);
			}

			m_droneProjectile.SetPosition(0, m_droneCannon.position);
			m_droneProjectile.SetPosition(1, m_droneCrosshair.position);

			if (m_droneInput.GetFirePressed() /* && (m_shotsTimer >= m_waitBetweenShots) */){
				Shoot();
			}
		}
	}

	private void Shoot(){
		if (!m_shooting) StartCoroutine(FireShot());
		/*
		 * OLD CODE
		Vector3 direction = (m_droneCrosshair.position - m_droneCannon.position).normalized;
		float angle = Vector3.Angle(Vector3.right, direction);

		//Debug.Log(direction + " / " + angle);

		Quaternion rotation;
		if (m_droneCrosshair.position.y > m_droneCannon.position.y){
			rotation = Quaternion.Euler(0, 0, angle);
		} else {
			rotation = Quaternion.Euler(0, 0, 360.0f-angle);
		}

		DroneProjectile projectile = Instantiate(m_droneProjectilePrefab, m_droneCannon.position, m_droneCannon.rotation * rotation) as DroneProjectile;
		projectile.SetParent(transform.parent);
		//projectile.SetDeathX(m_projectileDeathX);
		projectile.SetDestination(m_droneCrosshair.position);
		projectile.SetDirection((m_droneCrosshair.position - m_droneCannon.position).normalized);
		projectile.SetSpeed(m_droneProjectileSpeed);

		*/

		/*
		m_droneProjectile.SetPosition(0, m_droneCannon.position);
		m_droneProjectile.SetPosition(1, m_droneCrosshair.position);

		m_droneProjectile.enabled = true;
		m_droneCrosshairKillzone.enabled = true;

		m_shotsTimer = 0;
		m_waitBetweenShots = 1.0f/m_shotsPerSecond;
		*/
	}

	private IEnumerator FireShot(){
		m_shooting = true;

		m_droneProjectile.enabled = true;
		m_droneCrosshairKillzone.enabled = true;

		m_shotsTimer = 0;

		while (m_shotsTimer < m_damageTime){
			m_shotsTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		m_droneProjectile.enabled = false;
		m_droneCrosshairKillzone.enabled = false;

		m_shotsTimer = 0;
		m_waitBetweenShots = 1.0f/m_shotsPerSecond;

		while (m_shotsTimer < m_waitBetweenShots){
			m_shotsTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		m_shooting = false;
	}
}
