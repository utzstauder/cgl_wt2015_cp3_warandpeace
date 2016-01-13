using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimes;

/*
 * This class manages player connections and basically every communication between players and the game
 */

public class PlayerManager : MonoBehaviour {

	public static PlayerManager s_playerManager;	// Static reference to this instance

	private List<DrawInputPlayer> m_players;		// A generic list of all the players that are currently connected to the game
	private int m_nPlayers = 0;						// The number of players currently connected to the game
	private int m_nPlayersCurrentRound = 0;

	public delegate void PlayerCountChange();		// Broadcasts a message when the number of players changes; mainly for fallback purposes	
	public static event PlayerCountChange OnPlayerJoin;
	public static event PlayerCountChange OnPlayerLeave;

	// Use this for initialization
	void Awake () {
		if (s_playerManager != null){
			Debug.LogError("There is more than one PlayerManager in the scene");
			Destroy(this);
		} else{
			s_playerManager = this;
		}

		m_players = new List<DrawInputPlayer>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// This is called by every DrawInputPlayer class upon connecting to the game
	public void AddPlayer(DrawInputPlayer _player){
		m_players.Add(_player);
		UpdateNumberOfPlayers();
	}

	// This is called by every DrawInputPlayer upon disconnecting from the
	public void RemovePlayer(DrawInputPlayer _player){
		m_players.RemoveAt(m_players.IndexOf(_player));

		UpdateNumberOfPlayers();

		// TODO: implement some kind of fallback for player disconnect during a session
	}

	//
	public int GetPlayerIndex(DrawInputPlayer _player){
		return m_players.IndexOf(_player);
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

	public void SetNumberOfPlayersInCurrentRound(int _nPlayers){
		m_nPlayersCurrentRound = _nPlayers;
	}

}
