﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimes;

/*
 * This class manages player connections and basically every communication between players and the game
 */

public class PlayerManager : MonoBehaviour {

	public static PlayerManager s_playerManager;	// Static reference to this instance

	public int m_minPlayersPerTeam = 3;
	[Range(3, 8)]
	public int m_maxPlayersPerTeam = 4; 

	private List<DrawInputPlayer> m_players;		// A generic list of all the players that are currently connected to the game
	private int m_nPlayers = 0;						// The number of players currently connected to the game
	private int m_nPlayersCurrentRound = 0;

	private List<int> m_playersInTeamAtBeginningOfRound;

	public delegate void PlayerCountChange();		// Broadcasts a message when the number of players changes; mainly for fallback purposes	
	public static event PlayerCountChange OnPlayerJoin;
	public static event PlayerCountChange OnPlayerLeave;

	private int m_numberOfTeams = 0;


	// References
	private WordSpawner m_wordSpawner;

	// Use this for initialization
	void Awake () {
		if (s_playerManager != null){
			Debug.LogError("There is more than one PlayerManager in the scene");
			Destroy(this);
		} else{
			s_playerManager = this;
		}

		m_players = new List<DrawInputPlayer>();
		m_playersInTeamAtBeginningOfRound = new List<int>();

	}
	
	void Start () {
		m_wordSpawner = GameObject.FindGameObjectsWithTag("WordSpawner")[0].GetComponent<WordSpawner>();
	}

	// This is called by every DrawInputPlayer class upon connecting to the game
	public void AddPlayer(DrawInputPlayer _player){
		m_players.Add(_player);

		GameManager.s_gameManager.HandlePlayerJoin(_player);

		UpdateNumberOfPlayers();
	}

	// This is called by every DrawInputPlayer upon disconnecting from the
	public void RemovePlayer(DrawInputPlayer _player){
		if (_player.IsPlayingInCurrentRound()){
			
			// fallback only needed while in word mode and when the player did not submit a drawing yet
			if (GameManager.s_gameManager.m_currentState == GameManager.GameState.playing_word &&
				!m_wordSpawner.IsPlayerDrawingInQueue(_player)){
				// TODO: add a substitute drawing to the queue
				Drawing drawing = AlphabetManager.LetterToDrawing(AlphabetManager.CharToInt(_player.GetCurrentLetter().ToCharArray()[0]), 2);
				drawing.teamId = _player.GetTeamId();
				m_wordSpawner.AddFreeDrawingToQueue(drawing);
			}

			m_nPlayersCurrentRound--;
		}

		m_players.RemoveAt(m_players.IndexOf(_player));

		UpdateNumberOfPlayers();

		// TODO: implement some kind of fallback for player disconnect during a session
	}

	//
	public int GetPlayerIndex(DrawInputPlayer _player){
		return m_players.IndexOf(_player);
	}
		
	/*
	 * Tells every participating player which game mode is currently beeing played
	 */
	public void BroadcastCurrentGameMode(GameManager.GameState _currentGameMode){
		List<DrawInputPlayer> playersInCurrentRound = GetPlayersInCurrentRound();

		foreach (DrawInputPlayer player in playersInCurrentRound){
			player.SetGameState(GameManager.s_gameManager.GetGameStateAsString(_currentGameMode));
		}
	}

	/*
	 * Broadcasts a notification to every participating player
	 */
	public void BroadcastNotificationToCurrentRound(string _message, bool _dismissable, float _timeout){
		List<DrawInputPlayer> playersInCurrentRound = GetPlayersInCurrentRound();

		foreach (DrawInputPlayer player in playersInCurrentRound){
			player.SendNotification(_message, _dismissable, _timeout);
		}
	}

	/*
	 * Broadcasts a notification to every member of team _teamId
	 */
	public void BroadcastNotificationToTeam(int _teamId, string _message, bool _dismissable, float _timeout){
		List <DrawInputPlayer> playersInTeam = PlayerManager.s_playerManager.GetPlayersOfTeam(_teamId);

		foreach (DrawInputPlayer player in playersInTeam){
			player.SendNotification(_message, _dismissable, _timeout);
		}
	}

	/*
	 * Broadcasts a notification to every player
	 */
	public void BroadcastNotification(string _message, bool _dismissable, float _timeout){
		foreach (DrawInputPlayer player in m_players){
			player.SendNotification(_message, _dismissable, _timeout);
		}
	}

