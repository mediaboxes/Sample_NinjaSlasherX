using UnityEngine;
using System.Collections;

public class PlayerController :BaseCharacterController {
	//外部パラメーター(Inspector)
	//セーブデータ
	public static float nowHpMax = 0;
	public static float nowHp = 0;
	public static int score = 0;

	public float initHpMax = 20.0f;
	[Range(0.1f,100.0f)]public float initSpeed = 12.0f;

	//外部パラメーター
	[System.NonSerialized] public Vector3 enemyActiveZonePointA;
	[System.NonSerialized] public Vector3 enemyActiveZonePointB;
	[System.NonSerialized] public float groundY = 0.0f;

	[System.NonSerialized] public int comboCount = 0;

	//アニメーションのハッシュ名
	public readonly static int ANISTS_Idle = Animator.StringToHash("Base Layer.Player_Idle");
	public readonly static int ANISTS_Walk = Animator.StringToHash("Base Layer.Player_Walk");
	public readonly static int ANISTS_Run = Animator.StringToHash("Base Layer.Player_Run");
	public readonly static int ANISTS_Jump = Animator.StringToHash("Base Layer.Player_Jump");
	public readonly static int ANISTS_ATK_A = Animator.StringToHash("Base Layer.Player_ATK_A");
	public readonly static int ANISTS_ATK_B = Animator.StringToHash("Base Layer.Player_ATK_B");
	public readonly static int ANISTS_ATK_C = Animator.StringToHash("Base Layer.Player_ATK_C");
	public readonly static int ANISTS_ATKJUMP_A = Animator.StringToHash("Base Layer.Player_ATKJUMP_A");
	public readonly static int ANISTS_ATKJUMP_B = Animator.StringToHash("Base Layer.Player_ATKJUMP_B");
	public readonly static int ANISTS_DEAD = Animator.StringToHash("Base Layer.Player_DEAD");

	//キャッシュ 
	LineRenderer hudHpBar;
	TextMesh hudScore;
	GameObject hudCombo;
	TextMesh hudComboText;


	//内部パラメーター
	int jumpCount = 0;

	volatile bool atkInputEnabled = false;
	volatile bool atkInputNow = false;

	bool breakEnable = true;
	float groundFriction = 0.0f;
	float comboTimer = 0.0f;

	//コード(サポート関数)
	public static GameObject GetGameObject() {
		return GameObject.FindGameObjectWithTag("Player");
	}
	public static Transform GetTransform() {
		return GameObject.FindGameObjectWithTag("Player").transform;
	}
	public static PlayerController GetController() {
		return GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }
	public static Animator GetAnimator() {
		return GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
	}

	//コード(Monobehaviour)
	protected override void Awake() {
		base.Awake();

		//!!!ガベコレ強制実行
		System.GC.Collect();
		//!!!

		//キャッシュ
		hudHpBar = GameObject.Find("HUD_HPBar").GetComponent<LineRenderer>();
		hudScore = GameObject.Find("HUD_Score").GetComponent<TextMesh>();
		hudCombo= GameObject.Find("HUD_Combo");
		hudComboText = GameObject.Find("HUD_Combo_TEXT").GetComponent<TextMesh>();

		speed = initSpeed;
		SetHp(initHpMax, initHpMax);

		//アクティブゾーンを取得
		BoxCollider2D boxCol2D = transform.Find("Collider_EnemyActiveZone").GetComponent<BoxCollider2D>();
		enemyActiveZonePointA = new Vector3(boxCol2D.offset.x - boxCol2D.size.x / 2.0f, boxCol2D.offset.y - boxCol2D.size.y / 2.0f);
		enemyActiveZonePointB = new Vector3(boxCol2D.offset.x + boxCol2D.size.x / 2.0f, boxCol2D.offset.y + boxCol2D.size.y / 2.0f);
		boxCol2D.transform.gameObject.SetActive(false);
	}
	protected override void FixedUpdateCharacter() {
		//現在のステート
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		//着地チェック
		if(jumped) {
			if((grounded && !groundedPrev) || (grounded && Time.fixedTime > jumpStartTime + 1.0f)) {
				animator.SetTrigger("Idle");
				jumped = false;
				jumpCount = 0;
			}
		}

		//攻撃中か
		if(stateInfo.fullPathHash == ANISTS_ATK_A ||
		   stateInfo.fullPathHash == ANISTS_ATK_B ||
		   stateInfo.fullPathHash == ANISTS_ATK_C ||
		   stateInfo.fullPathHash == ANISTS_ATKJUMP_A ||
		   stateInfo.fullPathHash == ANISTS_ATKJUMP_B) {
			speedVx = 0;
		}

		//着地チェック
		if(!jumped) {
			jumpCount = 0;
			GetComponent<Rigidbody2D>().gravityScale = gravityScale;
		}

		//キャラクターの方向
		transform.localScale = new Vector3(baseScaleX * dir, transform.localScale.y, transform.localScale.z);
		//ジャンプ中の横移動減速
		if(jumped && !grounded) {
			if(breakEnable) {
				breakEnable = false;
				speedVx *= 0.9f;
			}
		}

		//移動停止(減速処理)
		if(breakEnable) {
			speedVx *= groundFriction;
		}

		//カメラ
		//Camera.main.transform.position = transform.position - Vector3.forward;
	}
	protected override void Update() {
		base.Update();

		//ステータス表示
		hudHpBar.SetPosition(1, new Vector3(5.0f * (hp / hpMax), 0.0f, 0.0f));
		hudScore.text = string.Format("Score {0}", score);

		if(comboTimer <= 0.0f) {
			hudCombo.SetActive(false);
			comboCount = 0;
			comboTimer = 0.0f;
		} else {
			comboTimer -= Time.deltaTime;
			if(comboTimer > 5.0f) {
				comboTimer = 5.0f;
			}
			float s = 0.3f + 0.5f * comboTimer;
			hudCombo.SetActive(true);
			hudCombo.transform.localScale = new Vector3(s, s, 1.0f);
		}
	}

