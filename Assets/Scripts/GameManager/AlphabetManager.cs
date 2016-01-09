using UnityEngine;
using System.Collections;

public class AlphabetManager : MonoBehaviour {

	public const int A = 0;
	public const int B = 1;
	public const int C = 2;
	public const int D = 3;
	public const int E = 4;
	public const int F = 5;
	public const int G = 6;
	public const int H = 7;
	public const int I = 8;
	public const int J = 9;
	public const int K = 10;
	public const int L = 11;
	public const int M = 12;
	public const int N = 13;
	public const int O = 14;
	public const int P = 15;
	public const int Q = 16;
	public const int R = 17;
	public const int S = 18;
	public const int T = 19;
	public const int U = 20;
	public const int V = 21;
	public const int W = 22;
	public const int X = 23;
	public const int Y = 24;
	public const int Z = 25;

	public const int width = 80;
	public const int height = 115;

	public static Texture2D[] g_letters;
	public static Texture2D[] g_digits;

	//public static AlphabetManager instance;

	// Use this for initialization
	void Awake () {
//		if (instance != null){
//			Destroy(this);
//		} else{
//			instance = this;
//		}

		Texture2D[] letters = new Texture2D[26];
		Texture2D[] digits = new Texture2D[10];

		string path = "Sprites/alphanumerics/";

		// Load all letters into array
		letters[0] = Resources.Load(path + "a", typeof(Texture2D)) as Texture2D;
		letters[1] = Resources.Load(path + "b", typeof(Texture2D)) as Texture2D;
		letters[2] = Resources.Load(path + "c", typeof(Texture2D)) as Texture2D;
		letters[3] = Resources.Load(path + "d", typeof(Texture2D)) as Texture2D;
		letters[4] = Resources.Load(path + "e", typeof(Texture2D)) as Texture2D;
		letters[5] = Resources.Load(path + "f", typeof(Texture2D)) as Texture2D;
		letters[6] = Resources.Load(path + "g", typeof(Texture2D)) as Texture2D;
		letters[7] = Resources.Load(path + "h", typeof(Texture2D)) as Texture2D;
		letters[8] = Resources.Load(path + "i", typeof(Texture2D)) as Texture2D;
		letters[9] = Resources.Load(path + "j", typeof(Texture2D)) as Texture2D;
		letters[10] = Resources.Load(path + "k", typeof(Texture2D)) as Texture2D;
		letters[11] = Resources.Load(path + "l", typeof(Texture2D)) as Texture2D;
		letters[12] = Resources.Load(path + "m", typeof(Texture2D)) as Texture2D;
		letters[13] = Resources.Load(path + "n", typeof(Texture2D)) as Texture2D;
		letters[14] = Resources.Load(path + "o", typeof(Texture2D)) as Texture2D;
		letters[15] = Resources.Load(path + "p", typeof(Texture2D)) as Texture2D;
		letters[16] = Resources.Load(path + "q", typeof(Texture2D)) as Texture2D;
		letters[17] = Resources.Load(path + "r", typeof(Texture2D)) as Texture2D;
		letters[18] = Resources.Load(path + "s", typeof(Texture2D)) as Texture2D;
		letters[19] = Resources.Load(path + "t", typeof(Texture2D)) as Texture2D;
		letters[20] = Resources.Load(path + "u", typeof(Texture2D)) as Texture2D;
		letters[21] = Resources.Load(path + "v", typeof(Texture2D)) as Texture2D;
		letters[22] = Resources.Load(path + "w", typeof(Texture2D)) as Texture2D;
		letters[23] = Resources.Load(path + "x", typeof(Texture2D)) as Texture2D;
		letters[24] = Resources.Load(path + "y", typeof(Texture2D)) as Texture2D;
		letters[25] = Resources.Load(path + "z", typeof(Texture2D)) as Texture2D;

		// Load all digits into array
		digits[0] = Resources.Load(path + "0", typeof(Texture2D)) as Texture2D;
		digits[1] = Resources.Load(path + "1", typeof(Texture2D)) as Texture2D;
		digits[2] = Resources.Load(path + "2", typeof(Texture2D)) as Texture2D;
		digits[3] = Resources.Load(path + "3", typeof(Texture2D)) as Texture2D;
		digits[4] = Resources.Load(path + "4", typeof(Texture2D)) as Texture2D;
		digits[5] = Resources.Load(path + "5", typeof(Texture2D)) as Texture2D;
		digits[6] = Resources.Load(path + "6", typeof(Texture2D)) as Texture2D;
		digits[7] = Resources.Load(path + "7", typeof(Texture2D)) as Texture2D;
		digits[8] = Resources.Load(path + "8", typeof(Texture2D)) as Texture2D;
		digits[9] = Resources.Load(path + "9", typeof(Texture2D)) as Texture2D;

		// Set globals
		g_letters = letters;
		g_digits = digits;
	}

	// Creates an array of the alpha values of the sprite
	public static int[] LetterToArray(int letterIndex){
		int[] array = new int[width * height];
		Texture2D letter = g_letters[letterIndex];
		int i = 0;

		for (int y = 0; y < height; y++){
			for (int x = 0; x < width; x++){
				array[y * width + x] = to255(letter.GetPixel(x, y).a);
			}
		}

		return array;
	}

	public static int to255(float t){
		return (int)(t * 255) | 0;
	}

}
