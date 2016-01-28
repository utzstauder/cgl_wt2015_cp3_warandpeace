using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TXTReader{

	static char[] SEPERATOR = {','};

	public static List<string> Read(string _filename){
		List<string> returnList = new List<string>();
		TextAsset data = Resources.Load(_filename) as TextAsset;

		Debug.Log(data.text);

		string[] splitText = data.text.Split(SEPERATOR[0]);

		for (int i = 0; i < splitText.Length; i++){
			returnList.Add(splitText[i]);
		}

		return returnList;
	}
}