	//コード（アニメーションイベント）
	//入力許可
	public void EnebleAttackInput() {
		atkInputEnabled = true;
	}
	//次の攻撃遷移
	public void SetNextAttack(string name) {
		if(atkInputNow == true) {
			atkInputNow = false;
			animator.Play(name);
		}
	}

	//コード（基本アクション）
	public override void ActionMove(float n) {
		if(!activeSts) {
			return;
		}
		//初期化
		float dirOld = dir;
		breakEnable = false;

		//アニメーション指定
		float moveSpeed = Mathf.Clamp(Mathf.Abs(n), -1.0f, +1.0f);
		animator.SetFloat("MoveSpeed", moveSpeed);
		//animator.speed=1.0f+moveSpeed;
		if(n != 0.0f) {
			//移動
			dir = Mathf.Sign(n);
			moveSpeed = (moveSpeed < 0.5) ? (moveSpeed * (1.0f / 0.5f)) : 1.0f;
			speedVx = initSpeed * moveSpeed * dir;
		} else {
			//移動停止
			breakEnable = true;
		}
		if(dirOld != dir) {
			breakEnable = true;
		}
	}
	public void ActionJump() {
		switch(jumpCount) {
			case 0:
				if(grounded) {
					animator.SetTrigger("Jump");
					GetComponent<Rigidbody2D>().velocity = Vector2.up * 30.0f;
					jumpStartTime = Time.fixedTime;
					jumped = true;
					jumpCount++;
				}
				break;
			case 1:
				if(!grounded) {
					animator.Play("Player_Jump", 0, 0.0f);
					GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 20.0f);
					jumped = true;
					jumpCount++;
				}
				break;
		}
	}

	public void ActionAttack() {
		//現在のステート
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		if(stateInfo.fullPathHash == ANISTS_Idle ||
		   stateInfo.fullPathHash == ANISTS_Walk ||
		   stateInfo.fullPathHash == ANISTS_Run ||
		   stateInfo.fullPathHash == ANISTS_Jump) {
			animator.SetTrigger("Attack_A");
		} else {
			if(atkInputEnabled) {
				atkInputEnabled = false;
				atkInputNow = true;
			}
		}
	}

	public void ActionDamage (float damage) {
		if(!activeSts) {
			return;
		}
		//Debug.Log("DMG_A");
		animator.SetTrigger("DMG_A");
		speedVx = 0;
		GetComponent<Rigidbody2D>().gravityScale = gravityScale;
		if(jumped) {
			damage *= 1.5f;
		}
		if(SetHp(hp - damage, hpMax)) {
			Dead(true);
		}
	}

	//コード（その他）
	public override void Dead(bool gameOver) {
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if(!activeSts || stateInfo.fullPathHash == ANISTS_DEAD) {
			return;
		}
		base.Dead(gameOver);
		SetHp(0, hpMax);
		Invoke("GameOver", 3.0f);

		GameObject.Find("HUD_Dead").GetComponent<MeshRenderer>().enabled = true;
		GameObject.Find("HUD_DeadShadow").GetComponent<MeshRenderer>().enabled = true;
    }
	public void GameOver() {
		PlayerController.score = 0;
		Application.LoadLevel(Application.loadedLevelName);
	}
	public override bool SetHp(float _hp, float _hpMax) {
		if(_hp > _hpMax) {
			_hp = _hpMax;
		}
		nowHp = _hp;
		nowHpMax = _hpMax;
		return base.SetHp(_hp,_hpMax);
	}
	public void AddCombo() {
		comboCount++;
		comboTimer += 1.0f;
		hudComboText.text = string.Format("Combo {0}", comboCount);

	}
}
