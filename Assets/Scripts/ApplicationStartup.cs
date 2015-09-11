using UnityEngine;
using System.Collections;

public class ApplicationStartup : MonoBehaviour {

	void Start () {
		Debug.Log("=== Application StartUp [Ninja SlasherX]");
		SaveData.LoadOption();
	}
}
