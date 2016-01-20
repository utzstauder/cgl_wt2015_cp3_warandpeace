using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class AnimationOffset : MonoBehaviour {

	[SerializeField]
	private string m_AnimationLayerName = "";

	private Animator m_animator;

	void Awake () {
		m_animator = GetComponent<Animator>();
		m_animator.Play(m_AnimationLayerName, 0, Random.Range(0.0f, 1.0f));
	}
}
