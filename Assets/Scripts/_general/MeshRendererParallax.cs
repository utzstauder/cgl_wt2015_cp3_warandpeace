using UnityEngine;
using System.Collections;

public class MeshRendererParallax : MonoBehaviour {

	public float speed = 0.5f;
	public float start = 0;

	Vector2 offset = new Vector2 (0, 0);

	MeshRenderer renderer;

	void Awake(){
		renderer = GetComponent<MeshRenderer>();
		offset.x = start;
	}

	void Update ()
	{
		if (GameManager.s_gameManager.IsPlaying()){
			offset.x += Time.deltaTime * speed;

			renderer.material.mainTextureOffset = offset;
		}
	}
}
