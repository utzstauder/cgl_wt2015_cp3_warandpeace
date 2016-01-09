using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimes;
using CSSParse;

public class DrawInputPlayer : MonoBehaviour {

	private HFTGamepad m_gamepad;
	private HFTInput m_hftInput;
	private UnityEngine.UI.Text m_text;
	private Color m_color;
	private string m_name;

	public Letter letterPrefab;
	public LetterPixel letterPixelPrefab;
	public float letterPixelScale = 7.0f;
	public float letterScale = .5f;

	// Use this for initialization
	void Start () {
		m_gamepad  = GetComponent<HFTGamepad>();
		m_color = m_gamepad.Color;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)){
			//DrawTextureOnCube();
			//SpawnPixelsFromArray(2);
		}
	}

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

	// TODO: bad implementation / doesn't belong here
	void DrawLetterOnBackground(int letter){
		m_gamepad.SendLetter(letter);
	}

	void OnGUI(){
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
