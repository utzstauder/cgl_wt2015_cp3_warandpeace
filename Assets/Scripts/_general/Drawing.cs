using UnityEngine;
using System.Collections;

public class Drawing {

	public int[] drawing;
	public int width;
	public int height;
	public int gap;
	public string playerId;
	public float accuracy;

	public Drawing(int[] _drawing, int _width, int _height, int _gap, string _playerId, float _accuracy){
		drawing = _drawing;
		width = _width;
		height = _height;
		gap = _gap;
		playerId = _playerId;
		accuracy = _accuracy;
	}

}
