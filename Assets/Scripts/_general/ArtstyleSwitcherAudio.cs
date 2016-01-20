using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ArtstyleSwitcherAudio : MonoBehaviour {

	private AudioSource[] m_audioSources;
	private float[] m_initialVolumes;

	// Use this for initialization
	void Awake () {
		m_audioSources = GetComponents<AudioSource>();
		for (int i = 0; i < m_audioSources.Length; i++){
			m_initialVolumes[i] = m_audioSources[i].volume;
		}
	}

	void Start(){
		ArtstyleManager.OnSwitch += OnArtstyleChange;

		OnArtstyleChange();
	}

	void OnDestroy(){
		ArtstyleManager.OnSwitch -= OnArtstyleChange;
	}

	void OnArtstyleChange(){
		/*foreach(SpriteRenderer spriteRenderer in m_spriteRenderer){
			spriteRenderer.enabled = !spriteRenderer.enabled;
		}*/

		if (ArtstyleManager.s_artstyleManager.GetCurrentStyle() == ArtstyleManager.Style.arcade){
			// arcade
			m_audioSources[0].volume = m_initialVolumes[0];
			m_audioSources[1].volume = 0.0f;
		} else {
			// realistic
			m_audioSources[0].volume = 0.0f;
			m_audioSources[1].volume = m_initialVolumes[0];
		}
	}
}
