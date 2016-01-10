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

	public delegate void PlayerCountChange();		// Broadcasts a message when the number of players changes; mainly for fallback purposes	
	public static event PlayerCountChange OnPlayerCountChange;

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

		// TODO: implement some kind of fallback for player disconnect during a session

		UpdateNumberOfPlayers();
	}

	//
	public int GetPlayerIndex(DrawInputPlayer _player){
		return m_players.IndexOf(_player);
	}

	//
	public DrawInputPlayer GetPlayerReference(int _playerId){
		return m_players[_playerId];
	}

	// Helper method
	private void UpdateNumberOfPlayers(){
		m_nPlayers = m_players.Count;

		// Broadcast message to all subscribers
		if (OnPlayerCountChange != null){
			OnPlayerCountChange();
		}
	}

	// Returns the number of players
	public int GetNumberOfPlayers(){
		return m_nPlayers;
	}

}
