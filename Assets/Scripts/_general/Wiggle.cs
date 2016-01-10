using UnityEngine;
using System.Collections;

public class Wiggle : MonoBehaviour {

	public bool wiggle = false;
	public Vector2 wiggleSpan = Vector2.zero;
	public float wiggleSpeed = 1.0f;
	public Vector2 wiggleFrequencyRange = new Vector2(0.01f, 3.0f);

	private float wiggleFrequency = 1.0f;
	private Vector3 newPosition = Vector3.zero;
	private Vector3 initialPosition;
	private float frequencyTimer = 0.0f;

	// Use this for initialization
	void Start () {
		initialPosition = transform.localPosition;
	}

	// Update is called once per frame
	void Update () {
		if (wiggle){
			if (frequencyTimer == 0){
				wiggleFrequency = Mathf.Clamp(Random.Range(wiggleFrequencyRange.x, wiggleFrequencyRange.y), 0.001f, wiggleFrequencyRange.y);
				newPosition = new Vector3(	initialPosition.x + Random.Range(-wiggleSpan.x, wiggleSpan.x),
											initialPosition.y + Random.Range(-wiggleSpan.y, wiggleSpan.y),
											0);
			}
				
			// increment timer
			frequencyTimer += Time.deltaTime;

			// reset timer
			if (frequencyTimer >= 1.0f / wiggleFrequency) frequencyTimer = 0.0f;
		} else {
			// return to initial position
			newPosition = initialPosition;

			// reset timer
			frequencyTimer = 0;
		}
			
		transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, Time.deltaTime * wiggleSpeed);
	}
}
