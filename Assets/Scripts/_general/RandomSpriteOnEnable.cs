using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class RandomSpriteOnEnable : MonoBehaviour {

	public Sprite[] m_sprites;

	private SpriteRenderer m_spriteRenderer;

	void Awake(){
		m_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void OnEnable(){
		m_spriteRenderer.sprite = m_sprites[Random.Range(0, m_sprites.Length)];
	}
}
