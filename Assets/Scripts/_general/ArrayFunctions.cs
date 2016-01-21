using UnityEngine;
using System.Collections;

public class ArrayFunctions {

	public static void RandomizeArray(ref int[] _array)
	{
		for (int i = _array.Length - 1; i > 0; i--) {
			int r = Random.Range(0,i);
			int tmp = _array[i];
			_array[i] = _array[r];
			_array[r] = tmp;
		}
	}

}
