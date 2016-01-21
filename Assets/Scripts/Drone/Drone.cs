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
	private Vector3 m_targetPosition;
	public Vector2 m_movementBounds = new Vector2(100.0f, 280.0f);
	public float m_movementBoundsPadding = 10.0f;
	private Vector3 m_initialPosition;

	private Vector3 m_crosshairInitialScale;
	private float m_currentCrosshairScale;
	public float m_crosshairSpreadSpeed = 1.0f;
	public float m_crosshairSpreadMax = 3.0f;
	public float m_crosshairCooldownFactor = 2.0f;
	public Vector2 m_crosshairKillzoneArea = new Vector2(50.0f, 50.0f);
	private Vector3 m_droneSpreadOffset = Vector3.zero;

	public DroneProjectile m_droneProjectilePrefab;
	public GameObject m_droneProjectile;
	private LineRenderer m_droneProjectileLineRenderer;
	public GameObject m_droneRocketPrefab;

	public bool m_canFireRocket = false;

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
		m_crosshairInitialScale = m_droneCrosshair.localScale;
		if (!m_droneCrosshairKillzone && m_droneCrosshair) m_droneCrosshairKillzone = m_droneCrosshair.transform.FindChild("DroneCrosshairKillzone").GetComponent<BoxCollider2D>();
		if (!m_droneProjectile) m_droneProjectile = GameObject.Find("DroneProjectile");
		m_droneProjectileLineRenderer = m_droneProjectile.GetComponentInChildren<LineRenderer>();
		m_droneProjectile.SetActive(false);

		m_droneInput = GetComponent<DroneInput>();
		m_initialPosition = m_droneCrosshair.localPosition;

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

			// Limit crosshair to screen size
			if (m_droneCrosshair.localPosition.x < ((transform.parent.position.x - m_movementBounds.x/2) + m_movementBoundsPadding)){
				m_droneCrosshair.localPosition = new Vector3((transform.parent.position.x - m_movementBounds.x/2) + m_movementBoundsPadding, m_droneCrosshair.localPosition.y, m_droneCrosshair.localPosition.z);
			} else if (m_droneCrosshair.localPosition.x > (transform.parent.position.x + m_movementBounds.x/2) - m_movementBoundsPadding){
				m_droneCrosshair.localPosition = new Vector3((transform.parent.position.x + m_movementBounds.x/2) - m_movementBoundsPadding, m_droneCrosshair.localPosition.y, m_droneCrosshair.localPosition.z);
			}

			if (m_droneCrosshair.localPosition.y < ((transform.parent.position.y - m_movementBounds.y/2) + m_movementBoundsPadding)){
				m_droneCrosshair.localPosition = new Vector3(m_droneCrosshair.localPosition.x, (transform.parent.position.y - m_movementBounds.y/2) + m_movementBoundsPadding, m_droneCrosshair.localPosition.z);
			} else if (m_droneCrosshair.localPosition.y > (transform.parent.position.y + m_movementBounds.y/2) - m_movementBoundsPadding){
				m_droneCrosshair.localPosition = new Vector3(m_droneCrosshair.localPosition.x, (transform.parent.position.y + m_movementBounds.y/2) - m_movementBoundsPadding, m_droneCrosshair.localPosition.z);
			}

			m_currentCrosshairDistance = Vector3.Distance(transform.position, m_droneCrosshair.position);

			// Move drone into position
			if (m_currentCrosshairDistance >= m_maxCrosshairDistance){
				m_targetPosition = m_droneCrosshair.position - ((m_droneCrosshair.position - transform.position)
					* (1.0f - ((m_currentCrosshairDistance - m_maxCrosshairDistance) / m_currentCrosshairDistance)));
				transform.localPosition = Vector3.Lerp(transform.localPosition, m_targetPosition, m_droneSpeed * Time.deltaTime);
			}

			// Shoot
			if (m_droneInput.GetFirePressed()){
				Shoot();
				if (m_currentCrosshairScale < m_crosshairSpreadMax){
					m_currentCrosshairScale += m_crosshairSpreadSpeed * Time.deltaTime;
				}
			} else {
				if (m_currentCrosshairScale > 1.0f){
					m_currentCrosshairScale -= m_crosshairSpreadSpeed * Time.deltaTime * m_crosshairCooldownFactor;
				} else m_currentCrosshairScale = 1.0f;
			}

			// compensate for spread
			//m_droneCrosshair.localScale = Vector3.Lerp(m_crosshairInitialScale, m_crosshairInitialScale * m_currentCrosshairScale, m_crosshairSpreadSpeed * Time.deltaTime);
			m_droneCrosshair.localScale = m_crosshairInitialScale * m_currentCrosshairScale;

			// set killzone position
			m_droneCrosshairKillzone.transform.position = m_droneCrosshair.position + m_droneSpreadOffset;

			// set drone projectile position
			m_droneProjectile.transform.position = m_droneCrosshairKillzone.transform.position;

			m_droneProjectileLineRenderer.SetPosition(0, m_droneCannon.position);
			m_droneProjectileLineRenderer.SetPosition(1, m_droneCrosshairKillzone.transform.position);

			if (m_droneInput.GetFireAltPressed()){
				ShootRocket();
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

	private void ShootRocket(){
		GameObject rocket = (GameObject) Instantiate(m_droneRocketPrefab, m_droneCannon.position, Quaternion.identity);
		rocket.GetComponent<DroneRocket>().Init(m_droneCrosshairKillzone.transform.position);
	}

	private IEnumerator FireShot(){
		m_shooting = true;

		m_droneProjectile.SetActive(true);
		m_droneCrosshairKillzone.enabled = true;

		m_shotsTimer = 0;

		// calculate killzone spread
		m_droneSpreadOffset = new Vector3(	Random.Range(0, m_crosshairKillzoneArea.x) - m_crosshairKillzoneArea.x/2,
											Random.Range(0, m_crosshairKillzoneArea.y) - m_crosshairKillzoneArea.y/2,
											0) *
											((1.0f - m_currentCrosshairScale) / m_crosshairSpreadMax);
		//Debug.Log(m_droneSpreadOffset);

		while (m_shotsTimer < m_damageTime){
			m_shotsTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		m_droneProjectile.SetActive(false);
		m_droneCrosshairKillzone.enabled = false;

		m_shotsTimer = 0;
		m_waitBetweenShots = 1.0f/m_shotsPerSecond;

		while (m_shotsTimer < m_waitBetweenShots){
			m_shotsTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		m_droneSpreadOffset = Vector3.zero;

		m_shooting = false;
	}

	void OnDrawGizmosSelected(){
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, m_targetPosition);
	}
}
