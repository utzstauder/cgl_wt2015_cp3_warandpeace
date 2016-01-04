using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimes;
using HappyFunTimesExample;
using CSSParse;

public class DronesTouchPlayer : MonoBehaviour
{
	private Renderer m_renderer;
	private HFTGamepad m_gamepad;
	private HFTInput m_hftInput;
	private UnityEngine.UI.Text m_text;
	private UnityEngine.UI.RawImage m_rawImage;
	private Vector3 m_position;
	private Color m_color;
	private string m_name;
	private TouchGameSettings settings;

	private float inputX, inputY = 0;
	private float inputXPrev, inputYPrev = 0;
	private float l, nx, ny = 0;

	// pathfinding and drawing stuff
	private float drawThreshold = 0.01f;
	[SerializeField]
	private List<Vector3> listOfWaypoints;
	public bool drawWaypoints = true;

	// draw idle timer
	private bool idle = false;
	public float timeUntilIdle = 1f;
	private float idleTimer = 0;

    void Start()
    {
        m_gamepad  = GetComponent<HFTGamepad>();
        m_renderer = GetComponent<Renderer>();
        m_position = transform.localPosition;

        m_text = transform.FindChild("NameUI/Name").gameObject.GetComponent<UnityEngine.UI.Text>();
        m_rawImage = transform.FindChild("NameUI/NameBackground").gameObject.GetComponent<UnityEngine.UI.RawImage>();
        m_rawImage.material = (Material)Instantiate(m_rawImage.material);

        m_gamepad.OnNameChange += ChangeName;

		GameManager.current.AddPlayer(this);

        SetName(m_gamepad.Name);
        SetColor(m_gamepad.Color);

		listOfWaypoints = new List<Vector3>();
    }

    void Update()
    {
		settings = TouchGameSettings.settings();

		inputX = m_gamepad.axes[HFTGamepad.AXIS_TOUCH_X];
		inputY = m_gamepad.axes[HFTGamepad.AXIS_TOUCH_Y];

		// Debug.Log(name + " // " + m_gamepad.axes[HFTGamepad.AXIS_TOUCH_X]);

		if (inputX != inputXPrev || inputY != inputYPrev){
			// touch input

			// reset idle timer
			idleTimer = 0;
			idle = false;

			// TODO: change this
			m_renderer.material.color = m_color;

	      	l = Time.deltaTime * 10.0f; //	1.0f;
	        nx = inputX * 0.5f + 0.5f;	//	0 <-> 1
	        ny = inputY * 0.5f + 0.5f;	//	0 <-> 1

			// divide screen into vertical pieces based on number of connected players
			nx = (nx / GameManager.current.GetNumberOfPlayers()) + (GameManager.current.GetPlayerIndex(this) * (1f / GameManager.current.GetNumberOfPlayers())) - 0.5f;
		
		} else {
			// no touch input

			// increment idle timer
			idleTimer += Time.deltaTime;

			// check if idle
			if (idleTimer >= timeUntilIdle){
				idle = true;

				if (listOfWaypoints.Count > 0){
					SendWaypoints();	
				}

				listOfWaypoints.Clear();
			}

			m_renderer.material.color = Color.Lerp(m_renderer.material.color, Color.gray, Time.deltaTime * timeUntilIdle);
		}

		m_position.x = Mathf.Lerp(m_position.x, settings.areaWidth * nx, l);
		m_position.z = Mathf.Lerp(m_position.z, settings.areaHeight - (ny * settings.areaHeight) - 1, l);  // because in 2D down is positive.

		if ( ((Mathf.Abs(inputX) - Mathf.Abs(inputXPrev)) > drawThreshold) || ((Mathf.Abs(inputY) - Mathf.Abs(inputYPrev)) > drawThreshold) ){
			listOfWaypoints.Add(m_position);
		}

		gameObject.transform.localPosition = m_position;

		inputXPrev = inputX;
		inputYPrev = inputY;
    }

    void SetName(string name)
    {
        m_name = name;
        gameObject.name = "Player-" + m_name;
		m_text.text = GameManager.current.GetPlayerIndex(this) + " " + name;
    }

    void SetColor(Color color)
    {
        m_color = color;
        m_renderer.material.color = m_color;
        m_rawImage.material.color = m_color;
    }

	public Color GetColor(){
		return m_color;
	}

    public void OnTriggerEnter(Collider other)
    {
        // Because of physics layers we can only collide with the goal
    }

	public void OnDestroy(){
		GameManager.current.RemovePlayer(this);
	}

	// TODO: not elegant; currently only sets a flag for the gizmos to draw
	private void SendWaypoints(){
		// TODO: send list of waypoints to game manager
		//drawWaypoints = true;
		HumanParticleHandler.current.SetParticlesOnPlayerPath(GameManager.current.GetPlayerIndex(this), listOfWaypoints);
	}

    private void Remove(object sender, EventArgs e)
    {
        Destroy(gameObject);
    }

    private void ChangeName(object sender, EventArgs e)
    {
        SetName(m_gamepad.Name);
    }

	void OnDrawGizmos(){
		Gizmos.color = m_color;

		for (int i = 1; i < listOfWaypoints.Count; i++){
			Gizmos.DrawLine(listOfWaypoints[i-1], listOfWaypoints[i]);
		}
	}
}

