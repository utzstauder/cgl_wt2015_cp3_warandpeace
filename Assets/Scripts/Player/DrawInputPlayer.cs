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

	private List<int[]> listOfDrawings;
	private List<float> listOfAccuracies;

	private HFTGamepad m_gamepad;
	private HFTSoundPlayer m_soundPlayer;
	private HFTInput m_hftInput;
	private UnityEngine.UI.Text m_text;
	private Color m_color;
	private string m_name;

	private int m_teamId = 0;						// 0 = no team; free mode
	private bool m_playingInCurrentRound = false;

	private string m_currentLetter = "";

	public Letter letterPrefab;
	public LetterPixel letterPixelPrefab;
	//public float letterPixelScale = 7.0f;
	public float letterScale = .5f;

	void Awake(){
		listOfDrawings = new List<int[]>();
		listOfAccuracies = new List<float>();

		m_gamepad  = GetComponent<HFTGamepad>();
		m_soundPlayer = GetComponent<HFTSoundPlayer>();
	}

	void Start () {
		m_color = m_gamepad.Color;

		m_gamepad.OnReceiveDrawing += OnDrawingReceived;
		m_gamepad.OnNameChange += ChangeName;

		SetName(m_gamepad.Name);
		SetGameState(GameManager.s_gameManager.GetGameStateAsString(GameManager.s_gameManager.m_currentState));

		// Add this player to the list of players
		PlayerManager.s_playerManager.AddPlayer(this);
	}

	void OnDestroy(){
		// Remove this player of the list of players
		PlayerManager.s_playerManager.RemovePlayer(this);

		m_gamepad.OnReceiveDrawing -= OnDrawingReceived;
		m_gamepad.OnNameChange -= ChangeName;

		Debug.Log(m_name + " disconnected!");
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void SetGameState(string _gameState){
		m_gamepad.drawOptions.gameState = _gameState;
	}

	private void SetName(string _name){
		m_name = _name;
		gameObject.name = "Player_" + _name + ":" + GetSessionId();
	}

	private void ChangeName(object sender, EventArgs e)
	{
		SetName(m_gamepad.Name);
	}

	public int GetTeamId(){
		return m_teamId;
	}

	public void SetTeamId(int _teamId){
		m_teamId = _teamId;
	}

	public bool IsPlayingInCurrentRound(){
		return m_playingInCurrentRound;
	}

	public void SetPlayingInCurrentRound(bool _value){
		m_playingInCurrentRound = _value;
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

	public void SetPlayerColor(Color _color){
		m_gamepad.Color = _color;
		m_color = _color;
	}

	public void SetRandomColor(){
		m_gamepad.SetDefaultColor();
		m_gamepad.SendColor();
		m_color = m_gamepad.Color;
	}

	public string GetSessionId(){
		return m_gamepad.NetPlayer.GetSessionId();
	}

	public Drawing GetCurrentDrawing(){
		//Debug.Log("GetCurrentDrawing");
		return new Drawing(	m_gamepad.drawArray,
							m_gamepad.drawArrayWidth,
							m_gamepad.drawArrayHeight,
							m_gamepad.drawOptions.drawArrayDivision,
							m_gamepad.NetPlayer.GetSessionId(),
							m_teamId,
							m_gamepad.drawAccuracy);
	}

	public GameManager.GameState GetCurrentGameState(){
		if (m_gamepad.drawGamemode == "playing_free") return GameManager.GameState.playing_free;
		if (m_gamepad.drawGamemode == "playing_word") return GameManager.GameState.playing_word;

		return GameManager.GameState.initializing;
	}

	// TODO: deprecated
	private void OnDrawingReceived(){
		//PushDrawing(m_gamepad.drawArray);
		//PushAccuracy(m_gamepad.drawAccuracy);
		//Debug.Log("OnDrawingReceived");
		GameManager.s_gameManager.OnPlayerReceivedDrawing(this);
		PlaySound("Basic-select");
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

	public string GetCurrentLetter(){
		return m_currentLetter;
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

		//texture.alphaIsTransparency = true;

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

		letterPixelPrefab.transform.localScale = Vector3.one * gap * letterScale;

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

	public void DrawLetterOnBackgroundFromString(string letter){
		m_currentLetter = letter;
		m_gamepad.SendLetter(letter);
		PlaySound("newLetters");
	}

	public void SendNotification(string _message, bool _dismissable, float _timeout){
		m_gamepad.SendNotification(_message, _dismissable, _timeout);
		PlaySound("Basic-confirm");
	}

	public void PlaySound(string filenameWithoutExtension){
		m_soundPlayer.PlaySound(filenameWithoutExtension);
	}
		
}
