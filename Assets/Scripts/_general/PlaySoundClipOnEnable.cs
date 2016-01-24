using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundClipOnEnable : MonoBehaviour {

	private AudioSource m_audioSource;

	// Use this for initialization
	void Awake () {
		m_audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void OnEnable () {
		m_audioSource.Play(0);
	}
}
