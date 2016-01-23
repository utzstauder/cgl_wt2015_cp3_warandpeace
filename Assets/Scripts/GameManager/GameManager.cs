using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public enum GameState{initializing, paused, playing_free, playing_word, end};


	// Debug varliables
	public bool m_debug = false;

	// Game variables
	public GameState m_currentState;
	private GameState m_previousState;
	private int m_score = 0;


	// References
	public static GameManager s_gameManager;
	private WordSpawner m_wordSpawner;

	// Artstyle change variables
	public bool m_artstyleDefaultMode = true;
	private bool m_artstyleCooldownRunning = false;
	private float m_timeUntilReset = 1.0f;
	private float m_currentArtstyleTimer = 0.0f;

	// Menu variables
	public int m_menuButtonWidth = 160;
	public int m_menuButtonHeight = 20;
	public int m_menuButtonSpacing = 40;

	// Game loop variables
	private bool m_roundStarted = false;

	// Use this for initialization
	void Awake () {
		if (s_gameManager != null){
			Debug.LogError("There is more than one GameManager in the scene");
			Destroy(this);
		} else{
			s_gameManager = this;
		}
			
		m_currentState = GameState.initializing;
	}

	void Start(){
		m_wordSpawner = GameObject.FindGameObjectsWithTag("WordSpawner")[0].GetComponent<WordSpawner>();

		PlayerManager.OnPlayerJoin += OnPlayerJoin;
		PlayerManager.OnPlayerLeave += OnPlayerLeave;
	}

	void OnDestroy(){
		PlayerManager.OnPlayerJoin -= OnPlayerJoin;
		PlayerManager.OnPlayerLeave -= OnPlayerLeave;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)){
			if (m_currentState == GameState.paused){
				ChangeGameState(m_previousState);
			} else {
				ChangeGameState(GameState.paused);
			}
		}

		// Debug safety; see what I did there? ;-)
		if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.T)){
			ResetGame();
		}

		//Debug.Log(m_currentArtstyleTimer + " / " + m_timeUntilReset + " => current artstyle: " + ArtstyleManager.s_artstyleManager.m_currentStyle);
		//Debug.Log(m_timeUntilReset - m_currentArtstyleTimer);
	}

	private void ChangeGameState(GameState _targetState){
		m_previousState = m_currentState;
		m_currentState = _targetState;

		string gameStateToSend = GetGameStateAsString(m_currentState);

		for (int i = 0; i < PlayerManager.s_playerManager.GetNumberOfPlayers(); i++){
			PlayerManager.s_playerManager.GetPlayerReference(i).SetGameState(gameStateToSend);
		}

		// TODO: don't know if this belongs here
		if (PlayerManager.s_playerManager.GetNumberOfPlayers() > 0 &&
			PlayerManager.s_playerManager.GetNumberOfPlayersInCurrentRound() <= 0 &&
			m_currentState == GameState.playing_word){
			StartNewRound();
		}
	}

	// TODO: implement
	private void ResetGame(){
		StopAllCoroutines();
	}

	/*
	 * This function gets called whenever a previous round end or in the beginning of the game
	 */
	public void StartNewRound(){
		int numberOfPlayers = PlayerManager.s_playerManager.GetNumberOfPlayers();
		Debug.Log("numberOfPlayer :" + numberOfPlayers);

		if (m_currentState == GameState.playing_word){
			if (numberOfPlayers >= 2){
				SendRandomWord();
			} else {
				ChangeGameState(GameState.playing_free);
				// TODO: ???
				StartNewRound();
			}

		} else if (m_currentState == GameState.playing_free){
			
		}
	}

	/*
	 * This coroutine is running while a round is played
	 */
	private IEnumerator GameLoop(GameState _mode){
		// TODO: set flag for "round started"
		// TODO: start (internal) clock for round time

		// TODO: communicate game mode to player clients


		if (_mode == GameState.playing_word){

			// TODO: if in WORD mode, assign player clients to teams (if needed)
			
		}

		// Display logo on player clients (logic will happen on smartphone)


		// TODO: Assign color to player clients; team color in word mode or random color in free mode

		if (_mode == GameState.playing_word){

			// TODO: if in WORD mode, send letter templates to player clients

			// TODO: LOOP; wait for every player to send their drawing
			// TODO: players that are done will receive a "waiting for player(s)..." promt

			// TODO: inner LOOP; wait for team members
			// TODO: check results (correct order? accuracy?)

			// TODO: display the drawings on screen
			// TODO: first come first serve; fastest team or best accuracy? both at the same time?
		}

		// TODO: remove flag for "round started"

		yield return new WaitForEndOfFrame();
	}


	/*
	 * 
	 */
	private void OnPlayerJoin(){
		// if there is at least one player and no round is being played start one
		if (PlayerManager.s_playerManager.GetNumberOfPlayers() > 0 &&
			PlayerManager.s_playerManager.GetNumberOfPlayersInCurrentRound() <= 0 &&
			m_currentState == GameState.playing_word){

			// TODO: (re)move this
			StartNewRound();
		}

		// TODO: send wait message if in word mode

		// TODO: free mode loop ???
	}

	/*
	 * 
	 */
	private void OnPlayerLeave(){
		if (m_currentState == GameState.playing_word){
			// TODO: Check which letter the player was supposed to draw
			// TODO: communicate to other players

			// Spawn words that are already in queue
			PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(PlayerManager.s_playerManager.GetNumberOfPlayersInCurrentRound() - 1);

			// Check if queue full
			if (m_wordSpawner.IsQueueFull()){
				m_wordSpawner.SpawnLettersFromQueue();
			}

			if (PlayerManager.s_playerManager.GetNumberOfPlayers() < 2){
				// not enough players for word mode
				ChangeGameState(GameState.playing_free);
			}
		}
	}

	// This is called once a word was successfully generated
	public void OnWordSpawned(){
		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(0);
		StartNewRound();
	}

	// TODO: for testing
	public void OnPlayerReceivedDrawing(DrawInputPlayer _player){
		//Debug.Log("OnPlayerReceivedDrawing");
		if (m_currentState == GameState.playing_word){
			Drawing playerDrawing = _player.GetCurrentDrawing();
			m_wordSpawner.AddLetterDrawingToQueue(playerDrawing);

			// Check if queue full
			if (m_wordSpawner.IsQueueFull()){
				m_wordSpawner.SpawnLettersFromQueue();
			}

		} else if (m_currentState == GameState.playing_free) {
			// TODO: spawn free drawings
			Drawing playerDrawing = _player.GetCurrentDrawing();
			m_wordSpawner.SpawnLetterFromDrawing(playerDrawing);
		}
		
		/*int[] playerIds = new int[1];
		playerIds[0] = PlayerManager.s_playerManager.GetPlayerIndex(_player);
		m_wordSpawner.SpawnWord(playerIds);*/
	}

	/* Start a round with random letters
	 * TODO: DEPRECATED
	 */
	public void SendRandomLetters(){
		int currentNumberOfPlayers = PlayerManager.s_playerManager.GetNumberOfPlayers();
		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(currentNumberOfPlayers);

		int[] randomLetters = WordManager.s_wordManager.GetRandomLetters(currentNumberOfPlayers);

		for (int i = 0; i < PlayerManager.s_playerManager.GetNumberOfPlayers(); i++){
			PlayerManager.s_playerManager.GetPlayerReference(i).DrawLetterOnBackground(randomLetters[i]);
		}
	}

	/*
	 * Start a round with a random word
	 * 
	 */
	public void SendRandomWord(){
		int currentNumberOfPlayers = PlayerManager.s_playerManager.GetNumberOfPlayers();

		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(currentNumberOfPlayers);

		string word = WordManager.s_wordManager.GetRandomWord(currentNumberOfPlayers);
		Debug.Log(word);

		int[] wordArray = WordManager.s_wordManager.GetWordAsIntArrayFromString(word);

		// shuffle array
		ArrayFunctions.RandomizeArray(ref wordArray);

		for (int i = 0; i < currentNumberOfPlayers; i++){
			PlayerManager.s_playerManager.GetPlayerReference(i).DrawLetterOnBackground(wordArray[i]);
		}
	}

	public bool IsPlaying(){
		return m_currentState == GameState.playing_free || m_currentState == GameState.playing_word;
	}

	public string GetGameStateAsString(GameState _state){
		string gameStateToSend = "";

		switch (_state){
			case GameState.initializing: gameStateToSend = "initializing"; break;
			case GameState.playing_word: gameStateToSend = "playing_word"; break;
			case GameState.playing_free: gameStateToSend = "playing_free"; break;
			case GameState.paused: gameStateToSend = "paused"; break;
			case GameState.end: gameStateToSend = "end"; break;
		}

		return gameStateToSend;
	}

	public void AddScore(int _score){
		//Debug.Log("adding score " + _score);
		m_score += _score;
	}

	public void TriggerArtstyleChange(float _time){
		if (m_artstyleDefaultMode){
			m_timeUntilReset += _time/(1.0f + m_timeUntilReset);

			if (!m_artstyleCooldownRunning){
				StartCoroutine(ArtstyleChangeRoutine());
			}
		}

	}

	private IEnumerator ArtstyleChangeRoutine(){
		m_artstyleCooldownRunning = true;

		ArtstyleManager.s_artstyleManager.SetArtstyle(ArtstyleManager.Style.realistic);

		while (m_currentArtstyleTimer < m_timeUntilReset){
			m_currentArtstyleTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
			
		ArtstyleManager.s_artstyleManager.SetArtstyle(ArtstyleManager.Style.arcade);

		m_timeUntilReset = 0;
		m_currentArtstyleTimer = 0;

		m_artstyleCooldownRunning = false;
	}

	void OnGUI(){
		// This is for debugging only
		if (m_debug){
			/*if (m_wordSpawner != null){
				if (GUI.Button(new Rect(200, 30, 120, 20), "SpawnWord All P")){
					int[] playerIds = new int[PlayerManager.s_playerManager.GetNumberOfPlayers()];
					for (int i = 0; i < playerIds.Length; i++){
						playerIds[i] = i;
					}
					m_wordSpawner.SpawnWord(playerIds);
				}

				if (GUI.Button(new Rect(200, 10, 120, 20), "SpawnWord P1")){
					int numberOfDrawings = PlayerManager.s_playerManager.GetPlayerReference(0).GetNumberOfDrawings();
					int[] playerIds = new int[numberOfDrawings];
					for (int i = 0; i < numberOfDrawings; i++){
						playerIds[i] = 0;
					}
					m_wordSpawner.SpawnWord(playerIds);
				}
			}
			if (m_currentState == GameState.playing_word){
				if (GUI.Button(new Rect(10, 30, 120, 20), "Send Random Letters")){
					SendRandomLetters();
				}
			}*/
			if (IsPlaying()){
				if (GUI.Button(new Rect(10, 30, 120, 20), "ARCADE MODE")){
					m_artstyleDefaultMode = false;
					ArtstyleManager.s_artstyleManager.SetArtstyle(ArtstyleManager.Style.arcade);
				}
				if (GUI.Button(new Rect(140, 30, 120, 20), "REALISTIC MODE")){
					m_artstyleDefaultMode = false;
					ArtstyleManager.s_artstyleManager.SetArtstyle(ArtstyleManager.Style.realistic);
				}
				if (GUI.Button(new Rect(270, 30, 120, 20), "DEFAULT MODE")){
					m_artstyleDefaultMode = true;
					ArtstyleManager.s_artstyleManager.SetArtstyle(ArtstyleManager.Style.arcade);
				}
			}
		}

		if (m_currentState == GameState.initializing || m_currentState == GameState.paused){

			// Menu
			GUILayout.BeginVertical();

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 - m_menuButtonSpacing * 1.5f , m_menuButtonWidth, m_menuButtonHeight), "WORD MODE")){
				ChangeGameState(GameState.playing_word);
			}

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 - m_menuButtonSpacing/2, m_menuButtonWidth, m_menuButtonHeight), "FREE MODE")){
				ChangeGameState(GameState.playing_free);
			}

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + m_menuButtonSpacing/2, m_menuButtonWidth, m_menuButtonHeight), "OPEN QUESTIONAIRE")){
				Application.OpenURL("https://docs.google.com/forms/d/1DUZs3-u70XKN4UzRwLdavks2_PsPnbGpu0_gVTBcvS8/viewform");
			}

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + m_menuButtonSpacing * 1.5f , m_menuButtonWidth, m_menuButtonHeight), "QUIT GAME")){
				Application.Quit();
			}

			GUILayout.EndVertical();
		}

		GUI.Label(new Rect(20, 10, 300, 30), "SCORE: " + m_score);
	}
}
