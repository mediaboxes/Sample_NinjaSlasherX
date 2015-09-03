using UnityEngine;
using System.Collections;

public class EnemySprite : MonoBehaviour {
	EnemyMain enemyMain;

	void Awake() {
		enemyMain = GetComponentInParent<EnemyMain>();
	}
	void OnWillRenderObject() {
		if(Camera.current.tag == "MainCamera") {
			enemyMain.cameraEnabled = true;
		}
	}
}
