using UnityEngine;
using System.Collections;

public class Letter : MonoBehaviour {

	public bool wiggleInChildren = false;
	private bool prevWiggleInChildren;

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
	}

	void OnWiggleInChildrenChange(bool param){
		Wiggle[] wiggleScripts = transform.GetComponentsInChildren<Wiggle>();

		foreach (Wiggle wiggle in wiggleScripts){
			wiggle.wiggle = param;
		}
	}
}
