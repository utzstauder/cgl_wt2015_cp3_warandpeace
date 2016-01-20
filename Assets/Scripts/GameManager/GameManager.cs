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
	private float m_timeUntilReset = 0.0f;
	private float m_currentArtstyleTimer = 0.0f;

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
			SendRandomLetters();
		}
	}

	// TODO: implement
	private void ResetGame(){
		StopAllCoroutines();
	}

	private void OnPlayerJoin(){
		// if there is at least one player and no round is being played start one
		if (PlayerManager.s_playerManager.GetNumberOfPlayers() > 0 &&
			PlayerManager.s_playerManager.GetNumberOfPlayersInCurrentRound() <= 0 &&
			m_currentState == GameState.playing_word){
			SendRandomLetters();
		}
		// TODO: send wait message
	}

	private void OnPlayerLeave(){
		if (m_currentState == GameState.playing_word){
			// Spawn words that are already in queue
			PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(PlayerManager.s_playerManager.GetNumberOfPlayersInCurrentRound() - 1);

			// Check if queue full
			if (m_wordSpawner.IsQueueFull()){
				m_wordSpawner.SpawnLettersFromQueue();
			}
		}
	}

	// This is called once a word was successfully generated
	public void OnWordSpawned(){
		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(0);
		SendRandomLetters();
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

	// Start a round with random letters
	public void SendRandomLetters(){
		int currentNumberOfPlayers = PlayerManager.s_playerManager.GetNumberOfPlayers();
		PlayerManager.s_playerManager.SetNumberOfPlayersInCurrentRound(currentNumberOfPlayers);

		int[] randomLetters = WordManager.s_wordManager.GetRandomLetters(currentNumberOfPlayers);

		for (int i = 0; i < PlayerManager.s_playerManager.GetNumberOfPlayers(); i++){
			PlayerManager.s_playerManager.GetPlayerReference(i).DrawLetterOnBackground(randomLetters[i]);
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
			m_timeUntilReset += _time;

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

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 -20, 100, 20), "WORD MODE")){
				ChangeGameState(GameState.playing_word);
			}

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 20, 100, 20), "FREE MODE")){
				ChangeGameState(GameState.playing_free);
			}

			if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 60, 100, 20), "QUIT GAME")){
				Application.Quit();
			}

			GUILayout.EndVertical();
		}

		GUI.Label(new Rect(20, 10, 300, 30), "SCORE: " + m_score);
	}
}
