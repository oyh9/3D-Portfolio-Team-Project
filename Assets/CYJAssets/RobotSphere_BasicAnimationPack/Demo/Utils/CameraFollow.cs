using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
	[SerializeField]
	Transform target;
	Vector3 lastTargetPos;

	Vector3 distancePos;
	Quaternion initRot;

	void Awake()
	{
		initRot = transform.rotation;
		lastTargetPos = target.position;
		distancePos = transform.position - lastTargetPos;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(target.position != lastTargetPos)
		{
			lastTargetPos = target.position;
			Vector3 newPos = distancePos;
			newPos.x += target.position.x;
			newPos.z += target.position.z;
			transform.position = newPos;
		}
	}
}
