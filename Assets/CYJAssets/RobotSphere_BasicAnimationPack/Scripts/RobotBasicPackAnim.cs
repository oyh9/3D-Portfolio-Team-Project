using UnityEngine;
using System.Collections;

public class RobotBasicPackAnim : MonoBehaviour {

	enum AnimType { Walk_Anim, Roll_Anim, Open_Anim, Turn_R_Anim, Turn_L_Anim}
	enum JumpState { Delay, Jump, Fall, Land, Grounded}

	Animator anim;

	[Header ("Translation"), SerializeField]
	float movementSpeed;
	[SerializeField]
	float rollingSpeed, rollingDelay;
	float rollingStartTime;
	Vector3 prevPos;
	Vector3 currentPos;
	bool canMove = false;

	[Header("Rotation"), SerializeField]
	float rotSpeed = 40f;
	Vector3 prevRot = Vector3.zero;
	Vector3 currentRot = Vector3.zero;
	ControlledRotationData controlledRot = new ControlledRotationData();
	bool isLargeTurn = false;

	[Header("Jump"), SerializeField]
	float jumpDuration;
	[SerializeField]
	float jumpDelay, landDelay, jumpHight;
	float jumpStartPosY;
	JumpState jumpState = JumpState.Grounded;
	float jumpStartTime = 0f;

	[Header("Close"), SerializeField]
	float openDelay;



