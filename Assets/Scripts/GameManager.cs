using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimes;
using HappyFunTimesExample;

public class GameManager : MonoBehaviour {

	public static GameManager current;

	private List<DronesTouchPlayer> listOfPlayers;

	// Use this for initialization
	void Awake () {
		if (!current){
			current = this;
		} else throw new System.InvalidProgramException("There is more than one GameManager class");

		listOfPlayers = new List<DronesTouchPlayer>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AddPlayer(DronesTouchPlayer player){
		listOfPlayers.Add(player);
		HumanParticleHandler.current.ReassignParticles();
	}

	public void RemovePlayer(DronesTouchPlayer player){
		listOfPlayers.RemoveAt(listOfPlayers.IndexOf(player));
		HumanParticleHandler.current.ReassignParticles();
	}

	public int GetNumberOfPlayers(){
		return listOfPlayers.Count;
	}

	public int GetPlayerIndex(DronesTouchPlayer player){
		return listOfPlayers.IndexOf(player);
	}

	public Color GetPlayerColor(int index){
		return listOfPlayers[index].GetColor();
	}

	void OnGUI(){
		GUI.Label(new Rect(10f, 10f, 200f, 20f), "Number of Players: " + GetNumberOfPlayers());
		for (int i = 0; i < listOfPlayers.Count; i++){
			if (listOfPlayers[i]) GUI.Label(new Rect(10f, 30f + i*20f, 200f, 50f), listOfPlayers[i].gameObject.name);
			else GUI.Label(new Rect(10f, 30f + i*20f, 200f, 50f), "disconnected");
		}
	}
}
