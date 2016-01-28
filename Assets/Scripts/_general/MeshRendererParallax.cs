using UnityEngine;
using System.Collections;

public class MeshRendererParallax : MonoBehaviour {

	public float start = 0;

	public float m_speedMultiplier = 1.0f;

	Vector2 offset = new Vector2 (0, 0);

	MeshRenderer renderer;

	void Awake(){
		renderer = GetComponent<MeshRenderer>();
		offset.x = start;
	}

	void Update ()
	{
		if (GameManager.s_gameManager.IsPlaying()){
			offset.x += Time.deltaTime * ScrollingManager.s_scrollingManager.GetSpeed() * m_speedMultiplier;

			renderer.material.mainTextureOffset = offset;
		}
	}
}
