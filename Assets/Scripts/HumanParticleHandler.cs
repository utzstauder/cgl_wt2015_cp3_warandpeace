using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimesExample;

public class HumanParticleHandler : MonoBehaviour {

	public static HumanParticleHandler current;

	public HumanParticle particlePrefab;

	private List<HumanParticle> listOfParticles;

	// Use this for initialization
	void Awake(){
		if (!current){
			current = this;
		} else throw new System.InvalidProgramException("There is more than one HumanParticleHandler class");

		listOfParticles = new List<HumanParticle>();
	}

	void Start () {
		//TODO: this is only for debugging
		//SpawnParticles(10);
		//SpreadParticles(TouchGameSettings.settings().areaWidth, TouchGameSettings.settings().areaHeight);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Reassign all particles to new/old players
	public void ReassignParticles(){
		if (GameManager.current.GetNumberOfPlayers() > 0){
			int i = 0;
			foreach (HumanParticle particle in listOfParticles){
				particle.AssignPlayer(i++%GameManager.current.GetNumberOfPlayers());
			}
		}
	}


	// Spawn new particles
	public void SpawnParticles(int amount){
		for (int i = 0; i < amount; i++){
			HumanParticle tmp = Instantiate(particlePrefab, this.transform.position, Quaternion.identity) as HumanParticle;
			listOfParticles.Add(tmp);
		}
		ReassignParticles();
	}

	// Removes a particle once it has been destroyed
	public void RemoveParticle(HumanParticle target){
		listOfParticles.RemoveAt(listOfParticles.IndexOf(target));
		ReassignParticles();
	}

	public void ClearAllParticles(){
		foreach (HumanParticle particle in listOfParticles){
			Destroy(particle.gameObject);
		}
		//listOfParticles.Clear();
	}

	// Spread all particles to random destinations
	public void SpreadParticles(int width, int height){
		foreach (HumanParticle particle in listOfParticles){
			particle.SetDestinationRandom(width, height);
			particle.Move();
		}
	}

	// Set particles on a player drawn path
	public void SetParticlesOnPlayerPath(int playerId, List<Vector3> listOfWaypoints){
		List<HumanParticle> playerParticles = new List<HumanParticle>();

		// add corresponding particles to temporary list
		foreach (HumanParticle particle in listOfParticles){
			if (particle.GetPlayerId() == playerId){
				playerParticles.Add(particle);
			}
		}

		Debug.Log("particles = " + playerParticles.Count);
		Debug.Log("waypoints = " + listOfWaypoints.Count);

		// check if there are enough particles
		if (playerParticles.Count >= listOfWaypoints.Count){
			
			// determine particle-waypoint-ratio
			int ratio = playerParticles.Count / listOfWaypoints.Count;

			int rest = playerParticles.Count % listOfWaypoints.Count;

			Debug.Log(ratio + " | " + rest);

			float amount = 0;

			Debug.Log("particle("+ playerParticles.Count +")-waypoint("+ listOfWaypoints.Count +")-ratio = " + ratio);

			for (int i = 1; i < listOfWaypoints.Count; i++){
				for (int j = (i-1)*ratio; j < ratio*(i+1); j++){
					Debug.Log(i + " | " + j);
					if (j < playerParticles.Count){
						amount = (float)j/(float)ratio - i + 1;
						//Debug.Log (amount);
						playerParticles[j].SetDestination(Vector3.Lerp(listOfWaypoints[i-1], listOfWaypoints[i], amount));
						playerParticles[j].Move();
					}
				}
			}

			// take care of the rest
			// TODO: very uncool; 
			for (int i = playerParticles.Count - rest; i < playerParticles.Count; i++){
				int randomInt = Random.Range(1, listOfWaypoints.Count-1);
				float randomFloat = Random.Range(0f, 1f);
				playerParticles[i].SetDestination(Vector3.Lerp(listOfWaypoints[randomInt-1], listOfWaypoints[randomInt], randomFloat));
				playerParticles[i].Move();
			}
				
		}
		else {
			// if not, distribute evenly; skip waypoints
			// TODO: implement
			//Debug.Log("Not enough particles to display the path. Sorry, not implemented yet.");
			Debug.Log("Particles P" + playerId + ": " + playerParticles.Count + " < " + "Waypoints: " + listOfWaypoints.Count); 

			int skip = listOfWaypoints.Count/playerParticles.Count;
			Debug.Log("Only placing one particle on every other " + skip + " waypoint");

			for (int i = 0; i < playerParticles.Count; i++){
				playerParticles[i].SetDestination(listOfWaypoints[i*skip]);
				playerParticles[i].Move();
			}
		}


	}

	void OnGUI(){
		if (GUI.Button(new Rect(300, 10, 100, 20), "more particles")){
			SpawnParticles(10);
		} 
		if (GUI.Button(new Rect(300, 30, 100, 20), "randomize")){
			SpreadParticles(TouchGameSettings.settings().areaWidth, TouchGameSettings.settings().areaHeight);
		}
		if (GUI.Button(new Rect(300, 50, 100, 20), "clear all")){
			ClearAllParticles();
		}
	}
}