	/*
	 * Divides all players up into x teams
	 * depending on the number of currently connected players
	 */
	public void AssignPlayersToTeams(){
		int numberOfTeams = ((m_nPlayersCurrentRound - 1) / m_maxPlayersPerTeam) + 1;
		int teamCounter = 0%numberOfTeams + 1;

		List<DrawInputPlayer> shuffledPlayerList = m_players;
		shuffledPlayerList.Shuffle();

		foreach (DrawInputPlayer player in shuffledPlayerList){
			player.SetTeamId(teamCounter);
			player.SetPlayerColor(GetTeamColor(teamCounter));
			teamCounter = teamCounter%numberOfTeams + 1;
		}

		m_numberOfTeams = numberOfTeams;

		// check if there are enough players in each team
		for (int i = 1; i <= numberOfTeams; i++){
			List<DrawInputPlayer> playersInTeam = GetPlayersOfTeam(i);
			if (playersInTeam.Count < m_minPlayersPerTeam){
				m_maxPlayersPerTeam++;
				AssignPlayersToTeams();
				i = numberOfTeams+1;
			}
		}

		// fill list
		m_playersInTeamAtBeginningOfRound.Clear();
		for (int i = 1; i <= numberOfTeams; i++){
			m_playersInTeamAtBeginningOfRound.Add(GetPlayersOfTeam(i).Count);
		}
	}

	public int GetNumberOfTeams(){
		return m_numberOfTeams;
	}

	public void AssignRandomColors(){
		foreach (DrawInputPlayer player in m_players){
			player.SetRandomColor();
		}
	}

	/*
	 * Returns a list of DrawInputPlayer references of team _teamId
	 */
	public List<DrawInputPlayer> GetPlayersOfTeam(int _teamId){
		List<DrawInputPlayer> playersInTeam = new List<DrawInputPlayer>();

		foreach (DrawInputPlayer player in m_players){
			if (player.GetTeamId() == _teamId){
				playersInTeam.Add(player);
			}
		}

		return playersInTeam;
	}

	public int GetNumberOfPlayersInTeamAtBeginningOfRound(int _teamId){
		return m_playersInTeamAtBeginningOfRound[_teamId-1];
	}

	/*
	 * Assigns all players to team 0; free mode
	 */
	public void RemoveTeamReferences(){
		foreach (DrawInputPlayer player in m_players){
			player.SetTeamId(0);
		}
	}

	public Color GetTeamColor(int _teamId){
		switch (_teamId){
			case 1: return Color.red;
			case 2: return Color.blue;
			case 3: return Color.yellow;
			case 4: return Color.cyan;
			case 5: return Color.green;
			case 6: return Color.magenta;
			case 7: return Color.gray;
		default: break;
		}

		return Color.white;
	}

	//
	public DrawInputPlayer GetPlayerReference(int _playerId){
		return m_players[_playerId];
	}

	public DrawInputPlayer GetPlayerReference(string _sessionId){
		for (int i = 0; i < m_players.Count; i++){
			if (m_players[i].GetSessionId() == _sessionId){
				return m_players[i];
			}
		}
		return null;
	}

	// Helper method
	private void UpdateNumberOfPlayers(){
		int prevNumberOfPlayers = m_nPlayers;
		m_nPlayers = m_players.Count;

		// Broadcast message to all subscribers
		if (prevNumberOfPlayers < m_nPlayers){
			if (OnPlayerJoin != null){
				OnPlayerJoin();
			}
		} else if (prevNumberOfPlayers > m_nPlayers){
			if (OnPlayerLeave != null){
				OnPlayerLeave();
			}
		}
	}

	// Returns the number of players
	public int GetNumberOfPlayers(){
		return m_nPlayers;
	}
		
	public int GetNumberOfPlayersInCurrentRound(){
		return m_nPlayersCurrentRound;
	}

	public bool IsPlayerInCurrentRound(DrawInputPlayer _player){
		List<DrawInputPlayer> playersInCurrentRound = GetPlayersInCurrentRound();

		if (playersInCurrentRound.Contains(_player)) return true;

		return false;
	}

	public int GetNumberOfPlayerAtBeginningOfCurrentRound(){
		int n = 0;

		foreach (int i in m_playersInTeamAtBeginningOfRound){
			n += i;
		}

		return n;
	}

	/*public void SetNumberOfPlayersInCurrentRound(int _nPlayers){
		m_nPlayersCurrentRound = _nPlayers;
	}*/

	/*
	 * Sets the number of players in the current round and the corresponding flags
	 * in the DrawInputPlayer references
	 */
	public void SetNumberOfPlayersInCurrentRound(bool _value){
		m_nPlayersCurrentRound = m_nPlayers;

		foreach (DrawInputPlayer player in m_players){
			player.SetPlayingInCurrentRound(_value);
		}
	}


	/*
	 * Returns a list of DrawInputPlayer references that are playing in the current round
	 */
	public List<DrawInputPlayer> GetPlayersInCurrentRound(){
		List<DrawInputPlayer> listOfPlayersInCurrentRound = new List<DrawInputPlayer>();

		foreach (DrawInputPlayer player in m_players){
			if (player.IsPlayingInCurrentRound()){
				listOfPlayersInCurrentRound.Add(player);
			}
		}

		return listOfPlayersInCurrentRound;
	}

}
