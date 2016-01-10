using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimes;
using CSSParse;

/*
 * TODO: create subclass or struct for DRAWING containing all the information needed
 */

public class DrawInputPlayer : MonoBehaviour {

	public bool m_debug = false;

	// This players index; default is -1
	private int m_playerIndex = -1;

	private List<int[]> listOfDrawings;
	private List<float> listOfAccuracies;

	private HFTGamepad m_gamepad;
	private HFTInput m_hftInput;
	private UnityEngine.UI.Text m_text;
	private Color m_color;
	private string m_name;

	public Letter letterPrefab;
	public LetterPixel letterPixelPrefab;
	public float letterPixelScale = 7.0f;
	public float letterScale = .5f;

	void Awake(){
		// Add this player to the list of players
		PlayerManager.s_playerManager.AddPlayer(this);

		listOfDrawings = new List<int[]>();
		listOfAccuracies = new List<float>();
	}

	void Start () {
		m_gamepad  = GetComponent<HFTGamepad>();
		m_color = m_gamepad.Color;

		m_gamepad.OnReceiveDrawing += OnDrawingReceived;
	}

	void OnDestroy(){
		// Remove this player of the list of players
		PlayerManager.s_playerManager.RemovePlayer(this);

		m_gamepad.OnReceiveDrawing -= OnDrawingReceived;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)){
			//DrawTextureOnCube();
			//SpawnPixelsFromArray(2);
		}
	}

	public int GetNumberOfDrawings(){
		return listOfDrawings.Count;
	}

	public int[] GetDrawing(){
		return m_gamepad.drawArray;
	}

	public Vector2 GetDrawingDimension(){
		return new Vector2(m_gamepad.drawArrayWidth, m_gamepad.drawArrayHeight);
	}

	public int GetDrawingDivision(){
		return m_gamepad.drawOptions.drawArrayDivision;
	}

	public Color GetPlayerColor(){
		return m_color;
	}

	private void OnDrawingReceived(){
		PushDrawing(m_gamepad.drawArray);
		PushAccuracy(m_gamepad.drawAccuracy);

		GameManager.s_gameManager.OnPlayerReceivedDrawing(this);
	}

	private void PushAccuracy(float _accuracy){
		listOfAccuracies.Add(_accuracy);
	}

	public float PopAccuracy(){
		float returnAccuracy = -1.0f;

		if (listOfAccuracies.Count > 0){
			returnAccuracy = listOfAccuracies[0];
			listOfAccuracies.RemoveAt(0);
		}

		return returnAccuracy;
	}

	private void PushDrawing(int[] _drawing){
		listOfDrawings.Add(_drawing);
	}

	public int[] PopDrawing(){
		int[] returnDrawing = null;

		if (listOfDrawings.Count > 0){
			returnDrawing = listOfDrawings[0];
			listOfDrawings.RemoveAt(0);
		}

		return returnDrawing;
	}

	// +++++++++++++++++++++++++++++++++++
	// TODO: everything below should be implemented somewhere else


	void PullDrawing(){
		m_gamepad.NetPlayer.SendCmd("pullDrawing");
	}

	// This is "proof-of-concept" only
	// TODO: 
	void DrawTextureOnCube(){
		SpriteRenderer spriteRenderer = GameObject.Find("DrawTarget").GetComponent<SpriteRenderer>();

		// Create texture from array
		// TODO: implement
		int width = m_gamepad.drawArrayWidth;
		int height= m_gamepad.drawArrayHeight;
		Color color;

		//Debug.Log(m_gamepad.drawArray.Length);

		Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
		for (int y = 0; y < height; y++){
			for (int x = 0; x < width; x++){
				color = m_gamepad.drawArray[width*y + x] > 0 ? Color.black : Color.clear;
				//color = Color.black;
				//color.a = m_gamepad.drawArray[width*y + x];
				texture.SetPixel(x, height-y, color);
			}
		}

		// Write drawing to png file
		byte[] bytes = texture.EncodeToPNG();
		File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

		texture.alphaIsTransparency = true;

		Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100.0f);
		sprite.name = "drawnSprite";

		spriteRenderer.sprite = sprite;
	}

	void SpawnPixelsFromArray(){
		// Debug only
		DeleteAllLettersFromScene();

		int width = m_gamepad.drawArrayWidth;
		int height= m_gamepad.drawArrayHeight;
		int gap = m_gamepad.drawOptions.drawArrayDivision;

		Vector3 position;

		letterPixelPrefab.transform.localScale = Vector3.one * letterPixelScale * gap * letterScale;

		Letter letter = (Letter)Instantiate(letterPrefab, transform.position, Quaternion.identity);

		for (int y = 0; y < height; y++){
			for (int x = 0; x < width; x++){
				if (m_gamepad.drawArray[width*y + x] > 0){
					position = new Vector3((x * gap) - (width / 2 * gap), (height / 2 * gap) - (y * gap), 0) * letterScale;
					LetterPixel pixel = (LetterPixel)Instantiate(letterPixelPrefab, position, Quaternion.identity);
					pixel.transform.SetParent(letter.transform);
					pixel.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.black, m_color, m_gamepad.drawAccuracy);
				}
			}
		}
			
		//letter.transform.localScale = Vector3.one * letterScale;
	}

	void DeletePixelsFromScene(){
		GameObject[] pixels = GameObject.FindGameObjectsWithTag("LetterPixel");
		foreach (GameObject pixel in pixels){
			Destroy(pixel.gameObject);
		}
	}
		
	void DeleteAllLettersFromScene(){
		GameObject[] letters = GameObject.FindGameObjectsWithTag("Letter");
		foreach (GameObject letter in letters){
			Destroy(letter.gameObject);
		}
	}

	void SaveDrawingToFile(){
		int width = m_gamepad.drawArrayWidth;
		int height= m_gamepad.drawArrayHeight;
		Color color;

		Debug.Log(m_gamepad.drawArray.Length);

		Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
		for (int y = 0; y < height; y++){
			for (int x = 0; x < width; x++){
				color = m_gamepad.drawArray[width*y + x] > 0 ? Color.black : Color.clear;
				//color = Color.black;
				//color.a = m_gamepad.drawArray[width*y + x];
				texture.SetPixel(x, height-y, color);
			}
		}

		// Write drawing to png file
		byte[] bytes = texture.EncodeToPNG();
		File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);
	}

	// TODO: bad implementation / doesn't belong here / or does it?
	public void DrawLetterOnBackground(int letter){
		m_gamepad.SendLetter(letter);
	}

	void OnGUI(){
		if (m_debug){
			if (GUI.Button(new Rect(10, 10, 120, 20), "Send letter")){
				int randomLetter = UnityEngine.Random.Range(0, AlphabetManager.g_letters.Length-1);
				DrawLetterOnBackground(randomLetter);
			}
			if (GUI.Button(new Rect(10, 30, 120, 20), "Pull drawing")){
				PullDrawing();
			}
			if (m_gamepad.drawArray != null){
				if (GUI.Button(new Rect(10, 50, 120, 20), "Spawn pixels")){
					SpawnPixelsFromArray();
				}
				if (GUI.Button(new Rect(10, 70, 120, 20), "Delete letters")){
					DeleteAllLettersFromScene();
				}
				if (GUI.Button(new Rect(10, 90, 120, 20), "Save to file")){
					SaveDrawingToFile();
				}
			}
		}
	}
		
}
