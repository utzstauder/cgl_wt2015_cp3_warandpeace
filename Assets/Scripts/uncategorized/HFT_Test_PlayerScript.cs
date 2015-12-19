using UnityEngine;
using System.Collections;

public class HFT_Test_PlayerScript : MonoBehaviour {

	public float speed = 4f;

	#region references
	private HFTInput hftinput;
	private HFTGamepad hftgamepad;

	private Renderer renderer;
	#endregion

	private float dx, dy;

	// Use this for initialization
	void Start () {
		hftinput = GetComponent<HFTInput>();
		hftgamepad = GetComponent<HFTGamepad>();
		renderer = GetComponent<Renderer>();
		renderer.material.color = hftgamepad.Color;
	}
	
	// Update is called once per frame
	void Update () {
		dx = speed * (hftinput.GetAxis("Horizontal") + Input.GetAxis("Horizontal")) * Time.deltaTime;
		dy = speed * (-hftinput.GetAxis("Vertical") + Input.GetAxis("Vertical")) * Time.deltaTime;

		transform.position = transform.position + new Vector3(dx, dy, 0);
	}
}
