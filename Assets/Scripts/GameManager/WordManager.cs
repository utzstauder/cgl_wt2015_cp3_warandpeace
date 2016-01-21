using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WordManager : MonoBehaviour {

	/*
	 * This class contains all the methods needed to read words from the "dictionary"
	 */

	public static WordManager s_wordManager;

	private List<string>[] m_wordlist;

	// Use this for initialization
	void Awake () {
		if (s_wordManager != null){
			Debug.LogError("There is more than one WordManager in the scene");
			Destroy(this);
		} else{
			s_wordManager = this;
		}

		m_wordlist = ReadWordsFromCSV();

		//Debug.Log(GetRandomWord(Random.Range(2, 8)));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/*
	 * This reads from a CSV file and fills a string-array-list thing
	 * 
	 * wordlist[0] == two letters
	 * wordlist[1] == three letters
	 * ...
	 * wordlist[6] == eight letters
	 */
	private List<string>[] ReadWordsFromCSV(){
		List<string>[] wordlist = new List<string>[7];
		for (int i = 0; i < wordlist.Length; i++){
			wordlist[i] = new List<string>();
		}

		List<Dictionary<string,object>> data = CSVReader.Read ("wordlist");

		for (int row = 0; row < data.Count; row++){
			for (int numberOfLetters = 2; numberOfLetters <= 8; numberOfLetters++){
				string word = (string) data[row][numberOfLetters.ToString()];

				if (word.Length == numberOfLetters){
					//Debug.Log(word + " => " + word.Length);
					wordlist[numberOfLetters-2].Add(word);
					//Debug.Log("added word: " + word);
				}
			}
			//Debug.Log(word.Length);

			/* Debug.Log ("two letter words " + data[i]["2"] + " " +
				"three letter words " + data[i]["3"] + " " +
				"four letter words " + data[i]["4"] + " " +
				"five letter words " + data[i]["5"] + " " +
				"six letter words " + data[i]["6"] + " " +
				"seven letter words " + data[i]["7"] + " " +
				"eight letter words " + data[i]["8"] ); */
		}

		return wordlist;
	}
		
	public int[] GetRandomWordAsIntArray(int _length){
		int[] letters = new int[_length];

		string word = GetRandomWord(_length);
		//string[] letters = new string[word.Length];

		for (int i = 0; i < word.Length; i++){
			letters[i] = AlphabetManager.CharToInt(word[i]);
		}

		return letters;
	}

	public int[] GetWordAsIntArrayFromString(string _word){
		int[] letters = new int[_word.Length];

		for (int i = 0; i < _word.Length; i++){
			letters[i] = AlphabetManager.CharToInt(_word[i]);
		}

		return letters;
	}

	public string GetRandomWord(int _length){
		return m_wordlist[_length - 2][Random.Range(0, m_wordlist[_length - 2].Count)];
	}

	/* Returns an array of random letters of _length
	 * DEPRECATED
	 */
	public int[] GetRandomLetters(int _length){
		int[] letters = new int[_length];

		for (int i = 0; i < letters.Length; i++){
			letters[i] = Random.Range(0, AlphabetManager.g_letters.Length - 1);
		}

		return letters;
	}
}
