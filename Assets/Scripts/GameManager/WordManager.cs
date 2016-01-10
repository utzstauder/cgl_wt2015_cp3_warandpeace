using UnityEngine;
using System.Collections;

public class WordManager : MonoBehaviour {

	/*
	 * This class contains all the methods needed to read words from the "dictionary"
	 */

	// Use this for initialization
	void Start () {
		
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
