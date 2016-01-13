using UnityEngine;
using System.Collections;

public class WordManager : MonoBehaviour {

	/*
	 * This class contains all the methods needed to read words from the "dictionary"
	 */

	public static WordManager s_wordManager;

	// Use this for initialization
	void Awake () {
		if (s_wordManager != null){
			Debug.LogError("There is more than one WordManager in the scene");
			Destroy(this);
		} else{
			s_wordManager = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// TODO: implement
	public int[] GetWord(int _length){
		return new int[_length];
	}

	// Returns an array of random letters of _length
	public int[] GetRandomLetters(int _length){
		int[] letters = new int[_length];

		for (int i = 0; i < letters.Length; i++){
			letters[i] = Random.Range(0, AlphabetManager.g_letters.Length - 1);
		}

		return letters;
	}
}
