using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenLog : MonoBehaviour {

	static Dictionary<string, string> info = new Dictionary<string, string>();

	public static void SetInfo(string key, string value){
		info[key] = value;
	}

	// Update is called once per frame
	void Update () {
		Text instance = GetComponent<Text> ();
		instance.text = "";
		foreach (KeyValuePair<string, string> pair in info) {
			instance.text += pair.Key + ": " + pair.Value + "\n";
		}

	}
}
