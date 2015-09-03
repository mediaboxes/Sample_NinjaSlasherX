using UnityEngine;
using System.Collections;

public class EnemyMain_A : EnemyMain {
	//外部パラメーター(Inspector)
	public int aiIfRUNTOPPLAYER = 20;
	public int aiIfJUMPTOPPLAYER = 30;
	public int aiIfESCAPE = 10;

	public int damageAttack_A =1;

	//コード（AI処理サポート）
	public override void FixedUpdateAI() {
		//AIステート
		switch(aiState) {
			case ENEMYAISTS.ACTIONSELECT:
				//Debug.Log("ENEMYAISTS.ACTIONSELECT");
				//アクション選択
				int n = SelectRandomAIState();
				if(n < aiIfRUNTOPPLAYER) {
					SetAiState(ENEMYAISTS.RUNTOPLAYER, 3.0f);
				} else if(n < aiIfRUNTOPPLAYER + aiIfJUMPTOPPLAYER) {
					SetAiState(ENEMYAISTS.JUMPTOPLAYER, 1.0f);
				} else if(n < aiIfRUNTOPPLAYER + aiIfJUMPTOPPLAYER + aiIfESCAPE) {
					SetAiState(ENEMYAISTS.ESCAPE, Random.Range(2.0f, 5.0f));
				} else {
					SetAiState(ENEMYAISTS.WAIT, 1.0f + Random.Range(0.0f, 1.0f));
				}
				enemyCtrl.ActionMove(0.0f);
				break;
			case ENEMYAISTS.WAIT:
				//Debug.Log("ENEMYAISTS.WAIT");
				enemyCtrl.ActionLookup(player, 0.1f);
				enemyCtrl.ActionMove(0.0f);
				break;
			case ENEMYAISTS.RUNTOPLAYER:
				//Debug.Log("ENEMYAISTS.RUNTOPLAYER");
				if(GetDistancePlayerY() > 3.0f) {
					SetAiState(ENEMYAISTS.JUMPTOPLAYER, 1.0f);
				}
				if(!enemyCtrl.ActionMoveToNear(player, 2.0f)) {
					Attack_A();
				}
				break;
			case ENEMYAISTS.JUMPTOPLAYER:
				//Debug.Log("ENEMYAISTS.JUMPTOPLAYER");
				if(GetDistanPlayer() < 2.0f && IsChangeDistanePlayer(0.5f)) {
					Attack_A();
					break;
				}
				enemyCtrl.ActionJump();
				enemyCtrl.ActionMoveToNear(player, 0.1f);
				SetAiState(ENEMYAISTS.FREEZ, 0.5f);
				break;
			case ENEMYAISTS.ESCAPE:
				//Debug.Log("ENEMYAISTS.ESCAPE");
				if(!enemyCtrl.ActionMoveToFat(player, 7.0f)) {
					SetAiState(ENEMYAISTS.ACTIONSELECT, 1.0f);
				}
				break;
		}
	}

	//コード（アクション処理サポート）
	void Attack_A() {
		enemyCtrl.ActionLookup(player, 0.1f);
		enemyCtrl.ActionMove(0.0f);
		enemyCtrl.ActionAttack("Attack_A", damageAttack_A);
		SetAiState(ENEMYAISTS.WAIT, 2.0f);
	}
}
