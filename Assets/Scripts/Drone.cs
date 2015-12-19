using UnityEngine;
using System.Collections;

public class Drone : MonoBehaviour {

	public float speed = 20.0f;
	public float minX = -12.0f;
	public float maxX = 12.0f;

	public DroneProjectile projectilePrefab;
	public float fireCooldown = 1f;
	private float currentCooldownTime = 0;
	private bool canFire = true;

	private float horizontal = 0;
	private bool firePressed = false;
	private float newX = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// Check input
		horizontal = Input.GetAxis("Horizontal");
		firePressed = Input.GetButtonDown("Jump");

		// Calculate new position and limit to horizontal boundaries
		newX = Mathf.Clamp(this.transform.position.x + horizontal * speed * Time.deltaTime, minX, maxX);

		// Move to new position
		this.transform.position = new Vector3(newX, this.transform.position.y, this.transform.position.z);

		// Check for cooldown time
		if ((currentCooldownTime += Time.deltaTime) > fireCooldown){
			canFire = true;
		}

		// Fire missile if button pressed and cooldown
		if (firePressed && canFire){
			Instantiate(projectilePrefab, this.transform.position, Quaternion.identity);
			canFire = false;
			currentCooldownTime = 0;
		}
	}

	void OnDrawGizmosSelected(){
		Gizmos.color = Color.red;

		Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(minX, 0, 0));
		Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(maxX, 0, 0));
	}
}
