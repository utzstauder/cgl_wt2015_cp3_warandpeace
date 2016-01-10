using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager s_gameManager; 	// Static reference to this instance

	private WordSpawner m_wordSpawner;

	// Use this for initialization
	void Awake () {
		if (s_gameManager != null){
			Debug.LogError("There is more than one GameManager in the scene");
			Destroy(this);
		} else{
			s_gameManager = this;
		}

	}

	void Start(){
		m_wordSpawner = GameObject.FindGameObjectsWithTag("WordSpawner")[0].GetComponent<WordSpawner>();
	}
	
	// Update is called once per frame
	void Update () {


		// Debug safety; see what I did there? ;-)
		if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.T)){
			ResetGame();
		}
	}

	// TODO: implement
	private void ResetGame(){
		StopAllCoroutines();
	}

	// TODO: for testing
	public void OnPlayerReceivedDrawing(DrawInputPlayer _player){
		int[] playerIds = new int[1];
		playerIds[0] = PlayerManager.s_playerManager.GetPlayerIndex(_player);
		m_wordSpawner.SpawnWord(playerIds);
	}

	void OnGUI(){
		// This is for debugging only
		if (m_wordSpawner != null){
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
	}
}
