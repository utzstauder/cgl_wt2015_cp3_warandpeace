using UnityEngine;
using System.Collections;

public class HumanParticle : MonoBehaviour {

	enum HumanParticleBehaviour {idle, moving};
	[SerializeField]
	private HumanParticleBehaviour currentState;

	private Renderer renderer;

	public float speed = 10f;
	private int playerId = -1;

	[SerializeField]
	private Vector3 targetPosition;
	private float minDistance = .5f;

	// Use this for initialization
	void Awake () {
		renderer = GetComponent<Renderer>();

		currentState = HumanParticleBehaviour.idle;
	}
	
	// Update is called once per frame
	void Update () {
		
		switch(currentState){
		case HumanParticleBehaviour.idle:
			break;

		case HumanParticleBehaviour.moving:
			if (Vector3.Distance(this.transform.position, targetPosition) <= minDistance){
				currentState = HumanParticleBehaviour.idle;
			} else{
				this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, Time.deltaTime * speed);
			}
			break;

		default:
			break;
		}

	}
		
	// Assign to new player when the number of players changes
	public void AssignPlayer(int newPlayerId){
		playerId = newPlayerId;
		renderer.material.color = GameManager.current.GetPlayerColor(newPlayerId);
	}

	// Get the player this particle is assigned to
	public int GetPlayerId(){
		return playerId;
	}

	// Set a new destination
	public void SetDestination(Vector3 newTarget){
		targetPosition = newTarget;
	}

	// Set a random destination
	public void SetDestinationRandom(int width, int height){
		targetPosition = new Vector3(Random.Range(-width/2, width/2), 0, Random.Range(0, height));
	}

	// Start movement
	public void Move(){
		currentState = HumanParticleBehaviour.moving;
	}

	// Calls the handler to remove this particle from the list
	public void OnDestroy(){
		HumanParticleHandler.current.RemoveParticle(this);
	}
}
