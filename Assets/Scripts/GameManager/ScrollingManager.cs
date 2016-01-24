using UnityEngine;
using System.Collections;

public class ScrollingManager : MonoBehaviour {

	public static ScrollingManager s_scrollingManager;

	[Range(-1.0f, 1.0f)]
	public float m_speed = 0.5f;

	private float m_multiplier;

	[SerializeField]
	private Transform m_background;

	// Use this for initialization
	void Awake () {
		if (s_scrollingManager != null){
			Debug.LogError("There is more than one ScrollingManager in the scene");
			Destroy(this);
		} else{
			s_scrollingManager = this;
		}

		if (!m_background) m_background = GameObject.Find("Background").transform;
	}
	
	// Update is called once per frame
	public float GetSpeed () {
		return m_speed;
	}

	public float GetMultiplier(){
		return m_background.lossyScale.x;
	}
}
