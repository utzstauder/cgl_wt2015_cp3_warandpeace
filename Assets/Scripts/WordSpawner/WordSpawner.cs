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
	public float m_letterScale = 1.0f;

	public Vector2 m_wordScaleRange = new Vector2(0.5f, 1.0f);

	public Transform m_wordDestroyer;
	private float deathX = 0;

	// the drawing queue
	private List<Drawing> m_drawingQueue;
	private List<int> m_teamSubmissionOrder;

	public float m_timeUntilNextDrawingInFreeMode = 2.0f;

	// pooled letterPixel objects
	private List<LetterPixel> m_letterPixelPool;
	public int m_initialPoolSize = 10000;

	void Awake(){
		m_drawingQueue = new List<Drawing>();
		m_teamSubmissionOrder = new List<int>();
	}

	// Use this for initialization
	void Start () {
		m_wordDestroyer = GameObject.FindGameObjectWithTag("WordDestroyer").transform;

		deathX = m_wordDestroyer.position.x;

		// Fill the letterPixelPool
		m_letterPixelPool = new List<LetterPixel>();

		StartCoroutine(AddPixelsToPool(m_initialPoolSize));
	}

	private IEnumerator AddPixelsToPool(int _amount){
		for (int i = 0; i < m_initialPoolSize; i++){
			LetterPixel letterPixel = Instantiate(m_letterPixelPrefab, Vector3.zero, Quaternion.identity) as LetterPixel;
			letterPixel.transform.parent = this.transform;
			letterPixel.gameObject.SetActive(false);
			m_letterPixelPool.Add(letterPixel);

			if (i%(_amount/10) == 0) yield return new WaitForSeconds(3.0f/10.0f);
		}
		Debug.Log("PixelPool filled");
	}

	public void DeactivateAllPixelsInPool(){
		for (int i = 0; i < m_letterPixelPool.Count; i++){
			if (m_letterPixelPool[i]) m_letterPixelPool[i].gameObject.SetActive(false);
		}
	}

	/*
	 * Looks for an inactive letterPixel object
	 */
	private LetterPixel GetInactivePixelFromPool(){
		foreach (LetterPixel pixel in m_letterPixelPool){
			if (!pixel.gameObject.activeSelf) return pixel;
		}
		return null;
	}

	public float GetDeathX(){
		return deathX;
	}


	/*
	 * Call this in word mode
	 * Accepts only one drawing per player
	 */
	public void AddLetterDrawingToQueue(Drawing _drawing){
		//Debug.Log("AddLetterDrawingToQueue");
		bool alreadyInQueue = false;

		foreach (Drawing drawing in m_drawingQueue){
			if (drawing.playerId == _drawing.playerId){
				alreadyInQueue = true;
				Debug.Log("Player " + PlayerManager.s_playerManager.GetPlayerReference(_drawing.playerId).name + " already submitted a drawing");
			}
		}

		if (!alreadyInQueue){
			m_drawingQueue.Add(_drawing);

			// check if all team members are done
			if (IsTeamReady(_drawing.teamId) && !m_teamSubmissionOrder.Contains(_drawing.teamId)){
				// add team to submission order
				m_teamSubmissionOrder.Add(_drawing.teamId);
			}
		}

		//Debug.Log("Queue contains " + m_drawingQueue.Count + " drawings");
	}

	/*
	 * Returns true if the DrawInputPlayer _player already submitted a drawing to the queue
	 */
	public bool IsPlayerDrawingInQueue(DrawInputPlayer _player){
		foreach (Drawing drawing in m_drawingQueue){
			if (drawing.playerId == _player.GetSessionId()){
				Debug.Log("Player already submitted a drawing. The team is safe!");
				return true;
			}
		}

		return false;
	}

	/*
	 * Call this in free mode
	 * Accepts multiply drawings per player
	 */
	public void AddFreeDrawingToQueue(Drawing _drawing){
		m_drawingQueue.Add(_drawing);
	}

	public void EmptyQueue(){
		m_drawingQueue.Clear();
		m_teamSubmissionOrder.Clear();
	}

	public bool IsQueueFull(){
		return (PlayerManager.s_playerManager.GetNumberOfPlayerAtBeginningOfCurrentRound() == m_drawingQueue.Count);
	}

	public void SpawnLetterFromDrawing(Drawing _drawing){
		StartCoroutine(SpawnLetterFromDrawingCoroutine(_drawing));
	}

	private IEnumerator SpawnLetterFromDrawingCoroutine(Drawing _drawing){
		Word word = Instantiate(m_wordPrefab, transform.position, Quaternion.identity) as Word;
		word.SetWordSpawnerReference(this);

		/*Letter letter = SpawnLetter(_drawing, transform.position);
		if (letter != null){
			letter.transform.parent = word.transform;
		}
		yield return new WaitForEndOfFrame();
		*/

		yield return StartCoroutine(SpawnLetterCoroutine(_drawing, word.transform));

		word.GetComponent<ScrollWithBackground>().m_Scroll = true;
	}

	/*
	 * Call this to spawn the drawings of one team which have been submitted so far
	 * You may check for TeamSubmittedAllDrawings first
	 */
	public void SpawnWordsOfTeam(int _teamId){
		//Debug.Log("SpawnWordsOfTeam called");
		StartCoroutine(SpawnWordsOfTeamCoroutine(_teamId));
	}

	public IEnumerator SpawnWordsOfTeamCoroutine(int _teamId){
		//Debug.Log("SpawnWordsOfTeamCoroutine started");

		Vector3 letterPosition = Vector3.zero;
		float letterPositionX = 0;
		Word word = Instantiate(m_wordPrefab, transform.position, Quaternion.identity) as Word;
		word.SetWordSpawnerReference(this);

		// check every drawing in the queue for its team id and spawn the letter
		for(int d = 0; d < m_drawingQueue.Count; d++){
			//Debug.Log(m_drawingQueue[d].teamId + "==" + _teamId);
			if (m_drawingQueue[d].teamId == _teamId){

				/*
				Letter letter = SpawnLetter(m_drawingQueue[d], transform.position);
				letterPositionX = transform.position.x + (d * word.m_wordSpacing);
				letterPosition = new Vector3(letterPositionX, transform.position.y, 0);
				letter.transform.position = letterPosition;

				if (letter != null){
					letter.transform.parent = word.transform;
				}

				yield return new WaitForSeconds(.33f);
				*/

				yield return StartCoroutine(SpawnLetterCoroutine(m_drawingQueue[d], word.transform));
			}
		}

		for (int c = 0; c < word.transform.childCount; c++){
			letterPositionX = transform.position.x + (c * word.m_wordSpacing);
			letterPosition = new Vector3(letterPositionX, transform.position.y, 0);
			word.transform.GetChild(c).transform.position = letterPosition;
		}

		word.GetComponent<ScrollWithBackground>().m_Scroll = true;
	}

	/*
	 * Call this to spawn all drawings in the order of which team submitted all drawings first 
	 */
	public void SpawnWordsFromQueueByTeamId(){
		StartCoroutine(SpawnWordsFromQueueByTeamIdCoroutine());
	}

	private IEnumerator SpawnWordsFromQueueByTeamIdCoroutine(){

		// Count through every team that participated
		for (int i = 0; i < m_teamSubmissionOrder.Count; i++){

			Vector3 letterPosition = Vector3.zero;
			float letterPositionX = 0;
			Word word = Instantiate(m_wordPrefab, transform.position, Quaternion.identity) as Word;
			word.SetWordSpawnerReference(this);

			// check every drawing in the queue for its team id and spawn the letter
			for(int d = 0; d < m_drawingQueue.Count; d++){
				if (m_drawingQueue[d].teamId == m_teamSubmissionOrder[i]){
				
					Letter letter = SpawnLetter(m_drawingQueue[d], transform.position);
					letterPositionX = transform.position.x + (d * word.m_wordSpacing);
					letterPosition = new Vector3(letterPositionX, transform.position.y, 0);
					letter.transform.position = letterPosition;

					if (letter != null){
						letter.transform.parent = word.transform;
					}

					yield return new WaitForSeconds(.33f);
				}
			}

			// wait until spawning the next word
			yield return new WaitForSeconds(2.0f);
		}

		// empty queue(s) when done
		EmptyQueue();
	}

	/*
	 * Returns TRUE if every team member submitted a drawing
	 * Should also account for disconnect fallback
	 */
	public bool IsTeamReady(int _teamId){
		int submittedDrawings = 0;
		int playersInTeamAtBeginningOfRound = PlayerManager.s_playerManager.GetNumberOfPlayersInTeamAtBeginningOfRound(_teamId);
		// List<DrawInputPlayer> playersInTeam = PlayerManager.s_playerManager.GetPlayersOfTeam(_teamId);

		foreach(Drawing drawing in m_drawingQueue){
			if (drawing.teamId == _teamId) submittedDrawings++;
		}

		// ">=" accounts for disconnect fallback.
		// There is only one drawing per player, so we are fine
		// if someone submitted their drawing and disconnected afterwards



		if (submittedDrawings >= playersInTeamAtBeginningOfRound){
			return true;
		}
		Debug.Log("Team " + _teamId + " submitted " + submittedDrawings + " of " + playersInTeamAtBeginningOfRound + " drawings.");
		return false;
	}

	/*
	 * Starts the coroutine in free mode
	 */
	public void SpawnDrawingsFromQueue(){
		StartCoroutine(SpawnDrawingsFromQueueCoroutine());
	}

	/*
	 * Ends the coroutine in free mode
	 */
	public void StopSpawning(){
		StopCoroutine(SpawnDrawingsFromQueueCoroutine());
	}

	/*
	 * This checks for drawing in the queue every m_timeUntilNextDrawingInFreeMode seconds
	 * and spawns them as words
	 */
	private IEnumerator SpawnDrawingsFromQueueCoroutine(){
		Debug.Log("Started spawning coroutine");

		while (true){
			if (GameManager.s_gameManager.m_currentState == GameManager.GameState.playing_free){
				// dont do anything if there is no drawing
				if (m_drawingQueue.Count >= 1){
					// pull drawing
					Drawing drawing = m_drawingQueue[0];
				
					// spawn drawing
					SpawnLetterFromDrawing(drawing);

					// remove drawing from queue afterwards
					m_drawingQueue.Remove(drawing);
				}
			}
			yield return new WaitForSeconds(m_timeUntilNextDrawingInFreeMode);
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

		m_drawingQueue.Clear();

		// Tell game GameManager that the word has been spawned
		GameManager.s_gameManager.OnWordSpawned();
	}

	/*
	 * use this for showcasing / screensaver
	 */
	public void SpawnWordFromString(string _word){
		StartCoroutine(SpawnWordFromStringCoroutine(_word));
	}

	private IEnumerator SpawnWordFromStringCoroutine(string _word){
		// convert to lower case
		_word = _word.ToLower();

		Vector3 letterPosition = Vector3.zero;
		float letterPositionX = 0;
		Word word = Instantiate(m_wordPrefab, transform.position, Quaternion.identity) as Word;
		word.SetWordSpawnerReference(this);

		for (int i = 0; i < _word.Length; i++){
			/*
			Letter letter = SpawnLetterFromChar(_word[i]);
			letterPositionX = transform.position.x + (i * word.m_wordSpacing/2);
			letterPosition = new Vector3(letterPositionX, transform.position.y, 0);
			letter.transform.position = letterPosition;
			if (letter != null){
				letter.transform.parent = word.transform;
			}

			yield return new WaitForEndOfFrame();
			*/

			yield return StartCoroutine(SpawnLetterFromCharCoroutine(_word[i], word.transform));
			//Debug.Log("Spawned " + _word[i]);
			//yield return new WaitForSeconds(.1f);
		}

		for (int c = 0; c < word.transform.childCount; c++){
			letterPositionX = transform.position.x + (c * word.m_wordSpacing/2);
			letterPosition = new Vector3(letterPositionX, transform.position.y, 0);
			word.transform.GetChild(c).transform.position = letterPosition;
		}

		word.GetComponent<ScrollWithBackground>().m_Scroll = true;
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
				letterPositionX = transform.position.x /*- (numberOfLetters * word.m_wordSpacing)/2.0f */ + (i * word.m_wordSpacing);
				letterPosition = new Vector3(letterPositionX, transform.position.y, 0);
				Letter letter = SpawnLetter(_playerIds[i], letterPosition);
				if (letter != null){
					letter.transform.parent = word.transform;
				}
			}

			word.transform.localScale *= Random.Range(m_wordScaleRange.x, m_wordScaleRange.y);
		}
	}

	private Letter SpawnLetterFromChar(char _letter){
		Drawing currentLetterDrawing = AlphabetManager.LetterToDrawing(AlphabetManager.CharToInt(_letter), m_arrayDivisionFactor/2);

		if (currentLetterDrawing != null){

			Letter letter = Instantiate(m_letterPrefab, transform.position, Quaternion.identity) as Letter;
			Vector3 position;
			// TODO: set color?
			Color color = Random.ColorHSV();
		
			for (int y = 0; y < currentLetterDrawing.height; y += 2*m_arrayDivisionFactor){
				for (int x = 0; x < currentLetterDrawing.width; x += 2*m_arrayDivisionFactor){
					if (currentLetterDrawing.drawing[currentLetterDrawing.width*y + x] > 0){
						position = transform.position + new Vector3((x * currentLetterDrawing.gap) - (currentLetterDrawing.width / 2 * currentLetterDrawing.gap), (currentLetterDrawing.height / 2 * currentLetterDrawing.gap) - (y * currentLetterDrawing.gap), 0) * m_letterScale;
						LetterPixel pixel = spawnLetterPixel(position, m_letterScale * m_arrayDivisionFactor, color, currentLetterDrawing.accuracy);
						pixel.transform.SetParent(letter.transform);
					}
				}
			}

			return letter;
		}

		return null;
		//return SpawnLetter(currentLetterDrawing, Vector3.zero);
	}

	private Letter SpawnLetter(Drawing _drawing, Vector3 _position){
		if (_drawing != null){
			Letter letter = Instantiate(m_letterPrefab, _position, Quaternion.identity) as Letter;

			Vector3 position;
			Color color = Color.white;
			if (GameManager.s_gameManager.m_currentState == GameManager.GameState.playing_word){
				color = PlayerManager.s_playerManager.GetTeamColor(_drawing.teamId);
			} else if (GameManager.s_gameManager.m_currentState == GameManager.GameState.playing_free){
				color = PlayerManager.s_playerManager.GetPlayerReference(_drawing.playerId).GetPlayerColor();
			}

			for (int y = 0; y < _drawing.height; y += m_arrayDivisionFactor){
				for (int x = 0; x < _drawing.width; x += m_arrayDivisionFactor){
					if (_drawing.drawing[_drawing.width*y + x] > 0){
						position = _position + new Vector3((x * _drawing.gap) - (_drawing.width / 2 * _drawing.gap), (_drawing.height / 2 * _drawing.gap) - (y * _drawing.gap), 0) * m_letterScale;
						LetterPixel pixel = spawnLetterPixel(position, m_letterScale * m_arrayDivisionFactor, color, _drawing.accuracy);
						pixel.transform.SetParent(letter.transform);
					}
				}
			}

			return letter;
		}
		return null;
	}

	private IEnumerator SpawnLetterCoroutine(Drawing _drawing, Transform _parent){
		if (_drawing != null){
			Letter letter = Instantiate(m_letterPrefab, transform.position, Quaternion.identity) as Letter;

			letter.transform.parent = _parent;

			Vector3 position;
			Color color = Color.white;
			if (GameManager.s_gameManager.m_currentState == GameManager.GameState.playing_word){
				color = PlayerManager.s_playerManager.GetTeamColor(_drawing.teamId);
			} else if (GameManager.s_gameManager.m_currentState == GameManager.GameState.playing_free){
				color = PlayerManager.s_playerManager.GetPlayerReference(_drawing.playerId).GetPlayerColor();
			}

			for (int y = 0; y < _drawing.height; y += m_arrayDivisionFactor){
				for (int x = 0; x < _drawing.width; x += m_arrayDivisionFactor){
					if (_drawing.drawing[_drawing.width*y + x] > 0){
						position = transform.position + new Vector3((x * _drawing.gap) - (_drawing.width / 2 * _drawing.gap), (_drawing.height / 2 * _drawing.gap) - (y * _drawing.gap), 0) * m_letterScale;
						LetterPixel pixel = spawnLetterPixel(position, m_letterScale * m_arrayDivisionFactor, color, _drawing.accuracy);
						if (letter) {
							pixel.transform.SetParent(letter.transform);
						} else {
							Destroy(pixel.gameObject);
						}
					}
				}

				yield return new WaitForEndOfFrame();
			}

			if (letter.transform.childCount <= 0) Destroy(letter.gameObject);
		}
	}

	private IEnumerator SpawnLetterFromCharCoroutine(char _char, Transform _parent){
		Drawing drawing = AlphabetManager.LetterToDrawing(AlphabetManager.CharToInt(_char), m_arrayDivisionFactor/2);

		Letter letter = Instantiate(m_letterPrefab, transform.position, Quaternion.identity) as Letter;

		if (_parent){
			letter.transform.parent = _parent;
		}
		else {
			Destroy(letter.gameObject);
		}

		Vector3 position;
		Color color = Color.white;

		for (int y = 0; y < drawing.height; y += m_arrayDivisionFactor){
			for (int x = 0; x < drawing.width; x += m_arrayDivisionFactor){
				if (drawing.drawing[drawing.width*y + x] > 0){
					position = transform.position + new Vector3((x * drawing.gap) - (drawing.width / 2 * drawing.gap), (drawing.height / 2 * drawing.gap) - (y * drawing.gap), 0) * m_letterScale;
					LetterPixel pixel = spawnLetterPixel(position, m_letterScale * m_arrayDivisionFactor, color, drawing.accuracy);
					if (letter) {
						pixel.transform.SetParent(letter.transform);
					} else {
						Destroy(pixel.gameObject);
					}
				}
			}

			yield return new WaitForEndOfFrame();
		}
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

		if (drawing != null){
			Letter letter = Instantiate(m_letterPrefab, _position, Quaternion.identity) as Letter;

			for (int y = 0; y < height; y += 1*m_arrayDivisionFactor){
				for (int x = 0; x < width; x += 1*m_arrayDivisionFactor){
					if (drawing[width*y + x] > 0){
						position = _position + new Vector3((x * gap) - (width / 2 * gap), (height / 2 * gap) - (y * gap), 0) * m_letterScale;
						LetterPixel pixel = spawnLetterPixel(position, m_letterScale * (m_arrayDivisionFactor/2.0f), color, accuracy);
						pixel.transform.SetParent(letter.transform);
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
		//LetterPixel letterPixel = Instantiate(m_letterPixelPrefab, _position, Quaternion.identity) as LetterPixel;
		LetterPixel letterPixel = GetInactivePixelFromPool();

		// If there is no inactive pixel add more to the pool
		if (letterPixel == null){
			letterPixel = Instantiate(m_letterPixelPrefab, Vector3.zero, Quaternion.identity) as LetterPixel;
			m_letterPixelPool.Add(letterPixel);
		}

		letterPixel.gameObject.SetActive(true);
		letterPixel.transform.position = _position;
		letterPixel.transform.localScale = Vector3.one * _scale;
		letterPixel.Init(_accuracy, _color, this.transform);

		//letterPixel.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.black, _color, _accuracy);
		//letterPixel.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit *= _scale;

		return letterPixel;
	}

}
