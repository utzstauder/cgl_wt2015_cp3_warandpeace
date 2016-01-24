using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayAnimationOnEnable : MonoBehaviour {

	[SerializeField]
	private string m_AnimationLayerName = "";

	private Animator m_animator;

	// Use this for initialization
	void Awake () {
		m_animator = GetComponent<Animator>();	
	}
	
	// Update is called once per frame
	void OnEnable () {
		m_animator.Play(m_AnimationLayerName, 0, 0);
	}
}
