using UnityEngine;
using System.Collections;

public class DroneProjectile : MonoBehaviour {

	public float speed = 10.0f;
	public GameObject explosionPrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.position += new Vector3 (0, -speed * Time.deltaTime, 0);

		// Destroy when at bottom of screen
		// TODO: fix later, very un-modular
		if (transform.position.y < LevelSettings.settings.bottomOfLevel.position.y) {
			Destroy(this.gameObject);
		}
	}

	void OnEnable(){
		// For later
	}

	void OnDestroy(){
		// Explosion or something else
		GameObject explosion = (GameObject)Instantiate(explosionPrefab, this.transform.position, Quaternion.identity);
		Destroy(explosion, .5f);
	}

	void OnCollisionEnter2D(){
		Destroy(this.gameObject);
	}
}
