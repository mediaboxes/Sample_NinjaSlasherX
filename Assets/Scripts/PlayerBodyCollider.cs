using UnityEngine;
using System.Collections;

public class PlayerBodyCollider:MonoBehaviour {
	PlayerController playerCtrl;

	void Awake() {
		playerCtrl = transform.parent.GetComponent<PlayerController>();
	}

	void OnTriggerEnter2D(Collider2D other) {
		if(other.tag == "EnemyArm") {
			EnemyController enemyCtrl = other.GetComponentInParent<EnemyController>();
			if(enemyCtrl.attackEnabled) {
				enemyCtrl.attackEnabled = false;
				playerCtrl.dir = (playerCtrl.transform.position.x < enemyCtrl.transform.position.x) ? +1 : -1;
				//playerCtrl.AddForceAnimatorVx
				//playerCtrl.AddForceAnimatorVy
				playerCtrl.ActionDamage(enemyCtrl.attackDamage);
			}
		}
	}

	void OnCollisionStay2D(Collision2D col) {
		if(!playerCtrl.jumped &&
			(col.gameObject.tag == "Road" ||
			 col.gameObject.tag == "MoveObject" ||
			 col.gameObject.tag == "Enemy")) {
			playerCtrl.groundY = transform.parent.transform.position.y;
		}
	}
}