	// Use this for initialization
	void Awake()
	{
		anim = gameObject.GetComponent<Animator> ();
		gameObject.transform.eulerAngles = currentRot;
		prevPos = currentPos = gameObject.transform.position;
		if (anim.GetBool("Open_Anim"))
		{
			StartCoroutine(WaitCoroutine(openDelay, onComplete: () =>
			{
				canMove = true;
			}));
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		CheckKey();
		JumpChatacter();
		RotateCharacter();
		TranslateCharacter();
	}

	void CheckKey()
	{
		// Walk
		if (canMove && Input.GetKey (KeyCode.W))
		{
			anim.SetBool(AnimType.Walk_Anim.ToString(), true);
			UpdateMovement(movementSpeed);
		} 
		else if (Input.GetKeyUp (KeyCode.W))
		{
			anim.SetBool (AnimType.Walk_Anim.ToString(), false);
		}

		// Large Turn Toggle
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			isLargeTurn = !isLargeTurn;
			anim.SetBool("LargeTurn_Anim", isLargeTurn);
		}

		// Rotate Left
		if((anim.GetBool(AnimType.Walk_Anim.ToString()) || anim.GetBool(AnimType.Roll_Anim.ToString())) && Input.GetKey(KeyCode.A))
		{
			currentRot.y -= rotSpeed * Time.fixedDeltaTime;
		}
		else if (!controlledRot.isControlledRotating && !anim.GetBool(AnimType.Walk_Anim.ToString()) && Input.GetKey(KeyCode.A))
		{
			anim.SetBool(AnimType.Turn_L_Anim.ToString(), true);
			// Closed Turns
			if (!anim.GetBool(AnimType.Open_Anim.ToString()))
			{
				if (isLargeTurn)
					controlledRot.Set(isControlledRotating: true, initAngle: currentRot.y, angle: 90f, direction: -1, delay: 1.04f, timeStarted: Time.time, targetTime: 0.5f, afterDelay: 0.5f, animatorBoolName: AnimType.Turn_L_Anim.ToString());
				else
					controlledRot.Set(isControlledRotating: true, initAngle: currentRot.y, angle: 25f, direction: -1, delay: 0.25f, timeStarted: Time.time, targetTime: 0.2f, afterDelay: 0.45f, animatorBoolName: AnimType.Turn_L_Anim.ToString());
			}
			// Open Turns
			else
			{
				if (isLargeTurn)
					controlledRot.Set(isControlledRotating: true, initAngle: currentRot.y, angle: 90f, direction: -1, delay: 0.83f, timeStarted: Time.time, targetTime: 0.42f, afterDelay: 0.42f, animatorBoolName: AnimType.Turn_L_Anim.ToString());
				else
				controlledRot.Set(isControlledRotating: true, initAngle: currentRot.y, angle: 25f, direction: -1, delay: 0.25f, timeStarted: Time.time, targetTime: 0.2f, afterDelay: 0.45f, animatorBoolName: AnimType.Turn_L_Anim.ToString());
			}
		}

		// Rotate Right
		if ((anim.GetBool(AnimType.Walk_Anim.ToString()) || anim.GetBool(AnimType.Roll_Anim.ToString())) &&  Input.GetKey(KeyCode.D))
		{
			currentRot.y += rotSpeed * Time.fixedDeltaTime;
		}
		else if (!controlledRot.isControlledRotating && !anim.GetBool(AnimType.Walk_Anim.ToString()) && Input.GetKey(KeyCode.D))
		{
			anim.SetBool(AnimType.Turn_R_Anim.ToString(), true);
			// Closed Turn
			if (!anim.GetBool(AnimType.Open_Anim.ToString()))
			{
				if (isLargeTurn)
					controlledRot.Set(isControlledRotating: true, initAngle: currentRot.y, angle: 90f, direction: 1, delay: 1.04f, timeStarted: Time.time, targetTime: 0.5f, afterDelay: 0.5f, animatorBoolName: AnimType.Turn_R_Anim.ToString());
				else
					controlledRot.Set(isControlledRotating: true, initAngle: currentRot.y, angle: 25f, direction: 1, delay: 0.25f, timeStarted: Time.time, targetTime: 0.2f, afterDelay: 0.45f, animatorBoolName: AnimType.Turn_R_Anim.ToString());
			}
			// Open Turn
			else
			{
				if (isLargeTurn)
					controlledRot.Set(isControlledRotating: true, initAngle: currentRot.y, angle: 90f, direction: 1, delay: 0.83f, timeStarted: Time.time, targetTime: 0.42f, afterDelay: 0.42f, animatorBoolName: AnimType.Turn_R_Anim.ToString());
				else
					controlledRot.Set(isControlledRotating: true, initAngle: currentRot.y, angle: 25f, direction: 1, delay: 0.25f, timeStarted: Time.time, targetTime: 0.2f, afterDelay: 0.45f, animatorBoolName: AnimType.Turn_R_Anim.ToString());
			}
		}

		if (jumpState == JumpState.Grounded)
		{

			// Roll
			if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				if (anim.GetBool(AnimType.Roll_Anim.ToString()))
				{
					canMove = true;
					anim.SetBool(AnimType.Roll_Anim.ToString(), false);
				}
				else
				{
					anim.SetBool(AnimType.Roll_Anim.ToString(), true);
					rollingStartTime = Time.time;
					canMove = false;
				}
			}

			// Close
			else  if(Input.GetKeyDown(KeyCode.LeftControl)){
				if (!anim.GetBool(AnimType.Open_Anim.ToString()))
				{
					anim.SetBool(AnimType.Open_Anim.ToString(), true);
					StartCoroutine(WaitCoroutine(openDelay, onComplete: () =>
					{
						canMove = true;
					}));
				} 
				else
				{
					anim.SetBool(AnimType.Open_Anim.ToString(), false);
					canMove = false;
				}
			}

			// Jump
			else if (anim.GetBool(AnimType.Open_Anim.ToString()) && Input.GetKeyDown(KeyCode.Space))
			{
				jumpState = JumpState.Delay;
				jumpStartTime = Time.time;
				jumpStartPosY = gameObject.transform.position.y;
				anim.SetBool("Jump_Anim", true);
				canMove = false;
				Debug.Log("Start Jump");
				return;
			}
		}
	}

	#region Rotation
	void RotateCharacter()
	{
		ControlledRotation();
		if (currentRot == prevRot)
			return;
		currentRot.y = ClampRotation(currentRot.y);
		gameObject.transform.eulerAngles = currentRot;
		prevRot = currentRot;
	}

	void ControlledRotation()
	{
		if (controlledRot.isControlledRotating)
		{
			float time = Time.time - controlledRot.timeStarted;
			if (controlledRot.stage == ControlledRotationData.Stage.Delay && time >= controlledRot.delay)
			{
				controlledRot.timeStarted = Time.time;
				controlledRot.stage = ControlledRotationData.Stage.Rotating;
				anim.SetBool(controlledRot.animatorBoolName, false);
			}
			else if (controlledRot.stage == ControlledRotationData.Stage.Rotating && time < controlledRot.targetTime)
			{
				currentRot.y = controlledRot.initAngle + (((time / controlledRot.targetTime) * controlledRot.angle) * controlledRot.direction);
			}
			else if (controlledRot.stage == ControlledRotationData.Stage.Rotating && time >= controlledRot.targetTime)
			{
				controlledRot.timeStarted = Time.time;
				controlledRot.stage = ControlledRotationData.Stage.AfterDelay;
			}
			else if(controlledRot.stage == ControlledRotationData.Stage.AfterDelay && time >= controlledRot.afterDelay)
			{
				controlledRot.isControlledRotating = false;
			}
		}
	}

	float ClampRotation(float rot)
	{
		if (rot < 0f)
		{
			rot = 360f - rot;
		}
		else if (rot > 360f)
		{
			rot -= 360f;
		}
		return rot;
	}
	#endregion


	#region TranslationAndRoll
	void TranslateCharacter()
	{
		if (anim.GetBool(AnimType.Roll_Anim.ToString()) && Time.time - rollingStartTime >= rollingDelay)
		{
			UpdateMovement(rollingSpeed);
		}
		else if (currentPos == prevPos)
			return;
		gameObject.transform.position = currentPos;
	}

	void UpdateMovement(float speed)
	{
		Vector2 speedOnAxis = CalculateMovementSpeedPercentage(currentRot.y);
		currentPos.z += (speed * speedOnAxis.x) * Time.fixedDeltaTime;
		currentPos.x += (speed * speedOnAxis.y) * Time.fixedDeltaTime;
	}

	Vector2 CalculateMovementSpeedPercentage(float facingRotation)
	{
		Vector2 result = Vector2.zero;
		result.x = Mathf.Cos(Mathf.Deg2Rad * facingRotation);
		result.y = Mathf.Sin(Mathf.Deg2Rad * facingRotation);
		return result;
	}
	#endregion

	#region Jump
	void JumpChatacter()
	{
		if (jumpState == JumpState.Grounded || jumpState == JumpState.Delay && Time.time - jumpStartTime <= jumpDelay)
			return;
		if (jumpState == JumpState.Delay && Time.time - jumpStartTime > jumpDelay)
		{
			jumpState = JumpState.Jump;
			jumpStartTime = Time.time;
			canMove = true;
		}
		currentPos.y = (jumpStartPosY + jumpHight) * CalculateJumpPercenatge();
	}

	float CalculateJumpPercenatge()
	{
		float f;
		if (!jumpState.Equals(JumpState.Land))
		{
			f = ((Time.time - jumpStartTime) / (jumpDuration / 2f)) * jumpHight;
			f = Mathf.Clamp(f, 0f, 1f);
		}
		else
			f = 1;

		if (jumpState == JumpState.Jump)
		{
			f = Easing.EaseOutSine(0f, 1f, f);
			if (f == 1f)
			{
				jumpState = JumpState.Fall;
				jumpStartTime = Time.time;
			}
		}
		else if (jumpState == JumpState.Fall || jumpState == JumpState.Land)
		{
			f = 1 - f;
			f = Easing.EaseOutSine(0f, 1f, f);
			if (jumpState == JumpState.Fall && f <= 0.2f)
			{
				jumpState = JumpState.Land;
				canMove = false;
				anim.SetBool("Jump_Anim", false);
				jumpStartTime = Time.time;
			}
			else if (jumpState == JumpState.Land && f == 0)
			{
				if(Time.time - jumpStartTime >= landDelay)
				{
					canMove = true;
					jumpState = JumpState.Grounded;
				}
			}
		}
		else
		{
			Debug.LogWarning(string.Format("Invalid Jump State: {0}", jumpState));
		}
		return f;
	}
	#endregion

	IEnumerator WaitCoroutine(float time, System.Action onComplete)
	{
		yield return new WaitForSeconds(time);
		onComplete();
	}

}

public class ControlledRotationData
{
	public enum Stage { Delay, Rotating, AfterDelay}

	public bool isControlledRotating;
	public float initAngle;
	public float angle;
	public int direction;
	public float delay;
	public float afterDelay;
	public float timeStarted;
	public float targetTime;
	public string animatorBoolName;
	public Stage stage = Stage.Delay;

	public void Set(bool isControlledRotating, float initAngle, float angle, int direction, float delay, float timeStarted, float targetTime, float afterDelay, string animatorBoolName)
	{
		this.isControlledRotating = isControlledRotating;
		this.initAngle = initAngle;
		this.angle = angle;
		this.direction = direction;
		this.delay = delay;
		this.timeStarted = timeStarted;
		this.targetTime = targetTime;
		this.afterDelay = afterDelay;
		this.animatorBoolName = animatorBoolName;
		stage = Stage.Delay;
	}
}

public class Easing
{
	public static float EaseInSine(float start, float end, float value)
	{
		end -= start;
		return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
	}

	public static float EaseOutSine(float start, float end, float value)
	{
		end -= start;
		return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
	}
};
