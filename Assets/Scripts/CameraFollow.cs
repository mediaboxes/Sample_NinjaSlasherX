using UnityEngine;
using System.Collections;

public enum CAMERATARGET {
	PLAYER,
	PLAYER_MARGIN,
	PLAYER_GROUND,
}

public enum CAMERAHOMING {
	DIRECT,
	LERP,
	SLERP,
	STOP,
}

public class CameraFollow:MonoBehaviour {
	//外部パラメーター(Inspector)
	[System.Serializable]
	public class Param {
		public CAMERATARGET targetType = CAMERATARGET.PLAYER_GROUND;
		public CAMERAHOMING homingType = CAMERAHOMING.LERP;
		public Vector2 margin = new Vector2(2.0f, 2.0f);
		public Vector2 homing = new Vector2(0.1f, 0.2f);
		public bool borderCheck = false;
		public GameObject borderLeftTop;
		public GameObject borderRightBottom;
		public bool viewAreaCheck = true;
		public Vector2 viewAreaMinMargin = new Vector2(0.0f, 0.0f);
		public Vector2 viewAreaMaxMargin = new Vector2(0.0f, 2.0f);

		public bool orthographicEnable = true;
		public float screenOGSize = 5.0f;
		public float screenOGSizeHoming = 0.1f;
		public float screenPSSize = 5.0f;
		public float screenPSSizeHoming = 0.1f;
	}
	public Param param;

	//キャッシュ 
	GameObject player;
	Transform playerTrfm;
	PlayerController playerCtrl;

	float screenOGSizeAdd = 0.0f;
	float screenPSSizeAdd = 0.0f;


	//コード(Monobehaviour)
	void Awake() {
		player = PlayerController.GetGameObject();
		playerTrfm = player.transform;
		playerCtrl = player.GetComponent<PlayerController>();
	}

	void LateUpdate() {
		float targetX = playerTrfm.position.x;
		float targetY = playerTrfm.position.y;
		float pX = transform.position.x;
		float pY = transform.position.y;
		float screenOGSize = GetComponent<Camera>().orthographicSize;
		float screenPSSize = GetComponent<Camera>().fieldOfView;

		switch(param.targetType) {
			case CAMERATARGET.PLAYER:
				targetX = playerTrfm.position.x;
				targetY = playerTrfm.position.y;
				break;
			case CAMERATARGET.PLAYER_MARGIN:
				targetX = playerTrfm.position.x + param.margin.x * playerCtrl.dir;
				targetY = playerTrfm.position.y + param.margin.y;
				break;
			case CAMERATARGET.PLAYER_GROUND:
				targetX = playerTrfm.position.x + param.margin.x * playerCtrl.dir;
				targetY = playerCtrl.groundY + param.margin.y;
				break;
		}
		if(param.borderCheck) {
			float cX = playerTrfm.transform.position.x;
			float cY = playerTrfm.transform.position.y;
			if(cX < param.borderLeftTop.transform.position.x ||
			   cX > param.borderRightBottom.transform.position.x ||
			   cY > param.borderLeftTop.transform.position.y ||
			   cY < param.borderRightBottom.transform.position.y) {
				return;
			}
		}
		if(param.viewAreaCheck) {
			float z = playerTrfm.position.z - transform.position.z;
			Vector3 minMargin = param.viewAreaMinMargin;
			Vector3 maxMargin = param.viewAreaMaxMargin;
			Vector2 min = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, z)) - minMargin;
			Vector2 max = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, z)) - maxMargin;
			if(playerTrfm.position.x < min.x || playerTrfm.position.x > max.x) {
				targetX = playerTrfm.position.x;
			}
			if(playerTrfm.position.y < min.y || playerTrfm.position.y > max.y) {
				targetY = playerTrfm.position.y;
				playerCtrl.groundY = playerTrfm.position.y;
			}
		}
		switch(param.homingType) {
			case CAMERAHOMING.DIRECT:
				pX = targetX;
				pY = targetY;
				screenOGSize = param.screenOGSize;
				screenPSSize = param.screenPSSize;
				break;
			case CAMERAHOMING.LERP:
				pX = Mathf.Lerp(transform.position.x, targetX, param.homing.x);
				pY = Mathf.Lerp(transform.position.y, targetY, param.homing.y);
				screenOGSize = Mathf.Lerp(screenOGSize, param.screenOGSize, param.screenOGSizeHoming);
				screenPSSize = Mathf.Lerp(screenPSSize, param.screenPSSize, param.screenPSSizeHoming);
				break;
			case CAMERAHOMING.SLERP:
				pX = Mathf.SmoothStep(transform.position.x, targetX, param.homing.x);
				pY = Mathf.SmoothStep(transform.position.y, targetY, param.homing.y);
				screenOGSize = Mathf.SmoothStep(screenOGSize, param.screenOGSize, param.screenOGSizeHoming);
				screenPSSize = Mathf.SmoothStep(screenPSSize, param.screenPSSize, param.screenPSSizeHoming);
				break;
			case CAMERAHOMING.STOP:
				break;
		}
		transform.position = new Vector3(pX, pY, transform.position.z);
		GetComponent<Camera>().orthographic = param.orthographicEnable;
		GetComponent<Camera>().orthographicSize = screenOGSize + screenOGSizeAdd;
		GetComponent<Camera>().fieldOfView = screenPSSize + screenPSSizeAdd;
		GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize,2.5f,10.0f);
		GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView, 30.0f, 100.0f);

		screenOGSizeAdd *= 0.99f;
		screenPSSizeAdd *= 0.99f;

	}

	//コード（その他）
	public void SetCamera(Param cameraPara) {
		param = cameraPara;
	}
	public void AddCameraSize(float ogAdd,float psAdd) {
		screenOGSizeAdd += ogAdd;
		screenPSSizeAdd += psAdd;
	}
}
