﻿using UnityEngine;
using System.Collections;

public class PlayerMain:MonoBehaviour {
	//キャッシュ-------------------------
	PlayerController playerCtrl;
	zFoxVirtualPad vpad;

	void Awake() {
		playerCtrl = GetComponent<PlayerController>();
		vpad = GameObject.FindObjectOfType<zFoxVirtualPad>();
	}

	void Update() {
		if(!playerCtrl.activeSts) {
			return;
		}
		//バーチャルパッド
		float vpad_vertical = 0.0f;
		float vpad_horizontal = 0.0f;
		zFOXVPAD_BUTTON vpad_btnA = zFOXVPAD_BUTTON.NON;
		zFOXVPAD_BUTTON vpad_btnB = zFOXVPAD_BUTTON.NON;
		if(vpad != null) {
			vpad_vertical = vpad.vertical;
			vpad_horizontal = vpad.horizontal;
			vpad_btnA = vpad.buttonA;
			vpad_btnB = vpad.buttonB;
		}


		//移動
		float joyMv = Input.GetAxis("Horizontal");
		joyMv = Mathf.Pow(Mathf.Abs(joyMv), 3.0f) * Mathf.Sign(joyMv);

		float vpadMv = vpad_horizontal;
		vpadMv = Mathf.Pow(Mathf.Abs(vpadMv), 1.5f) * Mathf.Sign(vpadMv);

		playerCtrl.ActionMove(joyMv+ vpadMv);

		//ジャンプ
		if(Input.GetButtonDown("Jump") || vpad_btnA==zFOXVPAD_BUTTON.DOWN) {
			playerCtrl.ActionJump();
		}
		//攻撃
		if(Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2") || Input.GetButtonDown("Fire3") || vpad_btnB == zFOXVPAD_BUTTON.DOWN) {
			playerCtrl.ActionAttack();
		}
	}
}
