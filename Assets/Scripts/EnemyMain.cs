using UnityEngine;
using System.Collections;

public enum ENEMYAISTS {//AIステート
	ACTIONSELECT,
	WAIT,
	RUNTOPLAYER,
	JUMPTOPLAYER,
	ESCAPE,
	ATTACKOMSIGHT,
	FREEZ,
}

public class EnemyMain : MonoBehaviour {
	//外部パラメーター(Inspector)
	public bool cameraSwitch = true;
	public bool inActiveZoneSwitch = false;

    public int degug_SelectRandomAIState = -1;

	//外部パラメーター
	[System.NonSerialized] public bool cameraEnabled = false;
	[System.NonSerialized] public bool inActiveZone = false;
	[System.NonSerialized] public ENEMYAISTS aiState = ENEMYAISTS.ACTIONSELECT;

	//キャッシュ 
	protected EnemyController enemyCtrl;
	protected GameObject player;
	protected PlayerController playerCtrl;

	//内部パラメーター
	protected float aiActionTimeLength = 0.0f;
	protected float aiActionTimeStart = 0.0f;
	protected float distanceToPlayer = 0.0f;
	protected float distanceToPlayerPrev = 0.0f;

	//コード(Monobehaviour)
	public virtual void Awake() {
		enemyCtrl = GetComponent<EnemyController>();
		player = PlayerController.GetGameObject();
		playerCtrl = player.GetComponent<PlayerController>();
	}

	public virtual void Start() {
	}
	public void OnTriggerStay2D(Collider2D other) {
		if(enemyCtrl.grounded && CheckAction()) {
			if(other.name == "EnemyJumpTrigger_L") {
				if(enemyCtrl.ActionJump()) {
					enemyCtrl.ActionMove(-1.0f);
				}
			} else if(other.name == "EnemyJumpTrigger_R") {
				if(enemyCtrl.ActionJump()) {
					enemyCtrl.ActionMove(1.0f);
				}
			}else if(other.name == "EnemyJumpTrigger") {
				enemyCtrl.ActionJump();
			}
		}
	}
	public virtual void Updatet() {
		cameraEnabled = false;
	}
	public virtual void FixedUpdate() {
		if(BeginEnemyCommonWork()) {
			FixedUpdateAI();
			EndEnemyCommonWork();
		}
	}
	public virtual void FixedUpdateAI() {
	}

	//コード（基本AI処理）
	public bool BeginEnemyCommonWork() {
		if(enemyCtrl.hp <= 0) {
			return false;
		}
		//アクティブゾーン
		if(inActiveZoneSwitch) {
			inActiveZone = false;
			Vector3 vecA = player.transform.position + playerCtrl.enemyActiveZonePointA;
			Vector3 vecB = player.transform.position + playerCtrl.enemyActiveZonePointB;

			if(transform.position.x > vecA.x && transform.position.x < vecB.x &&
				transform.position.y > vecA.y && transform.position.y < vecB.y) {
				inActiveZone = true;
			}
		}

		//空中は強制実行
		if(enemyCtrl.grounded) {
			if(cameraSwitch && !cameraEnabled && !inActiveZone) {
				//カメラに写っていない
				enemyCtrl.ActionMove(0.0f);
				enemyCtrl.cameraRendered = false;
				enemyCtrl.animator.enabled = false;
				GetComponent<Rigidbody2D>().Sleep();
				return false;
			}
		}
		enemyCtrl.animator.enabled = true;
		enemyCtrl.cameraRendered = true;

		if(!CheckAction()) {
			return false;
		}
		return true;
	}

	public void EndEnemyCommonWork() {
		float time = Time.fixedTime - aiActionTimeStart;
		if(time > aiActionTimeLength) {
			aiState = ENEMYAISTS.ACTIONSELECT;
		}
	}
	public bool CheckAction() {
		AnimatorStateInfo stateInfo = enemyCtrl.animator.GetCurrentAnimatorStateInfo(0);
		if(stateInfo.tagHash == EnemyController.ANITAG_ATTACK || 
			stateInfo.fullPathHash == EnemyController.ANISTS_DMG_A ||
			stateInfo.fullPathHash == EnemyController.ANISTS_DMG_B ||
			stateInfo.fullPathHash == EnemyController.ANISTS_Dead) {
			return false;
		}
		return true;
	}
	public int SelectRandomAIState() {
#if UNITY_EDITOR
		//		if(debug_SelectRandomAIState >= 0) {
		//			return debug_SelectRandomAIState;
		//}
#endif
		return Random.Range(0, 100 + 1);
	}
	public void SetAiState(ENEMYAISTS sts,float t) {
		aiState = sts;
		aiActionTimeStart = Time.fixedTime;
		aiActionTimeLength = t;
	}
	public virtual void SetCombatAIState(ENEMYAISTS sts) {
		aiState = sts;
		aiActionTimeStart = Time.fixedTime;
		enemyCtrl.ActionMove(0.0f);
	}

	//コード（AI処理サポート）
	public float GetDistanPlayer() {
		distanceToPlayerPrev = distanceToPlayer;
		distanceToPlayer = Vector3.Distance(transform.position, playerCtrl.transform.position);
		return distanceToPlayer;
	}
	public bool IsChangeDistanePlayer(float l) {
		return (Mathf.Abs(distanceToPlayer - distanceToPlayerPrev) > l);
	}
	public float GetDistancePlayerX() {
		Vector3 posA = transform.position;
		Vector3 posB = playerCtrl.transform.position;
		posA.y = 0;
		posA.z = 0;
		posB.y = 0;
		posB.z = 0;
		return Vector3.Distance(posA, posB);
	}
	public float GetDistancePlayerY() {
		Vector3 posA = transform.position;
		Vector3 posB = playerCtrl.transform.position;
		posA.x = 0;
		posA.z = 0;
		posB.x = 0;
		posB.z = 0;
		return Vector3.Distance(posA, posB);
	}
}
