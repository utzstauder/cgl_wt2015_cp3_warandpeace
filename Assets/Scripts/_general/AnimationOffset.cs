using UnityEngine;
using System.Collections;

public class AnimationOffset : MonoBehaviour {

	[SerializeField]
	private string m_AnimationLayerName = "";
	[SerializeField]
	private Animator[] m_animators;

	void Awake () {
		foreach (Animator animator in m_animators){
			animator.Play(m_AnimationLayerName, 0, Random.Range(0.0f, 1.0f));
		}
	}
}
