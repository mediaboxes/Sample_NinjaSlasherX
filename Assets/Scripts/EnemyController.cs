using UnityEngine;
using System.Collections;

public class EnemyController:BaseCharacterController {
	//外部パラメーター(Inspector)
	public float initHpMax = 5.0f;
	[Range(0.1f,100.0f)]public float initSpeed = 6.0f;
	public bool jumpActionEnable = true;
	public Vector2 jumpPower = new Vector2(0.0f, 1500.0f);
	public int addScore = 500;

	//外部パラメーター
	[System.NonSerialized]  public bool cameraRendered = false;
	[System.NonSerialized]  public bool attackEnabled = false;
	[System.NonSerialized]  public int attackDamage = 1;
	[System.NonSerialized]  public Vector2 attackNockBackVector = Vector3.zero;

	//アニメーションのハッシュ名
	public readonly static int ANISTS_Idle = Animator.StringToHash("Base Layer.Enemy_Idle");
	public readonly static int ANISTS_Run = Animator.StringToHash("Base Layer.Enemy_Run");
	public readonly static int ANISTS_Jump = Animator.StringToHash("Base Layer.Enemy_Jump");
	public readonly static int ANITAG_ATTACK = Animator.StringToHash("Attack");
	public readonly static int ANISTS_DMG_A = Animator.StringToHash("Base Layer.Enemy_DMG_A");
	public readonly static int ANISTS_DMG_B = Animator.StringToHash("Base Layer.Enemy_DMG_B");
	public readonly static int ANISTS_Dead = Animator.StringToHash("Base Layer.Enemy_Dead");

	//キャッシュ 
	PlayerController playerCtrl;
	Animator playerAnim;


	//コード(Monobehaviour)
	protected override void Awake() {
		base.Awake();
		playerCtrl = PlayerController.GetController();
		playerAnim = playerCtrl.GetComponent<Animator>();
		hpMax = initHpMax;
		hp = hpMax;
		speed = initSpeed;
	}
	protected override void FixedUpdateCharacter() {
		if(!cameraRendered) {
			return;
		}
		//着地チェック
		if(jumped) {
			if((grounded && !groundedPrev) ||
				(grounded && Time.fixedTime > jumpStartTime + 1.0f)) {
				jumped = false;
			}
			if(Time.fixedTime > jumpStartTime + 1.0f) {
				if(GetComponent<Rigidbody2D>().gravityScale < gravityScale) {
					GetComponent<Rigidbody2D>().gravityScale = gravityScale;
				}
			}
		} else {
			GetComponent<Rigidbody2D>().gravityScale = gravityScale;
		}
		//キャラクターの方向
		transform.localScale = new Vector3(baseScaleX * dir, transform.localScale.y, transform.localScale.z);

		//Memo:空中ダメージではX移動を禁止
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		if(stateInfo.fullPathHash == ANISTS_DMG_A ||
		   stateInfo.fullPathHash == ANISTS_DMG_B ||
		   stateInfo.fullPathHash == ANISTS_Dead) {
			speedVx = 0.0f;
			GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f, GetComponent<Rigidbody2D>().velocity.y);
		}
	}

	//コード（基本アクション）
	public bool ActionJump() {
		if(jumpActionEnable && grounded && !jumped) {
			animator.SetTrigger("Jump");
			GetComponent<Rigidbody2D>().AddForce(jumpPower);
			jumped = true;
			jumpStartTime = Time.fixedTime;
		}
		return jumped;
	}
	public void ActionAttack(string atkname, int damage) {
		attackEnabled = true;
		attackDamage = damage;
		animator.SetTrigger(atkname);
	}
	public void ActionDamage() {
		int damage = 0;
		if(hp <= 0) {
			return;
		}
		if(superArmor) {
			animator.SetTrigger("SuperArmor");
		}
		AnimatorStateInfo stateInfo = playerAnim.GetCurrentAnimatorStateInfo(0);
		if(stateInfo.fullPathHash == PlayerController.ANISTS_ATK_C) {
			//攻撃Cはスキップ
		} else if(!grounded) {
			damage = 2;
			if(!superArmor || superArmor_jumpAttackDmg) {
				animator.SetTrigger("DMG_B");
				jumped = true;
				jumpStartTime = Time.fixedTime;
				playerCtrl.GetComponent<Rigidbody2D>().AddForce(new Vector2(0.0f, 20.0f));
				Debug.Log(string.Format(">>>DMG_B {0}", stateInfo.fullPathHash));
			}
		} else {
			damage = 1;
			if(!superArmor) {
				animator.SetTrigger("DMG_A");
				Debug.Log(string.Format(">>>DMG_A {0}", stateInfo.fullPathHash));
			}
		}
		if(SetHp(hp - damage, hpMax)) {
			Dead(false);
			int addScoreV = ((int)((float)addScore * (playerCtrl.hp / playerCtrl.hpMax)));
			addScoreV = (int)((float)addScore * (grounded ? 1.0 : 1.5f));
			PlayerController.score += addScoreV;
		}
		playerCtrl.AddCombo();
	}
	//コード（その他）
	public override void Dead (bool gameOver) {
		base.Dead(gameOver);
		Destroy(gameObject, 1.0f);
	}
}