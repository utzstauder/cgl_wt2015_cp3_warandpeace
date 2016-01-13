using UnityEngine;
using System.Collections;

public class Letter : MonoBehaviour {

	public float deathX = -400;

	public bool wiggleInChildren = false;
	private bool prevWiggleInChildren;

	private Word parent;

	// Use this for initialization
	void Awake () {
		prevWiggleInChildren = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (wiggleInChildren != prevWiggleInChildren){
			OnWiggleInChildrenChange(wiggleInChildren);
		}

		prevWiggleInChildren = wiggleInChildren;

		if (transform.childCount <= 0) Destroy(this.gameObject);

		if (transform.position.x < deathX) Destroy(this.gameObject);
	}

	void OnWiggleInChildrenChange(bool param){
		Wiggle[] wiggleScripts = transform.GetComponentsInChildren<Wiggle>();

		foreach (Wiggle wiggle in wiggleScripts){
			wiggle.wiggle = param;
		}
	}

	public void SetParent(Word _parent){
		parent = _parent;
		transform.parent = _parent.transform;
	}

	public void CheckChildCount(){
		if (transform.childCount <= 0) Destroy(this.gameObject);
	}

	void OnDestroy(){
		if (parent) parent.CheckChildCount();
	}
}
