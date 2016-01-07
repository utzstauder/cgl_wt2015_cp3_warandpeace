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

	// Use this for initialization
	void Start () {
		m_gamepad  = GetComponent<HFTGamepad>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)){
			DrawTextureOnCube();
		}
	}

	void PullDrawing(){
		m_gamepad.NetPlayer.SendCmd("pullDrawing");
	}

	void DrawTextureOnCube(){
		Renderer cubeRenderer = GameObject.Find("DrawTarget").GetComponent<Renderer>();

		// Create texture from array
		// TODO: implement
		int width = m_gamepad.drawArrayWidth;
		int height= m_gamepad.drawArrayHeight;
		Color color;

		Debug.Log(m_gamepad.drawArray.Length);

		Texture2D texture = new Texture2D(width, height);
		for (int y = 0; y < height; y++){
			for (int x = 0; x < width; x++){
				color = m_gamepad.drawArray[width*y + x] > 0 ? Color.black : Color.clear;
				//color = Color.black;
				//color.a = m_gamepad.drawArray[width*y + x];
				texture.SetPixel(x, height-y, color);
			}
		}

		byte[] bytes = texture.EncodeToPNG();
		File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

		cubeRenderer.material.mainTexture = texture;
	}

	void OnGUI(){
		if (GUI.Button(new Rect(10, 10, 50, 20), "Pull drawing")){
			PullDrawing();
		}
	}
		
}
