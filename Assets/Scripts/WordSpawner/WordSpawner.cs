using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	public int m_arrayDivisionFactor = 2;
	public Vector2 m_wordScaleRange = new Vector2(0.5f, 1.0f);

	private Transform wordDestroyer;
	private float deathX = 0;

	// the drawing queue
	private List<Drawing> m_drawingQueue;

	void Awake(){
		m_drawingQueue = new List<Drawing>();
	}

	// Use this for initialization
	void Start () {
		wordDestroyer = GameObject.FindGameObjectWithTag("WordDestroyer").transform;
	}
	
	// Update is called once per frame
	void Update () {
		deathX = wordDestroyer.position.x;
	}

	public float GetDeathX(){
		return deathX;
	}

	public void AddLetterDrawingToQueue(Drawing _drawing){
		//Debug.Log("AddLetterDrawingToQueue");

		m_drawingQueue.Add(_drawing);

		//Debug.Log("Queue contains " + m_drawingQueue.Count + " drawings");
	}

	public bool IsQueueFull(){
		return (PlayerManager.s_playerManager.GetNumberOfPlayersInCurrentRound() == m_drawingQueue.Count);
	}

	public void SpawnLetterFromDrawing(Drawing _drawing){
		Word word = Instantiate(m_wordPrefab, transform.position, Quaternion.identity) as Word;
		word.SetWordSpawnerReference(this);
		Letter letter = SpawnLetter(_drawing, transform.position);
		if (letter != null){
			letter.transform.parent = word.transform;
		}
	}

	public void SpawnLettersFromQueue(){

		//Debug.Log("SpawnLettersFromQueue");

		if (m_drawingQueue.Count <= 0){
			Debug.Log("Drawing queue is empty");
			//break;
		}

		int numberOfLetters = m_drawingQueue.Count;
		Vector3 letterPosition = Vector3.zero;
		float letterPositionX = 0;
		Word word = Instantiate(m_wordPrefab, transform.position, Quaternion.identity) as Word;
		word.SetWordSpawnerReference(this);

		for (int i = 0; i < numberOfLetters; i++){
			letterPositionX = transform.position.x /* - (numberOfLetters * word.m_wordSpacing)/2.0f */ + (i * word.m_wordSpacing);
			letterPosition = new Vector3(letterPositionX, transform.position.y, 0);
			Letter letter = SpawnLetter(m_drawingQueue[i], letterPosition);
			if (letter != null){
				letter.transform.parent = word.transform;
			}
		}
		// Empty Queue

		m_drawingQueue = new List<Drawing>();

		// Tell game GameManager that the word has been spawned
		GameManager.s_gameManager.OnWordSpawned();
	}

	// TODO: deprecated
	// Spawn a complete word
	public void SpawnWord(int[] _playerIds){
		if (_playerIds.Length <= 0){
			Debug.LogWarning("There are no players");
		} else{

			int numberOfLetters = _playerIds.Length;
			Vector3 letterPosition = Vector3.zero;
			float letterPositionX = 0;
			Word word = Instantiate(m_wordPrefab, transform.position, Quaternion.identity) as Word;
			word.SetWordSpawnerReference(this);

			for (int i = 0; i < numberOfLetters; i++){
				letterPositionX = transform.position.x - (numberOfLetters * word.m_wordSpacing)/2.0f + (i * word.m_wordSpacing);
				letterPosition = new Vector3(letterPositionX, transform.position.y, 0);
				Letter letter = SpawnLetter(_playerIds[i], letterPosition);
				if (letter != null){
					letter.transform.parent = word.transform;
				}
			}

			word.transform.localScale *= Random.Range(m_wordScaleRange.x, m_wordScaleRange.y);
		}
	}

	private Letter SpawnLetter(Drawing _drawing, Vector3 _position){
		if (_drawing != null){
			Letter letter = Instantiate(m_letterPrefab, _position, Quaternion.identity) as Letter;

			Vector3 position;
			Color color = PlayerManager.s_playerManager.GetPlayerReference(_drawing.playerId).GetPlayerColor();
			float letterScale = PlayerManager.s_playerManager.GetPlayerReference(_drawing.playerId).letterScale;

			for (int y = 0; y < _drawing.height; y += 1*m_arrayDivisionFactor){
				for (int x = 0; x < _drawing.width; x += 1*m_arrayDivisionFactor){
					if (_drawing.drawing[_drawing.width*y + x] > 0){
						position = _position + new Vector3((x * _drawing.gap) - (_drawing.width / 2 * _drawing.gap), (_drawing.height / 2 * _drawing.gap) - (y * _drawing.gap), 0) * letterScale;
						LetterPixel pixel = spawnLetterPixel(position, 1f, color, _drawing.accuracy);
						pixel.transform.SetParent(letter.transform);
						pixel.transform.localScale *= m_arrayDivisionFactor;
					}
				}
			}

			return letter;
		}
		return null;
	}

	// TODO: deprecated
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

			for (int y = 0; y < height; y += 1*m_arrayDivisionFactor){
				for (int x = 0; x < width; x += 1*m_arrayDivisionFactor){
					if (drawing[width*y + x] > 0){
						position = _position + new Vector3((x * gap) - (width / 2 * gap), (height / 2 * gap) - (y * gap), 0) * player.letterScale;
						LetterPixel pixel = spawnLetterPixel(position, pixelScale, color, accuracy);
						pixel.transform.SetParent(letter.transform);
						pixel.transform.localScale *= m_arrayDivisionFactor;
					}
				}
			}

			return letter;
		}
		Debug.LogWarning("There is no drawing");
		return null;
	}

	// TODO: basically done
	// Spawns a letterPixel
	private LetterPixel spawnLetterPixel(Vector3 _position, float _scale, Color _color, float _accuracy){
		LetterPixel letterPixel = Instantiate(m_letterPixelPrefab, _position, Quaternion.identity) as LetterPixel;

		letterPixel.Init(_accuracy, _color);

		//letterPixel.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.black, _color, _accuracy);
		//letterPixel.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit *= _scale;

		return letterPixel;
	}

}
