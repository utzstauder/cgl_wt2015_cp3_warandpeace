using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public enum GameState{initializing, paused, playing_free, playing_word, end};


	// Debug varliables
	public bool m_debug = false;

	// Game variables
	public GameState m_currentState;
	private GameState m_previousState;
	private int m_score = 0;

	// Screensaver
	public string[] m_screensaverWords = {"Drawnes"};
	public float m_timeInBetweenWords = 20.0f;
	private int m_screensaverWordIndex = 0;

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
	private float m_roundTimer = 0;

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

	void StartGame(){
		if (PlayerManager.s_playerManager.GetNumberOfPlayers() >= PlayerManager.s_playerManager.m_minPlayersPerTeam){
			ChangeGameState(GameState.playing_word);
		} else ChangeGameState(GameState.playing_free);

		InvokeRepeating("Screensaver", 0, m_timeInBetweenWords);

		PlayerManager.s_playerManager.BroadcastNotification("This is a very long paragraph to test the awesome text wrapper code!");

		StartCoroutine(GameLoop(m_currentState));
	}

	private void ChangeGameState(GameState _targetState){
		m_previousState = m_currentState;
		m_currentState = _targetState;

		/*string gameStateToSend = GetGameStateAsString(m_currentState);

		for (int i = 0; i < PlayerManager.s_playerManager.GetNumberOfPlayers(); i++){
			PlayerManager.s_playerManager.GetPlayerReference(i).SetGameState(gameStateToSend);
		}

		// TODO: don't know if this belongs here
		if (PlayerManager.s_playerManager.GetNumberOfPlayers() > 0 &&
			PlayerManager.s_playerManager.GetNumberOfPlayersInCurrentRound() <= 0 &&
			m_currentState == GameState.playing_word){
			StartNewRound();
		}*/
	}

	// TODO: implement
	private void ResetGame(){
		StopAllCoroutines();
	}

	/*
	 * DEPRECATED; use GameLoop coroutine instead
	 * This function gets called whenever a previous round end or in the beginning of the game
	 */
	public void StartNewRound(){
		int numberOfPlayers = PlayerManager.s_playerManager.GetNumberOfPlayers();
		//Debug.Log("numberOfPlayers: " + numberOfPlayers);

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
		// set flag for "round started"
		m_roundStarted = true;

		// reset/start (internal) clock for round time
		m_roundTimer = 0;

		// determine who is playing the current round
		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(true);

		// communicate game mode to player clients
		PlayerManager.s_playerManager.BroadcastCurrentGameMode(m_currentState);

		if (_mode == GameState.playing_word){
			// +++ WORD MODE +++

			// if in WORD mode, assign player clients to teams (if needed)
			PlayerManager.s_playerManager.AssignPlayersToTeams();

			// notify the players
			PlayerManager.s_playerManager.BroadcastNotificationToCurrentRound("You are playing WORD MODE! Communicate with the other players of your color and draw the letters in the correct order!");

			int numberOfTeams = PlayerManager.s_playerManager.GetNumberOfTeams();

			// send letter templates to player clients / teams
			for (int i = 1; i <= numberOfTeams; i++){
				// get a list of players of team i
				List<DrawInputPlayer> playersInCurrentTeam = PlayerManager.s_playerManager.GetPlayersOfTeam(i);

				// get a random word of length = number of players in team i
				string word = WordManager.s_wordManager.GetRandomWord(playersInCurrentTeam.Count);

				// distribute the letters to the team members randomly
				playersInCurrentTeam.Shuffle();

				for (int c = 0; c < word.Length; c++){
					playersInCurrentTeam[c].DrawLetterOnBackgroundFromString(word[c].ToString());
				}
			}

			Debug.Log("Entering reception loop");
			while (true){
				// LOOP; wait for every player to send their drawing

				// inner LOOP; check for finished teams
				for (int t = 1; t <= numberOfTeams; t++){
					if (m_wordSpawner.IsTeamReady(t)){
						m_wordSpawner.SpawnWordsOfTeam(t);

						yield return new WaitForSeconds(PlayerManager.s_playerManager.GetPlayersOfTeam(t).Count * 2.0f);
					}
				}
				// TODO: check results (correct order? accuracy?)

				if (m_wordSpawner.IsQueueFull()) break;
				else yield return new WaitForSeconds(2.0f);
			}
			Debug.Log("Leaving reception loop");

			// display the drawings on screen
			// first come first serve; fastest team or best accuracy? both at the same time
			// right now, the fastest team gets drawn first
			// m_wordSpawner.SpawnWordsFromQueueByTeamId();

			// TODO: players that are done will receive a "waiting for player(s)..." promt
			// this will probably happen on the smartphone



		} else if (_mode == GameState.playing_free){
			// +++ FREE MODE +++

			// pick random colors for all players
			PlayerManager.s_playerManager.AssignRandomColors();

			// notify the players
			PlayerManager.s_playerManager.BroadcastNotificationToCurrentRound("You are playing FREE MODE! Draw anything you want and submit it to the game!");
		
			while (_mode == GameState.playing_free){
				yield return new WaitForEndOfFrame();
			}
		}

		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(false);

		// remove flag for "round started"
		m_roundStarted = false;

		yield return new WaitForEndOfFrame();
	}

	private void Screensaver(){
		if (PlayerManager.s_playerManager.GetNumberOfPlayers() <= 0){
			Debug.Log("screensaver is running...");
			m_wordSpawner.SpawnWordFromString(m_screensaverWords[m_screensaverWordIndex++%m_screensaverWords.Length]);
		} else CancelInvoke();
	}

	/*
	 * 
	 */
	private void OnPlayerJoin(){
		
		if (PlayerManager.s_playerManager.GetNumberOfPlayers() > 0){
			CancelInvoke();

			// if there is at least one player and no round is being played start one
			if (PlayerManager.s_playerManager.GetNumberOfPlayersInCurrentRound() <= 0 &&
			m_currentState == GameState.playing_word){

				// TODO: (re)move this
				StartNewRound();
			}
		}

		// TODO: send wait message if in word mode

		// TODO: free mode loop ???
	}

	/*
	 * 
	 */
	private void OnPlayerLeave(){
		/*
		 * DEPRECATED
		if (m_currentState == GameState.playing_word){
			// TODO: Check which letter the player was supposed to draw
			// TODO: communicate to other players

			// Check if queue is now full
			if (m_wordSpawner.IsQueueFull()){
				m_wordSpawner.SpawnLettersFromQueue();
			}
				
			if (PlayerManager.s_playerManager.GetNumberOfPlayers() < 2){
				// not enough players for word mode
				ChangeGameState(GameState.playing_free);
			}

			if (PlayerManager.s_playerManager.GetNumberOfPlayers() <= 0){
				InvokeRepeating("Screensaver", 0, m_timeInBetweenWords);
			}
		}
		*/
	}

	// This is called once a word was successfully generated
	public void OnWordSpawned(){
		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(true);
		StartNewRound();
	}

	// TODO: for testing
	public void OnPlayerReceivedDrawing(DrawInputPlayer _player){
		//Debug.Log("OnPlayerReceivedDrawing");
		if (m_currentState == GameState.playing_word){
			m_wordSpawner.AddLetterDrawingToQueue(_player.GetCurrentDrawing());

			/*// Check if queue full
			if (m_wordSpawner.IsQueueFull()){
				m_wordSpawner.SpawnLettersFromQueue();
			}*/

		} else if (m_currentState == GameState.playing_free) {
			// spawn free drawings
			m_wordSpawner.SpawnLetterFromDrawing(_player.GetCurrentDrawing());
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
		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(true);

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

		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(true);

		string word = WordManager.s_wordManager.GetRandomWord(currentNumberOfPlayers);
		Debug.Log(word);

		int[] wordArray = WordManager.s_wordManager.GetWordAsIntArrayFromString(word);

		// shuffle array
		ArrayFunctions.RandomizeArray(ref wordArray);

		for (int i = 0; i < currentNumberOfPlayers; i++){
			//PlayerManager.s_playerManager.GetPlayerReference(i).DrawLetterOnBackground(wordArray[i]);
			PlayerManager.s_playerManager.GetPlayerReference(i).DrawLetterOnBackgroundFromString(word[i].ToString());
			// TODO: for testing purposes
			PlayerManager.s_playerManager.GetPlayerReference(i).PlaySound("newLetters");
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
			if (IsPlaying() && m_debug){
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

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 - m_menuButtonSpacing * 1.5f , m_menuButtonWidth, m_menuButtonHeight * 3), "START GAME")){
				StartGame();
				//ChangeGameState(GameState.playing_word);
			}

			/*if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 - m_menuButtonSpacing/2, m_menuButtonWidth, m_menuButtonHeight), "FREE MODE")){
				ChangeGameState(GameState.playing_free);
			}*/

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + m_menuButtonSpacing/2, m_menuButtonWidth, m_menuButtonHeight), "OPEN QUESTIONAIRE")){
				Application.OpenURL("https://docs.google.com/forms/d/1DUZs3-u70XKN4UzRwLdavks2_PsPnbGpu0_gVTBcvS8/viewform");
			}

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + m_menuButtonSpacing * 1.5f , m_menuButtonWidth, m_menuButtonHeight), "QUIT GAME")){
				Application.Quit();
			}

			GUILayout.EndVertical();
		}

		if (ArtstyleManager.s_artstyleManager.GetCurrentStyle() == ArtstyleManager.Style.arcade){
			GUI.Label(new Rect(20, 10, 300, 30), 	"SCORE:      " + m_score);
		} else GUI.Label(new Rect(20, 10, 300, 30), "CASUALTIES: " + m_score);
	}
}
