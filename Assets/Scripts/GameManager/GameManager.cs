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
	private int m_highscore = 0;
	private string m_scoreText = "SCORE:      ";

	private bool m_pixelPoolReady = false;
	public GameObject m_titleScreen;

	// Screensaver
	public string[] m_screensaverWords = {"Drawnes"};
	public float m_timeInBetweenWords = 20.0f;
	private int m_screensaverWordIndex = 0;
	private bool m_screensaverIsRunning = false;

	// References
	public static GameManager s_gameManager;
	public WordSpawner m_wordSpawner;
	public Drone m_drone;

	// Artstyle change variables
	public bool m_artstyleDefaultMode = true;
	private bool m_artstyleCooldownRunning = false;
	private float m_timeUntilReset = 1.0f;
	private float m_currentArtstyleTimer = 0.0f;
	public int m_killsForArtstyleChangeEachRound = 50;
	private int m_killsForArtstyleChange = 0;
	private bool m_killsMetInLastRound = false;

	// Menu variables
	public int m_menuButtonWidth = 160;
	public int m_menuButtonHeight = 20;
	public int m_menuButtonSpacing = 40;

	// Game loop variables
	[Range(0.0f, 1.0f)]
	public float m_wordModeChance = 0.5f;
	private bool m_roundStarted = false;
	private float m_roundTimer = 0;
	private float m_freeModeRoundTime = 10.0f;
	public float m_secondsPerPlayerInFreeMode = 3.0f;
	public float m_timeUntilNextRound = 3.0f;
	public float m_waitAfterWordMode = 10.0f;
	private GameState m_nextMode = GameState.playing_word;
	private GameState m_prevRound = GameState.initializing;

	// Notification texts
	public string m_notificationFreeMode = "You are playing FREE MODE! Draw anything you want and submit it to the game!";
	public string m_notificationWordMode = "You are playing WORD MODE! Communicate with the other players of your color and draw the letters in the correct order!";
	public string m_notificationWordModeWaitForEndOfRound = "Please wait until the next round starts.";
	public string m_notificationWordModeWaitForRestOfTeam = "Awesome drawing! Now you just have to wait for your team to finish. Your accuracy was ";
	public string m_notificationWordModeCorrectOrder = "Great! You drew your word in the correct order!";
	public string m_notificationWordModeIncorrectOrder = "Oh my! Unfortunately the order was incorrect! The corect word was: ";
	public string m_notificationWordModeCorrectOrderWaitForOtherTeams = "Great! You drew your word in the correct order! But the other players aren't quite ready yet.";
	public string m_notificationWordModeIncorrectOrderWaitForOtherTeams = "Oh my! Unfortunately the order was incorrect! But the other players aren't quite ready yet. The corect word was: ";
	public string m_notificationGamePaused = "GAME PAUSED";
	public string m_notificationGameResumed = "GAME RESUMED";
	public string m_notificationInitializing = "Welcome to DRAWNES! Please wait until the game starts.";
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
		if (!m_titleScreen) m_titleScreen = GameObject.Find("TitleScreen");

		m_wordSpawner = GameObject.FindGameObjectsWithTag("WordSpawner")[0].GetComponent<WordSpawner>();
		m_drone = GameObject.Find("Drone").GetComponent<Drone>();

		PlayerManager.OnPlayerJoin += OnPlayerJoin;
		PlayerManager.OnPlayerLeave += OnPlayerLeave;

		PlayerManager.s_playerManager.BroadcastNotification(m_notificationInitializing, false, 0);
	}

	void OnDestroy(){
		PlayerManager.OnPlayerJoin -= OnPlayerJoin;
		PlayerManager.OnPlayerLeave -= OnPlayerLeave;
	}

	void OnApplicationQuit(){
		ChangeGameState(GameState.end);
		if(!m_debug) Application.OpenURL("https://docs.google.com/forms/d/1DUZs3-u70XKN4UzRwLdavks2_PsPnbGpu0_gVTBcvS8/viewform");
	}
	
	// Update is called once per frame
	void Update () {
		// handle button input
		if (Input.GetKeyDown(KeyCode.Escape) && (m_currentState != GameState.initializing || m_currentState == GameState.end)){
			if (m_currentState == GameState.paused){
				//PlayerManager.s_playerManager.BroadcastNotification(m_notificationGameResumed, true, 0);
				ChangeGameState(m_previousState);
			} else {
				ChangeGameState(GameState.paused);
				//PlayerManager.s_playerManager.BroadcastNotification(m_notificationGamePaused, false, 0);
			}
		}

		if (Input.GetKeyDown(KeyCode.Space) && m_currentState == GameState.initializing && m_pixelPoolReady){
			StartGame();
		}

		//Debug.Log(m_currentArtstyleTimer + " / " + m_timeUntilReset + " => current artstyle: " + ArtstyleManager.s_artstyleManager.m_currentStyle);
		//Debug.Log(m_timeUntilReset - m_currentArtstyleTimer);
	}

	public void SetPixelPoolReady(){
		m_pixelPoolReady = true;
	}

	/*
	 * This is called once the starts for the first time and the player hits START GAME in the menu
	 */
	void StartGame(){
		ResetGame();

		ChangeGameState(GameState.playing_free);

		// starts the coroutine that spawns drawings in free mode
		if (m_wordSpawner) m_wordSpawner.SpawnDrawingsFromQueue();

		//PlayerManager.s_playerManager.BroadcastNotification("This is a very long paragraph to test the awesome text wrapper code!");

		if (PlayerManager.s_playerManager.GetNumberOfPlayers() > 0){
			StartCoroutine(GameLoop());
		}
	}

	private void ChangeGameState(GameState _targetState){
		if (_targetState == GameState.initializing || _targetState == GameState.paused || _targetState == GameState.end){
			m_titleScreen.SetActive(true);
			Cursor.visible = true;
		} else {
			m_titleScreen.SetActive(false);
			Cursor.visible = false;
		}

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


	/*
	 * Returns either free or word mode
	 */
	private GameState GetRandomGameMode(){
		if (Random.Range(0.0f, 1.0f) < m_wordModeChance) return GameState.playing_word;

		return GameState.playing_free;
	}

	/*
	 * Resets everything to start a clean round
	 */
	private void ResetGame(){
		m_score = 0;
		m_screensaverWordIndex = 0;
		m_nextMode = GameState.playing_word;


		StopAllCoroutines();
		if (m_wordSpawner){
			//m_wordSpawner.StopSpawning();
			m_wordSpawner.StopAllCoroutines();
		}

		//InvokeRepeating("Screensaver", 0, m_timeInBetweenWords);

		m_wordSpawner.DeactivateAllPixelsInPool();

		StartCoroutine(Screensaver());
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
	private IEnumerator GameLoop(){
		while (true){
			Debug.Log("GameLoop started");

			// initialise variable for game mode
			// this is mostly just fallback
			GameState gamemode = GameState.playing_word;

			// determine game mode
			if (PlayerManager.s_playerManager.GetNumberOfPlayers() >= PlayerManager.s_playerManager.m_minPlayersPerTeam){
				// if there are enough players we can play both free and word mode
				Debug.Log("enough players for word mode");
				gamemode = m_nextMode;
			} else{
				// otherwise it's only free mode
				Debug.Log("not enough players for word mode");
				gamemode = GameState.playing_free;
			}

			ChangeGameState(gamemode);

			Debug.Log(gamemode);

			// change artstyle
			if (m_killsMetInLastRound){
				SwitchArtstyle();
			}
			m_killsMetInLastRound = false;

			// calculate neccessary kills
			m_killsForArtstyleChange = m_score + m_killsForArtstyleChangeEachRound;

			// wait for round to start
			for (float t = 0; t <= m_timeUntilNextRound - 1.0f; t += 1.0f){
				// notify players
				PlayerManager.s_playerManager.BroadcastNotification("Round starts in " + (int)(m_timeUntilNextRound - t) +  " seconds!", false, 1.0f);
				//PlayerManager.s_playerManager.BroadcastNotification("test", false, 0.9f);
				yield return new WaitForSeconds(1.0f);
			}
			Debug.Log("Notified players");

			// reset/start (internal) clock for round time
			m_roundTimer = 0;

			// determine who is playing the current round
			PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(true);

			// communicate game mode to player clients
			PlayerManager.s_playerManager.BroadcastCurrentGameMode(m_currentState);

			if (gamemode == GameState.playing_word){
				// +++ WORD MODE +++

				Debug.Log("Word mode started");

				// if in WORD mode, assign player clients to teams (if needed)
				PlayerManager.s_playerManager.AssignPlayersToTeamsRandom();

				// notify the players
				PlayerManager.s_playerManager.BroadcastNotificationToCurrentRound(m_notificationWordMode, true, 0);

				int numberOfTeams = PlayerManager.s_playerManager.GetNumberOfTeams();

				// create a variable to check if a teams' letters were already spawned
				bool[] alreadySpawned = new bool[numberOfTeams];
				for (int i = 0; i < alreadySpawned.Length; i++){
					alreadySpawned[i] = false;
				}

				// empty queue
				m_wordSpawner.EmptyQueue();

				// send letter templates to player clients / teams
				for (int i = 0; i < numberOfTeams; i++){
					// get a list of players of team i
					List<DrawInputPlayer> playersInCurrentTeam = PlayerManager.s_playerManager.GetPlayersOfTeam(i);

					// get a random word of length = number of players in team i
					string word = WordManager.s_wordManager.GetRandomWord(playersInCurrentTeam.Count);

					// add word to list
					m_wordSpawner.AddWordToList(word);

					// distribute the letters to the team members randomly
					playersInCurrentTeam.Shuffle();

					for (int c = 0; c < word.Length; c++){
						playersInCurrentTeam[c].DrawLetterOnBackgroundFromString(word[c].ToString());
					}
				}

				// set flag for "round started"
				m_roundStarted = true;

				Debug.Log("Entering reception loop");
				while (true){
					// wait for every player to send their drawing

					// inner LOOP; check for finished teams
					for (int t = 0; t < numberOfTeams; t++){
						//Debug.Log("t = " + t);
						if (m_wordSpawner.IsTeamReady(t) && !alreadySpawned[t]){
							alreadySpawned[t] = true;
							Debug.Log("Team " + t + " is ready!");

							// check for correct order
							if (m_wordSpawner.IsOrderCorrect(t)){
								// correct order
								Debug.Log("Correct order! :-)");

								if (numberOfTeams > 1){
									// send notification to entire team
									PlayerManager.s_playerManager.BroadcastNotificationToTeam(t, m_notificationWordModeCorrectOrderWaitForOtherTeams, false, 0);
								} else {
									PlayerManager.s_playerManager.BroadcastNotificationToTeam(t, m_notificationWordModeCorrectOrder, false, 0);
								}

								m_nextMode = GameState.playing_free;

							} else {
								// incorrect order
								Debug.Log("Incorrect order! :-(");
								m_wordSpawner.SpawnPowerUpBomb();
								if (numberOfTeams > 1){
									// send notification to entire team
									PlayerManager.s_playerManager.BroadcastNotificationToTeam(t, m_notificationWordModeIncorrectOrderWaitForOtherTeams + m_wordSpawner.GetWordFromList(t), false, 0);
								} else {
									PlayerManager.s_playerManager.BroadcastNotificationToTeam(t, m_notificationWordModeIncorrectOrder + m_wordSpawner.GetWordFromList(t), false, 0);
								}

								m_nextMode = GameState.playing_word;
							}



							// set the time to wait after end of word mode
							// will automatically be the length of the word that was spawned last (in seconds)
							m_waitAfterWordMode = (float) m_wordSpawner.GetWordFromList(t).Length;

							yield return StartCoroutine(m_wordSpawner.SpawnWordsOfTeamCoroutine(t));
						}
					}
					// TODO: check results (correct order? accuracy?)
					if (m_wordSpawner.IsQueueFull()) {
						yield return new WaitForSeconds(m_waitAfterWordMode);
						m_prevRound = GameState.playing_word;
					}
					if (m_wordSpawner.IsQueueFull()) break;
					else yield return new WaitForSeconds(2.0f);
				}
				Debug.Log("Leaving reception loop");

				// clear drawing queue
				m_wordSpawner.EmptyQueue();

				m_prevRound = GameState.playing_word;

			} else if (gamemode == GameState.playing_free){
				// +++ FREE MODE +++

				Debug.Log("Free mode started");

				// pick random colors for all players
				PlayerManager.s_playerManager.AssignRandomColors();

				// notify the players
				PlayerManager.s_playerManager.BroadcastNotificationToCurrentRound(m_notificationFreeMode, true, 0);
			
				// start spawning incoming drawings
				//m_wordSpawner.SpawnDrawingsFromQueue();

				// set the time for free mode based on the number of connected players
				m_freeModeRoundTime = PlayerManager.s_playerManager.GetNumberOfPlayers() * m_secondsPerPlayerInFreeMode;

				// reset the round timer
				m_roundTimer = 0;

				// empty queue
				m_wordSpawner.EmptyQueue();

				// set flag for "round started"
				m_roundStarted = true;

				while (true){

					// increment timer only when there are enough players for word mode
					// otherwise we just stay in free mode
					if (PlayerManager.s_playerManager.GetNumberOfPlayers() >= PlayerManager.s_playerManager.m_minPlayersPerTeam){
						m_roundTimer += 1.0f;

						if (m_roundTimer >= m_freeModeRoundTime){
							// go into word mode
							m_nextMode = GameState.playing_word;
							break;
						}
					}

					yield return new WaitForSeconds(1.0f);
				}
				// stop spawning incoming drawings
				//m_wordSpawner.StopSpawning();
				m_prevRound = GameState.playing_free;
			}

			// remove flag for "round started"
			m_roundStarted = false;

			PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(false);

			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator Screensaver(){
		while (true){
			if (PlayerManager.s_playerManager.GetNumberOfPlayers() <= 0){
				Debug.Log("screensaver is running...");

				m_screensaverIsRunning = true;

				yield return  m_wordSpawner.StartCoroutine(m_wordSpawner.SpawnWordFromStringCoroutine(m_screensaverWords[m_screensaverWordIndex]));

				yield return new WaitForSeconds((float)(m_screensaverWords[m_screensaverWordIndex].Length / 2));

				m_screensaverWordIndex = (m_screensaverWordIndex+1)%m_screensaverWords.Length;
			} else {
				m_screensaverIsRunning = false;

				yield return new WaitForSeconds(1.0f);
			}
		}
	}

	/*
	 * 
	 */
	private void OnPlayerJoin(){
		
		if (PlayerManager.s_playerManager.GetNumberOfPlayers() == 1 && IsPlaying()){
			StartGame();
		}
	}

	public void HandlePlayerJoin(DrawInputPlayer _player){
		if (m_roundStarted && m_currentState == GameState.playing_word && !PlayerManager.s_playerManager.IsPlayerInCurrentRound(_player)){
			_player.SendNotification(m_notificationWordModeWaitForEndOfRound, false, 0);
		} else if (m_roundStarted && m_currentState == GameState.playing_free){
			_player.SendNotification(m_notificationFreeMode, true, 0);
		} else if (m_currentState == GameState.initializing){
			PlayerManager.s_playerManager.BroadcastNotification(m_notificationInitializing, false, 0);
		} else if (m_currentState == GameState.paused){
			PlayerManager.s_playerManager.BroadcastNotification(m_notificationGamePaused, false, 0);
		}
	}

	/*
	 * 
	 */
	private void OnPlayerLeave(){
		if (PlayerManager.s_playerManager.GetNumberOfPlayers() < 1 && IsPlaying()){
			ResetGame();
		}
	}

	// This is called once a word was successfully generated
	public void OnWordSpawned(){
		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(true);
		StartNewRound();
	}

	// TODO: for testing
	public void OnPlayerReceivedDrawing(DrawInputPlayer _player){
		//Debug.Log("OnPlayerReceivedDrawing");
		if (m_roundStarted){
			if (m_currentState == GameState.playing_word && _player.GetCurrentGameState() == GameState.playing_word){
				m_wordSpawner.AddLetterDrawingToQueue(_player.GetCurrentDrawing());
				_player.SendNotification(m_notificationWordModeWaitForRestOfTeam + ((int)(_player.GetCurrentDrawing().accuracy * 100)) + "%!", false, 0);
			} else if (m_currentState == GameState.playing_free && _player.GetCurrentGameState() == GameState.playing_free) {
				m_wordSpawner.AddFreeDrawingToQueue(_player.GetCurrentDrawing());
			}
		}
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

		if (m_score%m_killsForArtstyleChangeEachRound == 0 &&
			(m_screensaverIsRunning || PlayerManager.s_playerManager.GetNumberOfPlayers() < PlayerManager.s_playerManager.m_minPlayersPerTeam)){
			SwitchArtstyle();
		}

		if (m_score >= m_killsForArtstyleChange){
			m_killsMetInLastRound = true;
		}

		if (m_score%m_killsForArtstyleChangeEachRound == m_killsForArtstyleChangeEachRound/2){
			m_wordSpawner.SpawnPowerUpBomb();
		}

		if (m_score > m_highscore){
			m_highscore = m_score;
		}
	}

	public void ResetScore(){
		m_score = 0;
	}

	public void SwitchArtstyle(){
		ArtstyleManager.s_artstyleManager.Switch();
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
		m_scoreText = "CASUALTIES: ";

		while (m_currentArtstyleTimer < m_timeUntilReset){
			m_currentArtstyleTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
			
		ArtstyleManager.s_artstyleManager.SetArtstyle(ArtstyleManager.Style.arcade);
		m_scoreText = "SCORE:      ";

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
				if (GUI.Button(new Rect(10, 50, 120, 20), "ARCADE MODE")){
					m_artstyleDefaultMode = false;
					ArtstyleManager.s_artstyleManager.SetArtstyle(ArtstyleManager.Style.arcade);
				}
				if (GUI.Button(new Rect(140, 50, 120, 20), "REALISTIC MODE")){
					m_artstyleDefaultMode = false;
					ArtstyleManager.s_artstyleManager.SetArtstyle(ArtstyleManager.Style.realistic);
				}
				if (GUI.Button(new Rect(270, 50, 120, 20), "DEFAULT MODE")){
					m_artstyleDefaultMode = true;
					ArtstyleManager.s_artstyleManager.SetArtstyle(ArtstyleManager.Style.arcade);
				}
			}
		}

		if (m_currentState == GameState.initializing && m_pixelPoolReady){
			if (GUI.Button(new Rect(Screen.width/2 - m_menuButtonWidth/2, Screen.height*2/3 + m_menuButtonSpacing , m_menuButtonWidth, m_menuButtonHeight * 3), "START GAME")){
				StartGame();
			}
		} else if (m_currentState == GameState.initializing && !m_pixelPoolReady){
			GUI.Label(new Rect(Screen.width/2 - m_menuButtonWidth/2, Screen.height*2/3 + m_menuButtonSpacing , m_menuButtonWidth, m_menuButtonHeight * 3), "LOADING...");
		}

		if (m_currentState == GameState.paused){

			// Menu
			GUILayout.BeginVertical();

			if (GUI.Button(new Rect(Screen.width/2 - m_menuButtonWidth/2, Screen.height*2/3 + m_menuButtonSpacing * 1, m_menuButtonWidth, m_menuButtonHeight), "RESUME GAME")){
				ChangeGameState(m_previousState);
				PlayerManager.s_playerManager.BroadcastNotification(m_notificationGameResumed, true, 0);
			}

			if (GUI.Button(new Rect(Screen.width/2 - m_menuButtonWidth/2, Screen.height*2/3 + m_menuButtonSpacing * 2, m_menuButtonWidth, m_menuButtonHeight), "RESTART GAME")){
				StartGame();
			}

			if (GUI.Button(new Rect(Screen.width/2 - m_menuButtonWidth/2, Screen.height*2/3 + m_menuButtonSpacing * 3, m_menuButtonWidth, m_menuButtonHeight), "OPEN QUESTIONAIRE")){
				Application.OpenURL("https://docs.google.com/forms/d/1DUZs3-u70XKN4UzRwLdavks2_PsPnbGpu0_gVTBcvS8/viewform");
			}

			if (GUI.Button(new Rect(Screen.width/2 - m_menuButtonWidth/2, Screen.height*2/3 + m_menuButtonSpacing * 4 , m_menuButtonWidth, m_menuButtonHeight), "QUIT GAME")){
				Application.Quit();
			}

			GUILayout.EndVertical();
		}

		if (IsPlaying()){
			if (m_score >= m_highscore){
				GUI.color = Color.red;

			} else {
				GUI.color = Color.yellow;
			}
			if (m_drone.IsInAutopilot()){
				GUI.color = Color.blue;
				GUI.Label(new Rect(20, 10, 300, 30), "AUTOPILOT");
			} else GUI.Label(new Rect(20, 10, 300, 30), m_scoreText + m_score);

			GUI.color = Color.white;

			GUI.Label(new Rect(20, 30, 300, 30), 		"BOMBS:      " + m_drone.m_bombCount);

			if (m_currentState == GameState.playing_free && m_roundStarted && m_roundTimer > 0 &&
				PlayerManager.s_playerManager.GetNumberOfPlayers() >= PlayerManager.s_playerManager.m_minPlayersPerTeam){
				GUI.Label(new Rect(20, Screen.height - 40, 300, 20), "TIME LEFT IN FREE MODE: " + (m_freeModeRoundTime - m_roundTimer));
			}
		}
	}
}
