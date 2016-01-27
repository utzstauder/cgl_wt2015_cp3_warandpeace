using UnityEngine;
using System.Collections;

public class WordDestroyer : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D _other){
		Debug.Log("TriggerEnter2D");
		if (_other.gameObject.CompareTag("LetterPixel")){
			if (GameManager.s_gameManager.m_wordSpawner) _other.transform.parent = GameManager.s_gameManager.m_wordSpawner.transform;
			_other.gameObject.SetActive(false);
		}
	}
}
