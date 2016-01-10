using UnityEngine;
using System.Collections;

public class WordSpawner : MonoBehaviour {

	/*
	 * This class handles all word/letter/pixel instantiating
	 */

	[SerializeField]
	private Word m_wordPrefab;
	[SerializeField]
	private Letter m_letterPrefab;
	[SerializeField]
	private LetterPixel m_letterPixelPrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// TODO: complete
	// Spawn a complete word
	public void SpawnWord(int[] _playerIds){
		if (_playerIds.Length <= 0){
			Debug.LogWarning("There are no players");
		} else{

			int numberOfLetters = _playerIds.Length;
			Vector3 letterPosition = Vector3.zero;
			float letterPositionX = 0;
			Word word = Instantiate(m_wordPrefab, transform.position, Quaternion.identity) as Word;

			for (int i = 0; i < numberOfLetters; i++){
				letterPositionX = transform.position.x - (numberOfLetters * word.m_wordSpacing)/2.0f + (i * word.m_wordSpacing);
				letterPosition = new Vector3(letterPositionX, transform.position.y, 0);
				Letter letter = SpawnLetter(_playerIds[i], letterPosition);
				if (letter != null){
					letter.transform.parent = word.transform;
				}
			}
		}

	}

	// TODO: implement
	// Spawns a single letter
	private Letter SpawnLetter(int _playerId, Vector3 _position){
		// Pop drawing from drawing list
		DrawInputPlayer player = PlayerManager.s_playerManager.GetPlayerReference(_playerId);

		int[] drawing = player.PopDrawing();
		float accuracy = player.PopAccuracy();
		int width = (int)player.GetDrawingDimension().x;
		int height = (int)player.GetDrawingDimension().y;
		int gap = player.GetDrawingDivision();
		Color color = player.GetPlayerColor();
		Vector3 position = Vector3.zero;
		float pixelScale = player.letterPixelScale * player.letterScale;

		if (drawing != null){
			Letter letter = Instantiate(m_letterPrefab, _position, Quaternion.identity) as Letter;

			for (int y = 0; y < height; y++){
				for (int x = 0; x < width; x++){
					if (drawing[width*y + x] > 0){
						position = _position + new Vector3((x * gap) - (width / 2 * gap), (height / 2 * gap) - (y * gap), 0) * player.letterScale;
						LetterPixel pixel = spawnLetterPixel(position, pixelScale, color, accuracy);
						pixel.transform.SetParent(letter.transform);
					}
				}
			}

			return letter;
		}
		Debug.LogWarning("There is no drawing");
		return null;
	}

	// TODO: implement
	// Spawns a letterPixel
	private LetterPixel spawnLetterPixel(Vector3 _position, float _scale, Color _color, float _accuracy){
		LetterPixel letterPixel = Instantiate(m_letterPixelPrefab, _position, Quaternion.identity) as LetterPixel;

		letterPixel.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.black, _color, _accuracy);
		//letterPixel.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit *= _scale;

		return letterPixel;
	}

}
