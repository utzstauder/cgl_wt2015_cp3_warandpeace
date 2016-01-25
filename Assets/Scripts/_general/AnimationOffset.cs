using UnityEngine;
using System.Collections;

public class AnimationOffset : MonoBehaviour {

	[SerializeField]
	private string m_AnimationLayerName = "";
	[SerializeField]
	private Animator[] m_animators;

	void Start () {
		float randomFloat = Random.Range(0.0f, 1.0f);

		foreach (Animator animator in m_animators){
			animator.Play(m_AnimationLayerName, 0, randomFloat);
		}
	}

	public void SetAnimationOffset(){
		float randomFloat = Random.Range(0.0f, 1.0f);

		foreach (Animator animator in m_animators){
			animator.Play(m_AnimationLayerName, 0, randomFloat);
		}
	}
}
